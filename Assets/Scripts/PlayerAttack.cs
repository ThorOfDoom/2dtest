using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{

	public GameObject handPosition;
	public GameObject weapon;
	public AnimationClip animation;
	public LayerMask enemyLayer;

	private WeaponStats weaponStats;

	private float lastHitTime;


	// Use this for initialization
	void Start ()
	{
		weapon = (GameObject)Instantiate (weapon, handPosition.transform.position, handPosition.transform.rotation);
		weapon.transform.parent = handPosition.transform;
		weaponStats = weapon.GetComponent<WeaponStats> ();

		if (weaponStats.attackSpeed != animation.length) {
			// adjust animation speed!
			weaponStats.attackSpeed = animation.length;
		}

		lastHitTime = Time.time - weaponStats.attackSpeed;

		Debug.Log (animation.length);
	}
	
	public WeaponStats GetWeaponStats ()
	{
		return weaponStats;
	}

	public RaycastHit2D DoAttack ()
	{
		Vector2 heading = weaponStats.weaponTip.transform.position - weaponStats.transform.position;
		float distance = heading.magnitude;
		var direction = heading / distance;
		Debug.DrawRay (weaponStats.weaponBase.transform.position, direction, Color.cyan, 1.0f);

		return Physics2D.Raycast (weaponStats.weaponBase.transform.position, direction, distance, enemyLayer);
	}
}
