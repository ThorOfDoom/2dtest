using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerInputController))]
public class Player : MonoBehaviour
{
	public float walkVelocity;

	PlayerInputController playerInputController;
	Rigidbody2D body;

	private bool shouldMove = false;
	private bool shouldRun = false;
	private Vector2 velocity;

	// Use this for initialization
	void Start ()
	{
		playerInputController = GetComponent<PlayerInputController> ();
		body = GetComponent<Rigidbody2D> ();
	}

	void Update ()
	{
		CheckInputs ();
		MovePlayer ();
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
	}

	void MovePlayer ()
	{
		velocity = body.velocity;
		if (shouldMove) {
			velocity.x = walkVelocity * playerInputController.moving;
			transform.localScale = new Vector3 (transform.localScale.x * (body.velocity.x > 0 ? 1 : -1), transform.localScale.y * 1, transform.localScale.z * 1);
		} else {
			velocity.x = 0.0f;
		}

		body.velocity = velocity;
	}
}
