using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{

	public float movementSpeed;
	public float damage;
	public float health;

	private BasicMovement movement;

	void Start ()
	{
		movement = GetComponent<BasicMovement> ();
	}

	public void TakeHit (float damage, float hitX)
	{
		health -= damage;

		int knockBackDiretion;
		if (transform.position.x > hitX) {
			knockBackDiretion = 1;
		} else if (transform.position.x < hitX) {
			knockBackDiretion = -1;
		} else {
			knockBackDiretion = 0;
		}

		movement.KnockBack (knockBackDiretion);

		if (health <= 0.0f) {
			Die ();
		}
	}

	void Die ()
	{
		Destroy (gameObject);
	}
}
