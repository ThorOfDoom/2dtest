using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerInputController))]
public class Player : MonoBehaviour
{
	public float walkVelocity;

	PlayerInputController playerInputController;
	Rigidbody2D body;

	private bool shouldMove = false;

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
	}

	void MovePlayer ()
	{
		if (shouldMove) {
			body.velocity = new Vector2 (walkVelocity * playerInputController.moving, body.velocity.y);
			transform.localScale = new Vector3 (transform.localScale.x * (body.velocity.x > 0 ? 1 : -1), transform.localScale.y * 1, transform.localScale.z * 1);
		} else {
			body.velocity = new Vector2 (0.0f, body.velocity.y);
		}
	}
}
