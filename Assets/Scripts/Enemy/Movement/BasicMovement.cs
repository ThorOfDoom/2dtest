using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicMovement : MonoBehaviour
{

	public LayerMask obstacleLayerMask;
	public bool facingRight = true;
	public float skinDepth;
	public int numberOfWallCheckRays;


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
		//Debug.Log (Mathf.Sin (45 * (Mathf.PI / 180)));
		CalculateWallCheckRayPositions ();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{

		velocity = body.velocity;

		if (facingRight) {
			groundCheckRayOrigin.x = rightRayOriginX;
			groundCheckDirection = rightGroundCheckDirection;
		} else {
			groundCheckRayOrigin.x = leftRayOriginX;
			groundCheckDirection = leftGroundCheckDirection;
		}
		groundCheckRayOrigin.y = transform.position.y;



		RaycastHit2D groundHit = Physics2D.Raycast (groundCheckRayOrigin, groundCheckDirection, groundCheckRayLength, obstacleLayerMask);

		
		if (groundHit.collider == null || isTouchingWall ()) {
			//Debug.Log (groundHit.collider.name);
			if (facingRight) {
				facingRight = false;

			} else {
				facingRight = true;
			}
			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1), transform.localScale.y, transform.localScale.z);
		} 

		velocity.x = enemy.movementSpeed * transform.localScale.x;
		
		body.velocity = velocity;

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











