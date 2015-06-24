using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
	public LayerMask platformLayerMask;
	public float checkRadius;
	//we start out facing right
	bool facingRight = true;
	Enemy enemy;
	Rigidbody2D body;
	BoxCollider2D clldr = null;
	Vector2 velocity;
	bool flipedLastFrame = true;
	float halveBodyWidth;
	bool isGrounded;
	bool groundedLastFrame;

	void Start ()
	{
		enemy = GetComponent<Enemy> ();
		body = GetComponent<Rigidbody2D> ();
		clldr = GetComponent<BoxCollider2D> ();
		halveBodyWidth = transform.localScale.x / 2;
		isGrounded = groundedLastFrame = Grounded ();
	}

	void FixedUpdate ()
	{
		isGrounded = Grounded ();
		// if not colliding
		if (flipedLastFrame || !NeedToFlip ()) {
			Move ();
			flipedLastFrame = false;
		} else {
			Flip ();
		}
		groundedLastFrame = isGrounded;
	}

	void Move ()
	{
		if (isGrounded) {
			velocity = body.velocity;
			velocity.x = enemy.movementSpeed * transform.localScale.x;
			body.velocity = velocity;
		}
	}
	
	void Flip ()
	{
		ToggleFacing ();
		transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x) * (facingRight ? 1 : -1),
		                                    transform.localScale.y, transform.localScale.z);
		flipedLastFrame = true;
	}

	bool Grounded ()
	{
		Collider2D[] collision = new Collider2D[1];/*Physics2D.OverlapCircle (groundCircle.transform.position, 
		                                                   checkCicrleRadius, 
		                                                   platformLayerMask);*/
		Vector2 groundCheckPoint = new Vector2 (transform.position.x + ((halveBodyWidth + checkRadius) * transform.localScale.x), 
		                                        clldr.bounds.min.y - checkRadius);
		if (Physics2D.OverlapPointNonAlloc (groundCheckPoint, collision, platformLayerMask) != 0) {
			Debug.Log ("yes");
			return true;
		}
		
		Debug.Log ("no");
		return false;
	}

	bool NeedToFlip ()
	{
		if (groundedLastFrame && !isGrounded) {
			return true;
		}
		float circleXPos = transform.position.x + (halveBodyWidth * transform.localScale.x);
		Collider2D collision = Physics2D.OverlapCircle (new Vector2 (circleXPos, transform.position.y), 
		                                                checkRadius, 
		                                                 platformLayerMask);

		if (collision != null) {
			return true;
		}

		return false;
	}
	
	void ToggleFacing ()
	{
		facingRight = facingRight ? false : true;
	}

	//DEBUG
	void OnDrawGizmos ()
	{
		if (clldr != null) {
			float circleXPos = transform.position.x + (halveBodyWidth * transform.localScale.x);
			Gizmos.DrawWireSphere (new Vector3 (circleXPos, transform.position.y, 0.0f), 
			                       checkRadius);
			Vector2 groundCheckPoint = new Vector2 (transform.position.x + ((halveBodyWidth + checkRadius) * transform.localScale.x), 
			                                        clldr.bounds.min.y - checkRadius);
			Gizmos.DrawSphere (groundCheckPoint, 0.05f);
		}
	}
}

/*// radius = sqrt(width * width + ehight * height) / 2 | sqrt(1 * 1 + 1 *1 ) / 2 -> 0.707106781
Collider2D[] collisions = Physics2D.OverlapCircle (transform.position, 0.7f, platformLayerMask);
// should allways collide with the ground unless falling
if(collisions.Length > 1){
	for (int i = 0; i < collisions.Length; i++) {
		collisions[i].gameObject.transform.position.x
	}
}*/