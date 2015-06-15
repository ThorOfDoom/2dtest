using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[RequireComponent (typeof(PlayerInputController))]
public class Player : MonoBehaviour
{
	public float walkVelocity;
	public float runVelocity;
	public float runAcceleration;
	public LayerMask PlatformLayerMask;
	public float jumpHeight;
	public float slideReductionMultiplier;
	public float airSlideReductionMultiplier;
	public int numberOfGroundCheckRays;
	public int numberOfWallCheckRays;
	public float skinDepth;
	public int touchesWall;
	public float wallJumpTime;
	public float blinkDistance;
	public LayerMask blinkLayerMask;
	public float blinkCoolDown;
	public float healthPoints;
	public Slider healthBar;
	public float hitImmunityTime;
	public Vector2 knockBackDistance;
	public LayerMask enemyLayerMask;
	public Slider blinkBar;
	public Vector2 startingPosition;
	[HideInInspector]
	public float
		lastGroundedLevel;

	public BoxCollider2D levelboundsCollider;

	PlayerInputController playerInputController;
	Rigidbody2D body;
	BoxCollider2D clldr;
	Animator anim;
	PlayerAttack playerAttack;

	bool shouldMove = false;
	bool shouldRun = false;
	Vector2 velocity = new Vector2 ();
	bool grounded = false;
	bool shouldJump = false;
	float airTime = 0.0f;
	bool isJumping = false;
	bool jumpFinished = true;
	bool didMove = false;
	float lastKnownVelocityX;
	List<Vector3> groundCheckRayOffsets = new List<Vector3> ();
	List<Vector3> wallCheckRayOffsets = new List<Vector3> ();
	bool doWallJump = false;
	int wallJumpDirection = 0;
	bool jumpKeyPressed = false;
	bool shouldBlink = false;
	bool facingRight = true;
	float lastBlinkTime;
	float initialJumpVelocity;
	float wallTouchCheckRaycastDistance;
	float groundCheckRaycastDistance;
	float lastHitTime;
	float currentHealthPoints;
	bool knockBack = false;
	int knockBackDirection;
	Vector2 knockBackForce;
	float knockBackAirTime = 0.0f;
	Vector2 knockBackVelocity = new Vector2 ();
	bool shouldAttack = false;
	float lastAttackTime;
	Dictionary<int, float> hitEnemies = new Dictionary<int, float> ();

	// debug
	Vector2 oldPos;


	void Start ()
	{
		playerInputController = GetComponent<PlayerInputController> ();
		body = GetComponent<Rigidbody2D> ();
		clldr = GetComponent<BoxCollider2D> ();
		anim = GetComponent<Animator> ();
		playerAttack = GetComponent<PlayerAttack> ();

		touchesWall = 0;
		lastBlinkTime = Time.time - blinkCoolDown;
		lastAttackTime = Time.time - lastAttackTime;
		initialJumpVelocity = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpHeight);
		knockBackForce = new Vector2 (Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance.x),
		                              Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance.y));
		groundCheckRaycastDistance = (clldr.bounds.extents.y + skinDepth);
		wallTouchCheckRaycastDistance = (clldr.bounds.extents.x + skinDepth);
		lastHitTime = Time.time;
		startingPosition = transform.position;
		currentHealthPoints = healthPoints;
		healthBar.maxValue = healthPoints;
		healthBar.value = currentHealthPoints;
		blinkBar.value = 1.0f;

		InitializeRays ();
	}


	void FixedUpdate ()
	{
		CheckIfOutsideBounds ();

		grounded = isGrounded ();
		touchesWall = !grounded ? isTouchingWall () : 0;

		if (shouldBlink) {
			DoBlink ();
		} else if (knockBack) {
			KnockBack ();
		} else {
			MovePlayer ();
		}

		if (shouldAttack) {
			Attack ();
		}
		UpdateHitEnemies ();

		float timeSinceBlink = Time.time - lastBlinkTime;
		if (timeSinceBlink < blinkCoolDown) {
			blinkBar.value = 1 / blinkCoolDown * timeSinceBlink;
		}

		oldPos = body.position;
	}
    

	void Update ()
	{
		CheckInputs ();
	}

	
	void CheckInputs ()
	{
		if (playerInputController.moving != 0) {
			shouldMove = true;
		} else {
			shouldMove = false;
		}
		
		if ((playerInputController.running && shouldMove) || playerInputController.runToggle) {
			shouldRun = true;
		} else {
			shouldRun = false;
		}
		
		if (playerInputController.jumping == 1) {
			if (jumpFinished && grounded) {
				shouldJump = true;
				jumpFinished = false;
			} else {
				jumpKeyPressed = true;
			}
		}
		
		if (playerInputController.jumping == -1) {
			shouldJump = false;
			jumpFinished = true;
			jumpKeyPressed = false;
		}
		
		if (playerInputController.blinking && ((lastBlinkTime + blinkCoolDown) < Time.time)) {
			shouldBlink = true;
			lastBlinkTime = Time.time;
			blinkBar.value = 0.0f;
		}

		if (playerInputController.attacking) {
			shouldAttack = true;
			anim.SetBool ("doHit", true);
		} else {
			shouldAttack = false;
			anim.SetBool ("doHit", false);
		}
	}


	void OnCollisionStay2D (Collision2D coll)
	{
		float hitTime = Time.time;

		if ((hitTime - hitImmunityTime) > lastHitTime) {
			if (coll.gameObject.tag == "Enemy") {
				knockBack = true;
				Enemy enemy = coll.gameObject.GetComponent<Enemy> ();
				TakeHit (enemy.damage);
				lastHitTime = hitTime;
				if (coll.transform.position.x < transform.position.x) {
					knockBackDirection = 1;
				} else {
					knockBackDirection = -1;
				}
				
				if ((transform.position.y - clldr.bounds.extents.y) > 
					(coll.transform.position.y + coll.collider.bounds.extents.y)) {
					knockBackDirection = 0;
				}
			}
		}
	}

	void OnCollisionEnter2D (Collision2D coll)
	{
		float hitTime = Time.time;
		if ((hitTime - hitImmunityTime) > lastHitTime && coll.gameObject.tag == "Spike") {
			Spike spike = coll.gameObject.GetComponentInParent<Spike> ();
			TakeHit (spike.damage);
			lastHitTime = hitTime;
		}
	}


	void Attack ()
	{
		RaycastHit2D hit = playerAttack.DoAttack ();
		if (hit.collider != null && !hitEnemies.ContainsKey (hit.collider.GetInstanceID ())) {
			Enemy enemy = hit.collider.GetComponent<Enemy> ();

			enemy.TakeHit (playerAttack.GetWeaponStats ().damage, hit.point.x);
			if (enemy.health > 0.0f) {
				hitEnemies.Add (hit.collider.GetInstanceID (), Time.time);
			}
		}
	}


	void UpdateHitEnemies ()
	{
		if (hitEnemies.Count != 0) {
			KeyValuePair<int, float>[] itemsToRemove = 
				hitEnemies.Where (f => f.Value < Time.time + playerAttack.GetWeaponStats ().attackSpeed).ToArray ();

			for (int i = 0; i < itemsToRemove.Length; i++) {
				hitEnemies.Remove (itemsToRemove [i].Key);
			}
		}
	}


	public void TakeHit (float damage)
	{
		ChangeHealth (-damage);
		if (currentHealthPoints <= 0) {
			Debug.Log ("you dead! :(");
			Die ();
		}
	}


	void ChangeHealth (float damage)
	{
		SetHealth (currentHealthPoints + damage);
	}


	void SetHealth (float health)
	{
		healthBar.value = currentHealthPoints = health;
	}


	void Die ()
	{
		transform.position = startingPosition;
		body.velocity = new Vector2 (0.0f, 0.0f);
		SetHealth (healthPoints);
		knockBack = false;
	}


	void KnockBack ()
	{
		knockBackAirTime += Time.deltaTime;
        
		knockBackVelocity.x = (knockBackForce.x + Physics2D.gravity.y * knockBackAirTime);
		knockBackVelocity.y = knockBackForce.y + Physics2D.gravity.y * knockBackAirTime;

		if (knockBackVelocity.x > 0.0f) {
			knockBackVelocity.x *= knockBackDirection;
			body.velocity = knockBackVelocity;
		} else {
			knockBack = false;
			knockBackAirTime = 0.0f;
			didMove = true;
		}

		Debug.DrawLine (oldPos, body.position, Color.green, 5.0f);
	}

	void MovePlayer ()
	{// TODO: split it up into seperat methods
		velocity.y = body.velocity.y;
		float absVelX = Mathf.Abs (velocity.x);
		// TODO feels hackish
		if (shouldMove && playerInputController.moving != 0) {
			if (shouldRun) {
				if ((absVelX + runAcceleration) <= runVelocity) {
					velocity.x = absVelX + runAcceleration;
				} else {
					velocity.x = runVelocity;
				}
			} else {
				velocity.x = walkVelocity;
			}
			velocity.x *= playerInputController.moving;

			if (velocity.x > 0) {
				facingRight = true;
			} else if (velocity.x < 0) {
				facingRight = false;
			}

			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1), 
			                                    transform.localScale.y, transform.localScale.z);
			didMove = true;
			lastKnownVelocityX = velocity.x;
		} else if (didMove) {
			if (Mathf.Abs (lastKnownVelocityX) > walkVelocity || !grounded) {
				lastKnownVelocityX *= !grounded ? airSlideReductionMultiplier : slideReductionMultiplier;
			} else {
				didMove = false;
				lastKnownVelocityX = 0.0f;
			}
			velocity.x = lastKnownVelocityX;
			Debug.DrawLine (oldPos, body.position, Color.green, 5.0f);
		}

		if (shouldJump && !isJumping && grounded && !doWallJump) {
			isJumping = true;
		} else if (isJumping && grounded && !doWallJump) {
			isJumping = false;
			shouldJump = false;
			airTime = 0.0f;
			velocity.y = 0.0f;
		} else if (doWallJump && (airTime > wallJumpTime)) {
			doWallJump = false;
			wallJumpDirection = 0;
			if (playerInputController.moving == 0) {
				velocity.x = 0.0f;
			}
			airTime = 0.0f;
		} else if ((playerInputController.jumping == 1 || jumpKeyPressed) && touchesWall != 0) {
			doWallJump = true;
			wallJumpDirection = -(touchesWall);
			airTime = 0.0f;
			jumpKeyPressed = false;
		} 

		if (isJumping) {
			airTime += Time.deltaTime;
			if (shouldJump) {
				velocity.y = initialJumpVelocity + Physics2D.gravity.y * airTime;
			} else if (touchesWall != 0) {
				isJumping = false;
				shouldJump = false;
				airTime = 0.0f;
			} else {
				velocity.y = velocity.y < 0 ? velocity.y : 0.0f;
			}
			Debug.DrawLine (oldPos, body.position, Color.red, 5.0f);
		} else if (doWallJump) {
			airTime += Time.deltaTime;
			velocity.y = initialJumpVelocity + Physics2D.gravity.y * airTime;
			velocity.x = walkVelocity * wallJumpDirection;
			transform.localScale = new Vector3 (
				Mathf.Abs (transform.localScale.x) * (body.velocity.x > 0 ? 1 : -1), 
				transform.localScale.y, 
				transform.localScale.z);
			didMove = true;
			lastKnownVelocityX = velocity.x;
			Debug.DrawLine (oldPos, body.position, Color.yellow, 5.0f);
		} else if (!grounded) {
			Debug.DrawLine (oldPos, body.position, Color.magenta, 5.0f);
		}
		
		body.velocity = velocity;
	}


	void DoBlink ()
	{
		float _blinkDistance = CheckBlinkDistance ();
		_blinkDistance *= (facingRight ? 1 : -1);
		transform.position += new Vector3 (_blinkDistance, 0.0f, 0.0f);
		oldPos = transform.position;
		shouldBlink = false;
	}


	float CheckBlinkDistance ()
	{
		float distance = blinkDistance;
		Vector3 skinDepth = new Vector3 (transform.localScale.x / 2, 0.0f, 0.0f);
		Vector3 blinkRayCastOrigin = transform.position + skinDepth;
		Vector2 blinkRayCastDirection = (facingRight ? Vector2.right : -Vector2.right);

		for (int i = 0; i < numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast (
				(blinkRayCastOrigin + wallCheckRayOffsets [i]), 
				blinkRayCastDirection, 
				distance, 
				blinkLayerMask);
			if (hit.collider != null) {
				distance = hit.distance;
			}
		}

		return Mathf.Round (distance * 10) / 10.0f;
	}


	bool isGrounded ()
	{
		for (int i = 0; i < numberOfGroundCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + groundCheckRayOffsets [i]), 
			                                      -Vector2.up, groundCheckRaycastDistance, PlatformLayerMask);
			if (hit.collider != null) {
				lastGroundedLevel = transform.position.y;
				return true;
			}
		}
        
		return false;
	}


	int isTouchingWall ()
	{	// returns -1 if touching left | 0 if touching noWall | 1 if touchign right
		return isTouchingLeftWall () ? -1 : (isTouchingRightWall () ? 1 : 0);
	}


	bool isTouchingRightWall ()
	{
		for (int i = 0; i < numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + wallCheckRayOffsets [i]), 
			                                      Vector2.right, wallTouchCheckRaycastDistance, PlatformLayerMask);
			if (hit.collider != null) {
				return true;
			}
		}

		return false;
	}
    

	bool isTouchingLeftWall ()
	{
		for (int i = 0; i < numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + wallCheckRayOffsets [i]), 
			                                      -Vector2.right, wallTouchCheckRaycastDistance, PlatformLayerMask);
			if (hit.collider != null) {
				return true;
			}
		}
		
		return false;
	}
    

	void InitializeRays ()
	{
		CalculateGroundCheckRayPositions ();
		CalculateWallCheckRayPositions ();
	}


	void CalculateGroundCheckRayPositions ()
	{
		groundCheckRayOffsets.Clear ();
		float extentsWithoutskinDepth = skinDepth - clldr.bounds.extents.x;
		float distanceBetweenRays = (clldr.bounds.size.x - 2 * skinDepth) / (numberOfGroundCheckRays - 1);
		Vector3 currentOffset = new Vector3 (0.0f, 0.0f, 0.0f);

		for (int i = 0; i < numberOfGroundCheckRays; i++) {
			currentOffset.x = extentsWithoutskinDepth + i * distanceBetweenRays;
			groundCheckRayOffsets.Add (currentOffset);
		}
	}


	void CalculateWallCheckRayPositions ()
	{
		wallCheckRayOffsets.Clear ();
		float extentsWithoutskinDepth = skinDepth - clldr.bounds.extents.y;
		float distanceBetweenRays = (clldr.bounds.size.y - 2 * skinDepth) / (numberOfWallCheckRays - 1);
		Vector3 currentOffset = new Vector3 (0.0f, 0.0f, 0.0f);

		for (int i = 0; i < numberOfWallCheckRays; i++) {
			currentOffset.y = extentsWithoutskinDepth + i * distanceBetweenRays;
			wallCheckRayOffsets.Add (currentOffset);
		}
	}


	void CheckIfOutsideBounds ()
	{
		if ((clldr.bounds.max.y + 3.0f) < (-levelboundsCollider.size.y / 2 + levelboundsCollider.offset.y)) {
			Die ();
		}
	}
}