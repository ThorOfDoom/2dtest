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
		ScaleSpike ();
		// update collider
		UpdateCollider ();
		// rotate
		RotateSpike ();
	}

	void ScaleSpike ()
	{
		spike.localScale = new Vector3 (_width, 0.5f, 0.0f);
		spike.localPosition = new Vector3 (_width / 2, 0.25f, 0.0f);
		spike.GetComponent<Renderer> ().material.SetTextureScale ("_MainTex", new Vector2 (_width, 1));
	}

	void UpdateCollider ()
	{
		int width = (int)_width;
		if (width > 1) {
			PolygonCollider2D col = spike.GetComponent<PolygonCollider2D> ();
			List<Vector2> pathPoints = new List<Vector2> ();
			pathPoints.Add (new Vector2 (-0.5f - 1 / (2 * _width) + (1 / _width), 0.4f));
			pathPoints.Add (new Vector2 (-0.5f - 1 / (2 * _width) + (1 / _width) * width, 0.4f));
			pathPoints.Add (new Vector2 (-0.5f + (1 / _width) * width, -0.5f));
			pathPoints.Add (new Vector2 (-0.5f, -0.5f));
			col.points = pathPoints.ToArray ();
		}
	}

	void RotateSpike ()
	{
		switch (_rotation) {
		case 0:
			// on floor
			break;
		case 1:
			// on a right wall
			transform.Rotate (0, 0, 45);
			break;
		case 2:
			// upside down
			transform.localScale = new Vector3 (1.0f, -1.0f, 1.0f);
			break;
		case 3:
			// on a left wall
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
