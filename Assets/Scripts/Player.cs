using UnityEngine;
using System.Collections;
// TODO framerate! & sliding
[RequireComponent (typeof(PlayerInputController))]
public class Player : MonoBehaviour
{
	public float walkVelocity;
	public float runVelocity;
	public float runAcceleration;
	public Transform groundCheck;
	public LayerMask groundLayerMask;
	public bool grounded = false;
	public float jumpStrength = 10.0f;
	public float maxAirTime = 0.5f;
	public float jumpHeight = 3.0f;
	public float slideReductionMultiplier = 0.93f;
	public float airSlideReductionMultiplier = 0.80f;


	PlayerInputController playerInputController;
	Rigidbody2D body;

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

	// debug
	public float lastVelocityX;
	private Vector2 oldPos;

	// Use this for initialization
	void Start ()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		playerInputController = GetComponent<PlayerInputController> ();
		body = GetComponent<Rigidbody2D> ();
		Debug.Log (Mathf.Sqrt (2 * Mathf.Abs (Physics2D.gravity.y) * jumpHeight));
		Debug.Log (Physics2D.gravity.y);
	}

	void FixedUpdate ()
	{
		grounded = isGrounded ();
		MovePlayer ();

	}

	void Update ()
	{
		CheckInputs ();
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere (groundCheck.position, groundRadius);
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
		if (playerInputController.jumping && grounded && jumpFinished) {
			shouldJump = true;
			jumpFinished = false;
		}
		if (!playerInputController.jumping) {
			shouldJump = false;
			jumpFinished = true;
		}
	}

	void MovePlayer ()
	{
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
			if (Mathf.Abs (lastKnownVelocityX) > walkVelocity || isJumping) {
				lastKnownVelocityX *= isJumping ? airSlideReductionMultiplier : slideReductionMultiplier;
				if (isJumping) {
					Debug.Log (absVelX);
				}
			} else {
				didMove = false;
				lastKnownVelocityX = 0.0f;
			}
			velocity.x = lastKnownVelocityX;
			Debug.DrawLine (oldPos, body.position, Color.green, 5.0f);
		}

		if (shouldJump && !isJumping) {
			isJumping = true;
		} else if (isJumping && grounded) {
			isJumping = false;
			shouldJump = false;
			airTime = 0.0f;
			velocity.y = 0.0f;
		}


		if (isJumping) {
			airTime += Time.deltaTime;
			if (shouldJump) {// TODO move the calculation to the top so it is not calculated all the time ;)
				velocity.y = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpHeight) + Physics2D.gravity.y * airTime;
			} else {
				//velocity.y = Physics2D.gravity.y * (1 / maxAirTime * airTime);
				//isJumping = false;
				//shouldJump = false;
				velocity.y = velocity.y < 0 ? velocity.y : 0.0f;
				//airTime = 0.0f;
			}
			Debug.DrawLine (oldPos, body.position, Color.red, 5.0f);
		}

		//Debug.Log (velocity.x);
		body.velocity = velocity;
		//lastVelocity = velocity;
		lastVelocityX = Mathf.Abs (velocity.x);
		oldPos = body.position;
	}

	bool isGrounded ()
	{
		bool isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, groundLayerMask);
		return isGrounded;
	}
}