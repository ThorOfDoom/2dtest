using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * TODO:
 * refactor & optimize! http://www.gamasutra.com/blogs/AmirHFassihi/20130828/199134/0__60_fps_in_14_days_What_we_learned_trying_to_optimize_our_game_using_Unity3D.php
 * go over the enemy again
 */

[RequireComponent (typeof(PlayerInputController))]
public class Player : MonoBehaviour
{
	public float walkVelocity;
	public float runVelocity;
	public float runAcceleration;
	public Transform groundCheck;
	public LayerMask PlatformLayerMask;
	public bool grounded = false;
	public float jumpStrength = 10.0f;
	public float maxAirTime = 0.5f;
	public float jumpHeight = 3.0f;
	public float slideReductionMultiplier = 0.93f;
	public float airSlideReductionMultiplier = 0.80f;
	public int numberOfGroundCheckRays = 3;
	public int numberOfWallCheckRays = 3;
	public float skinDepth = 0.05f;
	public int touchesWall = 0;
	public float wallHangingGravityScale = 0.3f;
	public float wallJumpTime = 0.4f;
	public float lastGroundedLevel;
	public float blinkDistance;
	public LayerMask blinkLayerMask;
	public float blinkCoolDown;
	public float healthPoints;
	public float hitImmunityTime;
	public Vector2 knockBackDistance;

	PlayerInputController playerInputController;
	Rigidbody2D body;
	BoxCollider2D collider;

	private bool shouldMove = false;
	private bool shouldRun = false;
	private Vector2 velocity = new Vector2 ();
	private bool shouldJump = false;
	private float groundRadius = 0.1f;
	private float airTime = 0.0f;
	private bool isJumping = false;
	private bool jumpFinished = true;
	private bool didMove = false;
	private float lastKnownVelocityX;
	private List<Vector3> groundCheckRayOffsets = new List<Vector3> ();
	private List<Vector3> wallCheckRayOffsets = new List<Vector3> ();
	public bool doWallJump = false;
	private int wallJumpDirection = 0;
	private bool jumpKeyPressed = false;
	private bool shouldBlink = false;
	private bool facingRight = true;
	private float lastBlinkTime;
	private float initialJumpVelocity;
	private float wallTouchCheckRaycastDistance;
	private float groundCheckRaycastDistance;
	private float lastHitTime;
	private Vector3 startingPosition;
	private float currentHealthPoints;
	private bool knockBack = false;
	private float knockBackTime;
	private int knockBackDirection;
	private Vector2 knockBackForce;
	private float knockBackAirTime = 0.0f;
	private Vector2 knockBackVelocity = new Vector2 ();

	// debug
	public float lastVelocityX;
	private Vector2 oldPos;
	private int lastWallTouch = 0;

	// Use this for initialization
	void Start ()
	{
		playerInputController = GetComponent<PlayerInputController> ();
		body = GetComponent<Rigidbody2D> ();
		collider = GetComponent<BoxCollider2D> ();

		lastBlinkTime = Time.time - blinkCoolDown;

		InitializeRays ();

		initialJumpVelocity = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpHeight);
		knockBackForce = new Vector2 (Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance.x), Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance.y));
		groundCheckRaycastDistance = (collider.bounds.extents.y + skinDepth);
		wallTouchCheckRaycastDistance = (collider.bounds.extents.x + skinDepth);
		lastHitTime = Time.time;
		startingPosition = transform.position;
		currentHealthPoints = healthPoints;
		//Debug.Log (Mathf.Sqrt (2 * Mathf.Abs (Physics2D.gravity.y) * jumpHeight));
		//Debug.Log (Physics2D.gravity.y);
	}

	void FixedUpdate ()
	{
		grounded = isGrounded ();
		touchesWall = !grounded ? isTouchingWall () : 0;

		if (shouldBlink) {
			DoBlink ();
		} else if (knockBack) {
			KnockBack ();
		} else {
			MovePlayer ();
		}
		oldPos = body.position;
	}
    
	void Update ()
	{
		CheckInputs ();
	}
	/*
	void OnDrawGizmos ()
	{
		Gizmos.color = Color.blue;
		Vector3 cubePosition = transform.position;
		Vector3 cubeSize = new Vector3 (0.2f, 0.2f, 0.2f);

		cubePosition.x += facingRight ? 0.4f : -0.4f;

		Gizmos.DrawWireCube (cubePosition, cubeSize);
	}*/
	

	void OnCollisionStay2D (Collision2D coll)
	{
		float hitTime = Time.time;

		if ((hitTime - hitImmunityTime) > lastHitTime && coll.gameObject.tag == "Enemy") {
			knockBackTime = Time.time;
			knockBack = true;
			Enemy enemy = coll.gameObject.GetComponent<Enemy> ();
			TakeHit (enemy.damage);
			lastHitTime = hitTime;
			if (coll.transform.position.x < transform.position.x) {
				knockBackDirection = 1;
			} else {
				knockBackDirection = -1;
			}
			Debug.Log (transform.position.y - collider.bounds.extents.y + " | " + coll.transform.position.y + coll.collider.bounds.extents.y);

			if ((transform.position.y - collider.bounds.extents.y) > (coll.transform.position.y + coll.collider.bounds.extents.y)) {
				knockBackDirection = 0;
			}
		}
	}

	public void TakeHit (float damage)
	{
		Debug.Log ("hit for " + damage + " points of damage");
		currentHealthPoints -= damage;
		Debug.Log (currentHealthPoints + " Health Points remaining");
		if (currentHealthPoints <= 0) {
			Debug.Log ("you dead! :(");
			Die ();
		}
	}

	void Die ()
	{
		transform.position = startingPosition;
		body.velocity = new Vector2 (0.0f, 0.0f);
		currentHealthPoints = healthPoints;
		knockBack = false;
	}

	void CheckInputs ()
	{
		//check if we should move (left/right)
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
		}
	}

	void KnockBack ()
	{
		knockBackAirTime += Time.deltaTime;
        
		knockBackVelocity.x = (knockBackForce.x + Physics2D.gravity.y * knockBackAirTime);
		knockBackVelocity.y = knockBackForce.y + Physics2D.gravity.y * knockBackAirTime;
//		Debug.Log (knockBackVelocity);
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
		//velocity.x = 0.0f;
		float absVelX = Mathf.Abs (velocity.x);
		// TODO this is just weird -> feels hackish
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

			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1), transform.localScale.y, transform.localScale.z);


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
		} else if (isJumping && grounded && !doWallJump/* && airTime > 0.1f*/) {// TODO check if airtime value needs adjustment
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
			if (shouldJump) {// TODO move the calculation to the top so it is not calculated all the time ;)
				velocity.y = initialJumpVelocity + Physics2D.gravity.y * airTime;
			} else if (touchesWall != 0) {
				isJumping = false;
				shouldJump = false;
				airTime = 0.0f;
				//velocity.y = 0.0f;
			} else {
				//velocity.y = Physics2D.gravity.y * (1 / maxAirTime * airTime);
				//isJumping = false;
				//shouldJump = false;
				velocity.y = velocity.y < 0 ? velocity.y : 0.0f;
				//airTime = 0.0f;
			}
			Debug.DrawLine (oldPos, body.position, Color.red, 5.0f);
		} else if (doWallJump) {
			airTime += Time.deltaTime;
			velocity.y = initialJumpVelocity + Physics2D.gravity.y * airTime;
			velocity.x = walkVelocity * wallJumpDirection;
			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (body.velocity.x > 0 ? 1 : -1), transform.localScale.y, transform.localScale.z);
			didMove = true;
			lastKnownVelocityX = velocity.x;
			Debug.DrawLine (oldPos, body.position, Color.yellow, 5.0f);
		} else if (!grounded) {
			Debug.DrawLine (oldPos, body.position, Color.magenta, 5.0f);
		}

		/*
		if (!grounded && touchesWall != 0) {
			//body.gravityScale = wallHangingGravityScale;
		} else { 
			body.gravityScale = 1.0f;
		}*/


		//Debug.Log (velocity.x);
		body.velocity = velocity;
		//lastVelocity = velocity;
		//oldPos = body.position;
		lastVelocityX = velocity.x;
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

		/*
		foreach (Vector3 rayOffset in wallCheckRayOffsets) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset + skinDepth), (facingRight ? Vector2.right : -Vector2.right), distance, blinkLayerMask);
			if (hit.collider != null) {
				distance = hit.distance;
			}
			//Debug.DrawRay ((transform.position + rayOffset + skinDepth), ((facingRight ? Vector2.right : -Vector2.right) * distance), Color.blue, 1.0f);
		}
		*/
		Vector3 blinkRayCastOrigin = transform.position + skinDepth;
		Vector2 blinkRayCastDirection = (facingRight ? Vector2.right : -Vector2.right);

		for (int i = 0; i < numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((blinkRayCastOrigin + wallCheckRayOffsets [i]), blinkRayCastDirection, distance, blinkLayerMask);
			if (hit.collider != null) {
				distance = hit.distance;
			}
		}

		return Mathf.Round (distance * 10) / 10.0f;
	}

	bool isGrounded ()
	{/*
		// TODO remeber that this is weird for debug reasosn aka. not optimised :)
		bool isGrounded = false;
		foreach (Vector3 rayOffset in groundCheckRayOffsets) {
			if (!isGrounded) {
				RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset), -Vector2.up, (collider.bounds.extents.y + 0.1f), PlatformLayerMask);
				if (hit.collider != null) {
					isGrounded = true;
				}
			}
			//Debug.DrawRay ((transform.position + rayOffset), new Vector2 (0.0f, -(collider.bounds.extents.y + 0.1f)), Color.red);
		}

		if (isGrounded) {
			lastGroundedLevel = Mathf.Floor (transform.position.y);
		}
		*/


		for (int i = 0; i < numberOfGroundCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + groundCheckRayOffsets [i]), -Vector2.up, groundCheckRaycastDistance, PlatformLayerMask);
			if (hit.collider != null) {
				lastGroundedLevel = Mathf.Floor (transform.position.y);
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
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + wallCheckRayOffsets [i]), Vector2.right, wallTouchCheckRaycastDistance, PlatformLayerMask);
			if (hit.collider != null) {
				return true;
			}
			//Debug.DrawRay ((transform.position + wallCheckRayOffsets [i]), new Vector2 ((collider.bounds.extents.x + 0.1f), 0.0f), Color.green);
		}

		return false;
	}
    
	bool isTouchingLeftWall ()
	{
        
		for (int i = 0; i < numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + wallCheckRayOffsets [i]), -Vector2.right, wallTouchCheckRaycastDistance, PlatformLayerMask);
			if (hit.collider != null) {
				return true;
			}
			//Debug.DrawRay ((transform.position + wallCheckRayOffsets [i]), new Vector2 (-(collider.bounds.extents.x + 0.1f), 0.0f), Color.green);
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
		float extentsWithoutskinDepth = skinDepth - collider.bounds.extents.x;
		float distanceBetweenRays = (collider.bounds.size.x - 2 * skinDepth) / (numberOfGroundCheckRays - 1);
		Vector3 currentOffset = new Vector3 (0.0f, 0.0f, 0.0f);

		for (int i = 0; i < numberOfGroundCheckRays; i++) {
			currentOffset.x = extentsWithoutskinDepth + i * distanceBetweenRays;
			groundCheckRayOffsets.Add (currentOffset);
		}
		/*
		Debug.Log("ground check ray offsets:");
		foreach (Vector3 rayOffset in groundCheckRayOffsets) {
			Debug.Log (rayOffset.x);
		}
		 */
	}
	
	void CalculateWallCheckRayPositions ()
	{
		wallCheckRayOffsets.Clear ();
		float extentsWithoutskinDepth = skinDepth - collider.bounds.extents.y;
		float distanceBetweenRays = (collider.bounds.size.y - 2 * skinDepth) / (numberOfWallCheckRays - 1);
		Vector3 currentOffset = new Vector3 (0.0f, 0.0f, 0.0f);

		for (int i = 0; i < numberOfWallCheckRays; i++) {
			currentOffset.y = extentsWithoutskinDepth + i * distanceBetweenRays;
			wallCheckRayOffsets.Add (currentOffset);
		}
		/*
		Debug.Log ("wall check ray offsets:");
		foreach (Vector3 rayOffset in wallCheckRayOffsets) {
			Debug.Log (rayOffset.y);
		}
		*/
	}







}