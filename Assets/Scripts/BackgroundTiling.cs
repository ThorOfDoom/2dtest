using UnityEngine;
using System.Collections;

public class BackgroundTiling : MonoBehaviour
{

	public float _width = 1.0f;
	public float _height = 1.0f;

	Transform block;

	// Use this for initialization
	void Start ()
	{
		UpdateBlock ();
	}

	public void UpdateBlock (float width, float height)
	{
		_width = width;
		_height = height;
		UpdateBlock ();
	}

	void UpdateBlock ()
	{
		CheckBlock ();
		block.localScale = new Vector3 (_width, _height, 0.0f);
		block.localPosition = new Vector3 (_width / 2, _height / 2, 0.0f);
		block.GetComponent<Renderer> ().material.SetTextureScale ("_MainTex", new Vector2 (_width, _height));
	}

	void CheckBlock ()
	{
		if (block == null) {
			block = transform.FindChild ("Block");
		}
	}
}
