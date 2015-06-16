using UnityEngine;
using System.Collections;


public class PlayerAttack : MonoBehaviour
{
	public GameObject handPosition;
	public GameObject weapon;
	public AnimationClip anim;
	public LayerMask enemyLayer;

	WeaponStats weaponStats;

	
	void Start ()
	{
		weapon = (GameObject)Instantiate (weapon, handPosition.transform.position, handPosition.transform.rotation);
		weapon.transform.parent = handPosition.transform;
		weaponStats = weapon.GetComponent<WeaponStats> ();

		if (weaponStats.attackSpeed != anim.length) {
			//TODO adjust animation speed!
			weaponStats.attackSpeed = anim.length;
			/*
			 * in new version of unity we will be able to set 
			 * the speed of the animation according to the weapons 
			 * attack speed
			 */
		}
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
