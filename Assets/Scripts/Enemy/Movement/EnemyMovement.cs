using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
	public LayerMask obstacleLayerMask;
	public LayerMask enableLayerMask;
	public float checkRadius;
	public bool neverSleep;

	//we start out facing right
	bool facingRight = true;
	Enemy enemy;
	Rigidbody2D body;
	BoxCollider2D clldr = null;
	Vector2 velocity;
	bool flipedLastFrame = true;
	float halveBodyWidth;
	float halveBodyHeight;
	bool isGrounded;
	bool groundedLastFrame;
	float airtime;
	int pingCounter;
	Vector3 rayOffset;
	Vector3[] groundCheckRayPositions;

	void Start ()
	{
		enemy = GetComponent<Enemy> ();
		body = GetComponent<Rigidbody2D> ();
		clldr = GetComponent<BoxCollider2D> ();
		halveBodyWidth = transform.localScale.x / 2;
		halveBodyHeight = transform.localScale.y / 2;
		isGrounded = groundedLastFrame = Grounded ();
		airtime = 0.0f;
		pingCounter = 0;
		if (neverSleep) {
			this.enabled = true;
		}
		rayOffset = new Vector3 (clldr.bounds.extents.x, 0.0f, 0.0f);
		groundCheckRayPositions = new Vector3[3];
	}

	void FixedUpdate ()
	{
		if (groundedLastFrame || flipedLastFrame) {
			isGrounded = Grounded ();
		} else {
			isGrounded = CastGroundRay ();
		}
		// if not colliding
		if (flipedLastFrame || !NeedToFlip ()) {
			Move ();
			flipedLastFrame = false;
		} else {
			Flip ();
		}
		groundedLastFrame = isGrounded;

		if (--pingCounter < 0 && !neverSleep) {
			body.velocity = new Vector2 (0.0f, 0.0f);
			this.enabled = false;
		}
	}

	void Move ()
	{
		velocity = body.velocity;
		if (isGrounded) {
			velocity.x = enemy.movementSpeed * transform.localScale.x;
			airtime = 0.0f;
		} else {
			airtime += Time.deltaTime;
			velocity.y = Physics2D.gravity.y * airtime;
		}
		body.velocity = velocity;
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
		if (Physics2D.OverlapPointNonAlloc (groundCheckPoint, collision, obstacleLayerMask) != 0) {
			//Debug.Log ("yes");
			return true;
		}
		
		//Debug.Log ("no");
		return false;
	}

	bool CastGroundRay ()
	{
		GetGroundCheckRayPositions ();
		float distance = Mathf.Abs (body.velocity.y * Time.fixedDeltaTime);

		for (int i = 0; i < groundCheckRayPositions.Length; i++) {
			RaycastHit2D hit = Physics2D.Raycast (groundCheckRayPositions [i], -Vector2.up, 
			                                      distance + 0.5f, obstacleLayerMask);
			Debug.DrawLine (groundCheckRayPositions [i] - new Vector3 (0.0f, 0.5f + distance * 2, 0.0f), 
			                groundCheckRayPositions [i], Color.blue, Time.fixedDeltaTime);
			if (hit.collider != null) {
				Debug.Log (hit.collider.name);
				if (hit.collider.tag == "Spike") {
					enemy.Die ();
				} else {
					Land (hit.distance - 0.5f);
				}
				return true;
			}
		}

		return false;
	}

	void GetGroundCheckRayPositions ()
	{
		groundCheckRayPositions [0] = transform.position - rayOffset;

		groundCheckRayPositions [1] = transform.position;
		groundCheckRayPositions [2] = transform.position + rayOffset;
	}

	void Land (float distance)
	{
		body.velocity = new Vector2 (body.velocity.x, 0.0f);
		transform.position = new Vector2 (transform.position.x, transform.position.y - distance);
	}

	bool NeedToFlip ()
	{
		if (groundedLastFrame && !isGrounded) {
			return true;
		}
		
		float circleXPos = transform.position.x + (halveBodyWidth * transform.localScale.x);
		float circleYPos = transform.position.y - halveBodyHeight + checkRadius + 0.05f;
		Collider2D collision = Physics2D.OverlapCircle (new Vector2 (circleXPos, circleYPos), 
		                                                checkRadius, 
		                                                 obstacleLayerMask);

		if (collision != null) {
			return true;
		}

		return false;
	}
	
	void ToggleFacing ()
	{
		facingRight = facingRight ? false : true;
	}

	public void EnablePing ()
	{
		pingCounter = Random.Range (25, 150);

		this.enabled = true;
	}

	//DEBUG
	void OnDrawGizmos ()
	{
		if (clldr != null) {
			float circleXPos = transform.position.x + (halveBodyWidth * transform.localScale.x);
			float circleYPos = transform.position.y - halveBodyHeight + checkRadius + 0.05f;
			Gizmos.DrawWireSphere (new Vector3 (circleXPos, circleYPos, 0.0f), 
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