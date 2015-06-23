using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class PlayerAttack : MonoBehaviour
{
	public GameObject handPosition;
	public GameObject weapon;
	public AnimationClip anim;
	public LayerMask enemyLayer;

	WeaponStats weaponStats;
	Dictionary<int, float> hitEnemies;

	
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

		hitEnemies = new Dictionary<int, float> ();
	}

	void FixedUpdate ()
	{
		UpdateHitEnemies ();
	}

	public void Attack ()
	{
		RaycastHit2D hit = DoAttack ();
		if (hit.collider != null && !hitEnemies.ContainsKey (hit.collider.GetInstanceID ())) {
			Enemy enemy = hit.collider.GetComponent<Enemy> ();
			
			enemy.TakeHit (weaponStats.damage, hit.point.x);
			if (enemy.health > 0.0f) {
				hitEnemies.Add (hit.collider.GetInstanceID (), Time.time);
			}
		}
	}
	
	
	void UpdateHitEnemies ()
	{
		if (hitEnemies.Count != 0) {
			KeyValuePair<int, float>[] itemsToRemove = 
				hitEnemies.Where (f => f.Value < Time.time + weaponStats.attackSpeed).ToArray ();
			
			for (int i = 0; i < itemsToRemove.Length; i++) {
				hitEnemies.Remove (itemsToRemove [i].Key);
			}
		}
	}


	RaycastHit2D DoAttack ()
	{
		Vector2 heading = weaponStats.weaponTip.transform.position - weaponStats.transform.position;
		float distance = heading.magnitude;
		var direction = heading / distance;
		Debug.DrawRay (weaponStats.weaponBase.transform.position, direction, Color.cyan, 1.0f);

		return Physics2D.Raycast (weaponStats.weaponBase.transform.position, direction, distance, enemyLayer);
	}
}
