
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



[RequireComponent (typeof(PlayerInputController))]
[RequireComponent (typeof(PlayerMovement))]
public class Player : MonoBehaviour
{

	public LayerMask PlatformLayerMask;

	public int numberOfGroundCheckRays;
	public int numberOfWallCheckRays;
	public float skinDepth;
	public int touchesWall;
	public float healthPoints;
	public Slider healthBar;
	public float hitImmunityTime;
	public LayerMask enemyLayerMask;
	public Slider blinkBar;
	public Vector2 startingPosition;
	[HideInInspector]
	public float
		lastGroundedLevel;
	[HideInInspector]
	public bool
		shouldMove = false;
	[HideInInspector]
	public bool
		shouldRun = false;
	[HideInInspector]
	public bool
		shouldJump = false;
	[HideInInspector]
	public bool
		grounded = false;
	[HideInInspector]
	public bool
		doWallJump = false;
	[HideInInspector]
	public bool
		knockBack = false;
	[HideInInspector]
	public bool
		jumpKeyPressed = false;
	[HideInInspector]
	public bool
		shouldBlink = false;

	public BoxCollider2D levelboundsCollider;

	PlayerInputController playerInputController;
	PlayerMovement playerMovement;
	Rigidbody2D body;
	BoxCollider2D clldr;
	public Animator anim;
	PlayerAttack playerAttack;
	bool jumpFinished = true;
	float lastKnownVelocityX;
	List<Vector3> groundCheckRayOffsets = new List<Vector3> ();
	[HideInInspector]
	public List<Vector3>
		wallCheckRayOffsets = new List<Vector3> ();
	float lastBlinkTime;
	float initialJumpVelocity;
	float wallTouchCheckRaycastDistance;
	float groundCheckRaycastDistance;
	float lastHitTime;
	float currentHealthPoints;
	bool shouldAttack = false;
	float lastAttackTime;


	// debug
	[HideInInspector]
	public Vector2
		oldPos;


	void Start ()
	{
		playerInputController = GetComponent<PlayerInputController> ();
		playerMovement = GetComponent<PlayerMovement> ();
		body = GetComponent<Rigidbody2D> ();
		clldr = GetComponent<BoxCollider2D> ();
		anim = GetComponent<Animator> ();
		playerAttack = GetComponent<PlayerAttack> ();

		touchesWall = 0;
		lastBlinkTime = Time.time - playerMovement.blinkCoolDown;
		lastAttackTime = Time.time - lastAttackTime;

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
		anim.SetBool ("Grounded", grounded);
		touchesWall = !grounded ? isTouchingWall () : 0;
		Debug.Log (touchesWall != 0 ? true : false);
		anim.SetBool("TouchesWall", touchesWall != 0 ? true : false);
		if (shouldBlink) {
			playerMovement.DoBlink ();
		} else if (knockBack) {
			playerMovement.KnockBack ();
		} else {
			playerMovement.MovePlayer ();
		}

		if (shouldAttack) {
			playerAttack.Attack ();
		}


		float timeSinceBlink = Time.time - lastBlinkTime;
		if (timeSinceBlink < playerMovement.blinkCoolDown) {
			blinkBar.value = 1 / playerMovement.blinkCoolDown * timeSinceBlink;
		}

		EnableEnemiesInRange ();

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


		if (playerInputController.running || playerInputController.runToggle) {
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
		
		if (playerInputController.blinking && ((lastBlinkTime + playerMovement.blinkCoolDown) < Time.time)) {
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
					playerMovement.knockBackDirection = 1;
				} else {
					playerMovement.knockBackDirection = -1;
				}
				
				if ((transform.position.y - clldr.bounds.extents.y) > 
					(coll.transform.position.y + coll.collider.bounds.extents.y)) {
					playerMovement.knockBackDirection = 0;
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

	void EnableEnemiesInRange ()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll (transform.position, 36.0f, enemyLayerMask);
		foreach (Collider2D collider in colliders) {
			collider.GetComponentInChildren<EnemyMovement> ().EnablePing ();
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