using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{

	public float walkVelocity;
	public float runVelocity;
	public float runAcceleration;
	public float jumpHeight;
	public float slideReductionMultiplier;
	public float airSlideReductionMultiplier;
	public float wallJumpTime;
	public float wallJumpModifier;
	public float wallJumpForceMoifier;
	public Vector2 jumpUpWallDistance;
	public float blinkDistance;
	public float blinkCoolDown;
	public LayerMask blinkLayerMask;
	public Vector2 knockBackDistance;

	Rigidbody2D body;
	PlayerInputController playerInputController;
	Player player;

	Vector2 velocity;
	
	bool facingRight = true;
	bool didMove = false;
	bool isJumping = false;
	float lastKnownVelocityX;
	float initialJumpVelocity;
	float airTime = 0.0f;
	int wallJumpDirection = 0;
	bool jumpUpWall = false;
	Vector2 jumpUpWallForce;
	Vector2 knockBackForce;
	float knockBackAirTime = 0.0f;
	Vector2 knockBackVelocity = new Vector2 ();
	[HideInInspector]
	public 
		int
		knockBackDirection;


	void Start ()
	{
		body = GetComponent<Rigidbody2D> ();
		player = GetComponent<Player> ();
		playerInputController = GetComponent<PlayerInputController> ();
		velocity = new Vector2 ();
		initialJumpVelocity = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpHeight);
		knockBackForce = new Vector2 (Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance.x),
		                              Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance.y));
	}

	public void MovePlayer ()
	{// TODO: split it up into seperat methods 
		velocity.y = body.velocity.y;
		player.anim.SetFloat ("VelY", body.velocity.y);
		float absVelX = Mathf.Abs (velocity.x);
		// TODO feels hackish
		if (player.shouldMove/* && playerInputController.moving != 0*/) {
			player.anim.SetBool("ShouldMove",true);
			if (player.shouldRun) {
				player.anim.SetBool("ShouldRun",true);
				if ((absVelX + runAcceleration) <= runVelocity) {
					velocity.x = absVelX + runAcceleration;
				} else {
					velocity.x = runVelocity;
				}	
			} else {
				velocity.x = walkVelocity;
				player.anim.SetBool("ShouldRun",false);
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
			player.anim.SetBool("ShouldMove",false);
			if (Mathf.Abs (lastKnownVelocityX) > walkVelocity || !player.grounded) {
				lastKnownVelocityX *= !player.grounded ? airSlideReductionMultiplier : slideReductionMultiplier;
				player.anim.SetBool("ShouldSlide",true);
			} else {
				didMove = false;
				lastKnownVelocityX = 0.0f;
				player.anim.SetBool("ShouldSlide",false);
			}
			velocity.x = lastKnownVelocityX;
			Debug.DrawLine (player.oldPos, body.position, Color.green, 5.0f);
		}
		
		if (player.doWallJump && (airTime > wallJumpTime)) {
			player.doWallJump = false;
			wallJumpDirection = 0;
			jumpUpWall = false;
			if (playerInputController.moving == 0) {
				velocity.x = 0.0f;
			}
			airTime = 0.0f;
		} else if ((playerInputController.jumping == 1 || player.jumpKeyPressed) && player.touchesWall != 0) {
			player.doWallJump = true;
			wallJumpDirection = -(player.touchesWall);
			jumpUpWall = (player.touchesWall == playerInputController.moving) ? true : false;
			airTime = 0.0f;
			player.jumpKeyPressed = false;
		} else if (player.shouldJump && !isJumping && player.grounded && !player.doWallJump) {
			isJumping = true;
		} else if (isJumping && player.grounded && !player.doWallJump) {
			isJumping = false;
			player.shouldJump = false;
			airTime = 0.0f;
			velocity.y = 0.0f;
		}
		
		if (isJumping) {
			airTime += Time.deltaTime;
			if (player.shouldJump) {
				velocity.y = initialJumpVelocity + Physics2D.gravity.y * airTime;
			} else if (player.touchesWall != 0 && playerInputController.moving != 0) {
				isJumping = false;
				player.shouldJump = false;
				airTime = 0.0f;
			} else {
				velocity.y = velocity.y < 0 ? velocity.y : 0.0f;
			}
			Debug.DrawLine (player.oldPos, body.position, Color.red, 5.0f);
		} else if (jumpUpWall) {
			if (wallJumpDirection == playerInputController.moving) {
				airTime = wallJumpTime;
			}
			airTime += Time.deltaTime;


			jumpUpWallForce = new Vector2 (Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpUpWallDistance.x),
			                               Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * jumpUpWallDistance.y));

			velocity.x = (jumpUpWallForce.x + Physics2D.gravity.y * airTime) * wallJumpDirection;
			velocity.y = jumpUpWallForce.y + Physics2D.gravity.y * airTime;
			
			didMove = true;
			lastKnownVelocityX = velocity.x;
			
			Debug.DrawLine (player.oldPos, body.position, Color.white, 5.0f);
		} else if (player.doWallJump) {
			airTime += Time.deltaTime;
			velocity.y = wallJumpModifier * initialJumpVelocity + Physics2D.gravity.y * airTime;
			velocity.x = wallJumpForceMoifier * runVelocity * wallJumpDirection;// TODO what if we are running?
			transform.localScale = new Vector3 (
				Mathf.Abs (transform.localScale.x) * (body.velocity.x > 0 ? 1 : -1), 
				transform.localScale.y, 
				transform.localScale.z);
			didMove = true;
			lastKnownVelocityX = velocity.x;
			//Debug.Log (player.touchesWall == playerInputController.moving);
			Debug.DrawLine (player.oldPos, body.position, Color.yellow, 5.0f);
		} else if (!player.grounded) {
			Debug.DrawLine (player.oldPos, body.position, Color.magenta, 5.0f);
		}
		
		body.velocity = velocity;
	}

	public void DoBlink ()
	{
		float _blinkDistance = CheckBlinkDistance ();
		_blinkDistance *= (facingRight ? 1 : -1);
		transform.position += new Vector3 (_blinkDistance, 0.0f, 0.0f);
		player.oldPos = transform.position;
		player.shouldBlink = false;
	}

	float CheckBlinkDistance ()
	{
		float distance = blinkDistance;
		Vector3 skinDepth = new Vector3 (transform.localScale.x / 2, 0.0f, 0.0f);
		Vector3 blinkRayCastOrigin = transform.position + skinDepth;
		Vector2 blinkRayCastDirection = (facingRight ? Vector2.right : -Vector2.right);
		
		for (int i = 0; i < player.numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast (
				(blinkRayCastOrigin + player.wallCheckRayOffsets [i]), 
				blinkRayCastDirection, 
				distance, 
				blinkLayerMask);
			if (hit.collider != null) {
				distance = hit.distance;
			}
		}
		
		return Mathf.Round (distance * 10) / 10.0f;
	}

	public 	void KnockBack ()
	{
		knockBackAirTime += Time.deltaTime;
		
		knockBackVelocity.x = knockBackForce.x + Physics2D.gravity.y * knockBackAirTime;
		knockBackVelocity.y = knockBackForce.y + Physics2D.gravity.y * knockBackAirTime;
		
		if (knockBackVelocity.x > 0.0f) {
			knockBackVelocity.x *= knockBackDirection;
			body.velocity = knockBackVelocity;
		} else {
			player.knockBack = false;
			knockBackAirTime = 0.0f;
			didMove = true;
		}
		
		Debug.DrawLine (player.oldPos, body.position, Color.green, 5.0f);
	}
}
