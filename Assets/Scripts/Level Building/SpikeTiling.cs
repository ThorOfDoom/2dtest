using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpikeTiling : MonoBehaviour
{
	
	public float _width = 1.0f;

	int _rotation = 5;
	
	Transform spike;
	
	// Use this for initialization
	void Start ()
	{
		UpdateSpike ();
	}
	
	public void UpdateSpike (float width, int rotation)
	{
		_width = width;
		_rotation = rotation;
		UpdateSpike ();
	}
	
	void UpdateSpike ()
	{
		CheckSpike ();
		spike.localScale = new Vector3 (_width, 0.5f, 0.0f);
		spike.localPosition = new Vector3 (_width / 2, 0.25f, 0.0f);
		spike.GetComponent<Renderer> ().material.SetTextureScale ("_MainTex", new Vector2 (_width, 1));
		// update collider
		PolygonCollider2D col = spike.GetComponent<PolygonCollider2D> ();
		List<Vector2> pathPoints = new List<Vector2> ();
		col.pathCount = (int)_width * 2 + 1;
		for (int i = 1; i <= _width; i++) {
			pathPoints.Add (new Vector2 (-0.5f - 1 / (2 * _width) + (1 / _width) * i, 0.5f));
			pathPoints.Add (new Vector2 (-0.5f + (1 / _width) * i, -0.5f));
		}
		pathPoints.Add (new Vector2 (-0.5f, -0.5f));
		col.points = pathPoints.ToArray ();
		switch (_rotation) {
		case 1: // on a right wall
			transform.Rotate (0, 0, 45);
			break;
		case 2: // upside down
			transform.localScale = new Vector3 (1.0f, -1.0f, 1.0f);
			break;
		case 3: // on a left wall
			transform.Rotate (0, 0, 45);
			transform.localScale = new Vector3 (1.0f, -1.0f, 1.0f);
			break;
		default:
			break;
		}
	}
	
	void CheckSpike ()
	{
		if (spike == null) {
			spike = transform.FindChild ("Triangle");
		}
	}
}
