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
	private List<Vector3> wallCheckRayOffsets = new List<Vector3>();


	// Use this for initialization
	void Start () 
	{
		enemy = GetComponent<Enemy> ();
		body = GetComponent<Rigidbody2D> ();
		collider = GetComponent<BoxCollider2D> ();
		groundCheckRayLength = (collider.bounds.extents.y + skinDepth) / Mathf.Sin(45*(Mathf.PI/180));
		Debug.Log (Mathf.Sin(45*(Mathf.PI/180)));
		CalculateWallCheckRayPositions ();
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{

		velocity = body.velocity;

		Vector2 groundCheckRayOrigin;
		if (facingRight) {
			groundCheckRayOrigin = new Vector2 (collider.bounds.max.x - skinDepth, transform.position.y);
		} else {
			groundCheckRayOrigin = new Vector2 (collider.bounds.min.x + skinDepth, transform.position.y);
		}


		RaycastHit2D groundHit = Physics2D.Raycast (groundCheckRayOrigin,
		                                      new Vector2(-1.0f, -1.0f),
		                                      groundCheckRayLength,
		                                      obstacleLayerMask);

		
		if (groundHit.collider == null || isTouchingWall ()) 
		{
			if (facingRight)
			{
				facingRight = false;

			}else{
				facingRight = true;
			}
			transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1), transform.localScale.y, transform.localScale.z);
		}
		velocity.x = enemy.movementSpeed * transform.localScale.x;
		
		body.velocity = velocity;

	}
	void CalculateWallCheckRayPositions ()
	{
		for (int i = 0; i < numberOfWallCheckRays; i++) {
			float yAxisOffset = skinDepth - collider.bounds.extents.y + i * (collider.bounds.size.y - 2 * skinDepth) / (numberOfWallCheckRays - 1);
			wallCheckRayOffsets.Add (new Vector3 (0.0f, yAxisOffset, 0.0f));
		}
	}

	bool isTouchingWall ()
	{

		foreach (Vector3 rayOffset in wallCheckRayOffsets) 
		{
			RaycastHit2D hit = Physics2D.Raycast ((transform.position + rayOffset), (facingRight ? Vector2.right : -Vector2.right), (collider.bounds.extents.x + 0.1f), obstacleLayerMask);
			if (hit.collider != null) {
				return true;
			}
		}
		return false;
	}

}











