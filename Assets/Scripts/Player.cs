using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * TODO:
 * make wall jump
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

	PlayerInputController playerInputController;
	Rigidbody2D body;
	BoxCollider2D collider;

	private bool shouldMove = false;
	private bool shouldRun = false;
	private Vector2 velocity;
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

	// debug
	public float lastVelocityX;
	private Vector2 oldPos;
	private int lastWallTouch = 0;

	// Use this for initialization
	void Start ()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		playerInputController = GetComponent<PlayerInputController> ();
		body = GetComponent<Rigidbody2D> ();
		collider = GetComponent<BoxCollider2D> ();

		InitializeRays ();

		Debug.Log (Mathf.Sqrt (2 * Mathf.Abs (Physics2D.gravity.y) * jumpHeight));
		Debug.Log (Physics2D.gravity.y);
	}

	void FixedUpdate ()
	{
		grounded = isGrounded ();
		touchesWall = !grounded ? isTouchingWall () : 0;
		/*if (lastWallTouch != touchesWall) {
			lastWallTouch = touchesWall;
			Debug.Log (touchesWall);
		}*/
		MovePlayer ();

	}

	void Update ()
	{
		CheckInputs ();
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.white;
		//Gizmos.DrawWireSphere (groundCheck.position, groundRadius);
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
	}

	void MovePlayer ()
	{// TODO: split it up into seperat methods
		velocity = body.velocity;
		float absVelX = Mathf.Abs (body.velocity.x);
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
			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (body.velocity.x > 0 ? 1 : -1), transform.localScale.y, transform.localScale.z);
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


		if (shouldJump && !isJumping && (grounded) && !doWallJump) {
			isJumping = true;
		} else if (isJumping && (grounded) && !doWallJump) {
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
				velocity.y = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpHeight) + Physics2D.gravity.y * airTime;
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
			velocity.y = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpHeight) + Physics2D.gravity.y * airTime;
			velocity.x = walkVelocity * wallJumpDirection;
			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (body.velocity.x > 0 ? 1 : -1), transform.localScale.y, transform.localScale.z);
			didMove = true;
			lastKnownVelocityX = velocity.x;
			Debug.DrawLine (oldPos, body.position, Color.yellow, 5.0f);
		} else if (!grounded) {
			Debug.DrawLine (oldPos, body.position, Color.magenta, 5.0f);
		}


		if (!grounded && touchesWall != 0) {
			//body.gravityScale = wallHangingGravityScale;
		} else { 
			body.gravityScale = 1.0f;
		}


		//Debug.Log (velocity.x);
		body.velocity = velocity;
		//lastVelocity = velocity;
		lastVelocityX = Mathf.Abs (velocity.x);
		oldPos = body.position;
	}

	bool isGrounded ()
	{
		// TODO remeber that this is weird for debug reasosn aka. not optimised :)
		bool isGrounded = false;
		foreach (Vector3 rayOffset in groundCheckRayOffsets) {
			if (!isGrounded) {
				RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset), -Vector2.up, (collider.bounds.extents.y + 0.1f), PlatformLayerMask);
				if (hit.collider != null) {
					isGrounded = true;
				}
			}
			Debug.DrawRay ((transform.position + rayOffset), new Vector2 (0.0f, -(collider.bounds.extents.y + 0.1f)), Color.red);
		}
		return isGrounded;
	}

	int isTouchingWall ()
	{	// returns -1 if touching left | 0 if touching noWall | 1 if touchign right
		return isTouchingLeftWall () ? -1 : (isTouchingRightWall () ? 1 : 0);
	}

	bool isTouchingRightWall ()
	{
		// TODO remeber that this is weird for debug reasosn aka. not optimised :)
		bool isTouchingRightWall = false;
		foreach (Vector3 rayOffset in wallCheckRayOffsets) {
			if (!isTouchingRightWall) {
				RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset), Vector2.right, (collider.bounds.extents.x + 0.1f), PlatformLayerMask);
				if (hit.collider != null) {
					isTouchingRightWall = true;
				}
			}
			Debug.DrawRay ((transform.position + rayOffset), new Vector2 ((collider.bounds.extents.x + 0.1f), 0.0f), Color.green);
		}
		return isTouchingRightWall;
	}
    
	bool isTouchingLeftWall ()
	{
		// TODO remeber that this is weird for debug reasosn aka. not optimised :)
		bool isTouchingLeftWall = false;
		foreach (Vector3 rayOffset in wallCheckRayOffsets) {
			if (!isTouchingLeftWall) {
				RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset), -Vector2.right, (collider.bounds.extents.x + 0.1f), PlatformLayerMask);
				if (hit.collider != null) {
					isTouchingLeftWall = true;
				}
			}
			Debug.DrawRay ((transform.position + rayOffset), new Vector2 (-(collider.bounds.extents.x + 0.1f), 0.0f), Color.green);
		}
		return isTouchingLeftWall;
	}
    
	void InitializeRays ()
	{
		CalculateGroundCheckRayPositions ();
		CalculateWallCheckRayPositions ();
	}

	void CalculateGroundCheckRayPositions ()
	{
		for (int i = 0; i < numberOfGroundCheckRays; i++) {
			float xAxisOffset = skinDepth - collider.bounds.extents.x + i * (collider.bounds.size.x - 2 * skinDepth) / (numberOfGroundCheckRays - 1);
			groundCheckRayOffsets.Add (new Vector3 (xAxisOffset, 0.0f, 0.0f));
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
		for (int i = 0; i < numberOfWallCheckRays; i++) {
			float yAxisOffset = skinDepth - collider.bounds.extents.y + i * (collider.bounds.size.y - 2 * skinDepth) / (numberOfWallCheckRays - 1);
			wallCheckRayOffsets.Add (new Vector3 (0.0f, yAxisOffset, 0.0f));
		}
		/*
		Debug.Log ("wall check ray offsets:");
		foreach (Vector3 rayOffset in wallCheckRayOffsets) {
			Debug.Log (rayOffset.y);
		}
		*/
	}







}