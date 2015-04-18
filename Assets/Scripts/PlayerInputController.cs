using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour
{
	public int moving;
	private Dictionary<string, string> keyBindings;
	// Use this for initialization
	void Start ()
	{
		keyBindings = GetKeyBindings ();
	}

	void Update ()
	{
		moving = 0;
		if (Input.GetKey (keyBindings ["left"]) || Input.GetKey (keyBindings ["leftAlt"])) {
			moving -= 1;
		}
		if (Input.GetKey (keyBindings ["right"]) || Input.GetKey (keyBindings ["rightAlt"])) {
			moving += 1;
		}
	}

	/*
	 * reads KeyBindings.txt and parses the values into the keyBindings Dictionary.
	 */
	Dictionary<string, string> GetKeyBindings ()
	{
		Dictionary<string, string> keys = new Dictionary<string, string> ();
		TextAsset textFile = (TextAsset)Resources.Load ("KeyBindings", typeof(TextAsset));
		System.IO.StringReader textStream = new System.IO.StringReader (textFile.text);
		string line;

		while ((line = textStream.ReadLine()) != null) {
			if (!string.IsNullOrEmpty (line)) {
				if (!line.StartsWith ("#")) {
					string[] binding = line.Split ('=');

					//if only one key set add the second
					if (binding.Length == 2) {
						binding [2] = binding [1];
					}
					
					keys.Add (binding [0], binding [1]);
					keys.Add (binding [0] + "Alt", binding [2]);
				}
			}
		}
		return keys;
	}
}
