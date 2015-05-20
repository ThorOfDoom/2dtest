using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicMovement : MonoBehaviour
{
	/*
	 * refactor methods that calculate rays to be external and flexible
	 * make them enemies obey gravity :O
	 * adjust attack
	 * bug fixes!! 
	 * refactor some more
	 */
	public LayerMask obstacleLayerMask;
	public bool facingRight = true;
	public float skinDepth;
	public int numberOfWallCheckRays;
	public float knockBackDuration;
	public float knockBackDistance;


	Rigidbody2D body;
	BoxCollider2D collider;
	Enemy enemy;

	private Vector2 velocity;
	private float groundCheckRayLength;
	private List<Vector3> wallCheckRayOffsets = new List<Vector3> ();
	private Vector2 rightGroundCheckDirection;
	private Vector2 leftGroundCheckDirection;
	private float rightRayOriginX;
	private float leftRayOriginX;
	private Vector2 groundCheckRayOrigin;
	private Vector2 groundCheckDirection;
	private float wallCheckRaycastDistance;
	private bool knockBack = false;
	private bool doKnockBack = false;
	private float knockBackTime;
	private int knockBackDiretion;
	private float knockBackVelocity;
	private bool movingBackwards = false;
	private bool flippedLastFrame = false;
	public bool falling = true;
	private float airTime;
    


	// Use this for initialization
	void Start ()
	{
		enemy = GetComponent<Enemy> ();
		body = GetComponent<Rigidbody2D> ();
		collider = GetComponent<BoxCollider2D> ();
		groundCheckRayLength = (collider.bounds.extents.y + skinDepth) / Mathf.Sin (45 * (Mathf.PI / 180));
		rightGroundCheckDirection = new Vector2 (1.0f, -1.0f);
		leftGroundCheckDirection = new Vector2 (-1.0f, -1.0f);
		rightRayOriginX = collider.bounds.max.x - skinDepth;
		leftRayOriginX = collider.bounds.min.x + skinDepth;
		groundCheckRayOrigin = new Vector2 (0.0f, 0.0f);
		wallCheckRaycastDistance = (collider.bounds.extents.x + 0.1f);
		knockBackVelocity = Mathf.Sqrt (2.0f * Mathf.Abs (Physics2D.gravity.y) * knockBackDistance);
		//Debug.Log (Mathf.Sin (45 * (Mathf.PI / 180)));
		CalculateWallCheckRayPositions ();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		/*
		 * to check for collisions we cast a ray skinDepth from the surface at halve the height of the enemy
		 * when we hit nothing we are at an edge and should turn
		 * when we hit the ground nothing happens
		 */
		velocity = body.velocity;

		if (!falling) {
			groundCheckDirection = facingRight ? rightGroundCheckDirection : leftGroundCheckDirection;
			groundCheckRayOrigin.y = transform.position.y;
			groundCheckRayOrigin.x = transform.position.x;
			
			
			
			RaycastHit2D groundHit = Physics2D.Raycast (groundCheckRayOrigin, groundCheckDirection, groundCheckRayLength, obstacleLayerMask);
			Debug.DrawRay (groundCheckRayOrigin, groundCheckDirection * groundCheckRayLength, Color.red);
			
			bool touchesWall = false;
			if (groundHit.collider == null || (touchesWall = isTouchingWall ())) {
				//Debug.Log (groundHit.collider.name);
				if (!touchesWall && flippedLastFrame) {
					falling = true;
				} else {
					Flip ();
					
					flippedLastFrame = true;
				}
			} else {
				flippedLastFrame = false;
			}
		} else {
			RaycastHit2D groundHit = Physics2D.Raycast (new Vector2 (collider.bounds.center.x, collider.bounds.min.y + 0.1f), -Vector2.up, 0.6f, obstacleLayerMask);
			Debug.DrawRay (new Vector2 (collider.bounds.center.x, collider.bounds.min.y + 0.1f), -Vector2.up * 0.6f, Color.red);

			if (groundHit.collider != null) {
				airTime = 0.0f;
				falling = false;
				velocity.y = 0.0f;
				body.transform.position = body.transform.position - new Vector3 (0.0f, groundHit.distance - 0.1f, 0.0f);
			} else {
				airTime += Time.deltaTime;
				velocity.y = Physics2D.gravity.y * airTime;
			}
		}

		if (knockBack) {
			doKnockBack = true;
			if (movingBackwards) {
				//transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1), transform.localScale.y, transform.localScale.z);
			}
		} else if (doKnockBack) {
			doKnockBack = false;
			if (movingBackwards) {
				ToggleFacing ();
			}
		}

		if (doKnockBack) {
			knockBackTime += Time.deltaTime;
			if (knockBackDuration > knockBackTime) {
				velocity.x = (knockBackVelocity + Physics2D.gravity.y * knockBackTime) * knockBackDiretion;
			} else {
				knockBack = false;
				knockBackTime = 0.0f;
			}
		} else if (!falling) {
			velocity.x = enemy.movementSpeed * transform.localScale.x;
		}
		
		body.velocity = velocity;

	}

	public void KnockBack (int direction)
	{
		knockBackDiretion = direction;
		knockBack = true;
		if ((facingRight && direction == -1) || (!facingRight && direction == 1)) {
			movingBackwards = true;
			ToggleFacing ();
		}
	}

	void ToggleFacing ()
	{
		facingRight = facingRight ? false : true;
	}

	void Flip ()
	{
		ToggleFacing ();
		transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1), transform.localScale.y, transform.localScale.z);
	}

	void CalculateWallCheckRayPositions ()
	{
		float skinDepthWithoutExtents = skinDepth - collider.bounds.extents.y;
		float distanceBetweenRays = (collider.bounds.size.y - 2 * skinDepth) / (numberOfWallCheckRays - 1);
		Vector3 currentOffset = new Vector3 (0.0f, 0.0f, 0.0f);

		for (int i = 0; i < numberOfWallCheckRays; i++) {
			currentOffset.y = skinDepthWithoutExtents + i * distanceBetweenRays;
			wallCheckRayOffsets.Add (currentOffset);
		}
	}

	bool isTouchingWall ()
	{
		Vector2 direction = (facingRight ? Vector2.right : -Vector2.right);

		for (int i = 0; i < numberOfWallCheckRays; i++) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + wallCheckRayOffsets [i]), direction, wallCheckRaycastDistance, obstacleLayerMask);
			if (hit.collider != null) {
				//Debug.Log (hit.collider.name);
				/*if (hit.collider.name == "Player") {
					Player player = hit.collider.gameObject.GetComponent<Player> ();
					player.TakeHit (enemy.damage);
				} else {
					return true;
				}*/
				return true;
			}
		}
		/*
		foreach (Vector3 rayOffset in wallCheckRayOffsets) {
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset), (facingRight ? Vector2.right : -Vector2.right), (collider.bounds.extents.x + 0.1f), obstacleLayerMask);
			if (hit.collider != null) {
				//Debug.Log (hit.collider.name);
				return true;
			}
		}*/
		return false;
	}

}











