using UnityEngine;
using System.Collections;

public class DebugLine : MonoBehaviour
{
	public float lineDuration;

	private Vector2 oldPos;

	// Use this for initialization
	void Start ()
	{
		oldPos = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Debug.DrawLine (oldPos, transform.position, Color.green, lineDuration);
		oldPos = transform.position;
	}
}
