using UnityEngine;
using System.Collections;


public class DebugLine : MonoBehaviour
{
	public float lineDuration;

	Vector2 oldPos;


	void Start ()
	{
		oldPos = transform.position;
	}
	

	void Update ()
	{
		Debug.DrawLine (oldPos, transform.position, Color.green, lineDuration);
		oldPos = transform.position;
	}
}
