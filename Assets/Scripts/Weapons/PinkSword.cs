using UnityEngine;
using System.Collections;

public class PinkSword : MonoBehaviour, IWeapon
{
	public float attackSpeed;
	public float damage;
	public float speed{ get; set; }
	public float dmg{ get; set; }

	void Start ()
	{
		speed = attackSpeed;
		dmg = damage;
	}
}
