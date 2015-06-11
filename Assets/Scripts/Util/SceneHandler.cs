using UnityEngine;
using System.Collections;

public class SceneHandler : MonoBehaviour
{

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.LoadLevel (1);
		}
	}

	public void LoadScene (int index)
	{
		Application.LoadLevel (index);
	}

	public void LoadSceneByName (string name)
	{
		Application.LoadLevel (name);
	}
}
