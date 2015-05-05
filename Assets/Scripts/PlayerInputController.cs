using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInputController : MonoBehaviour
{
	public int moving;
	public bool running = false;
	public bool runToggle = false;
	public int jumping = 0;
	public bool blinking = false;

	private Dictionary<string, string> keyBindings;
	// Use this for initialization
	void Start ()
	{
		keyBindings = GetKeyBindings ();
	}

	void Update ()
	{
		moving = 0;
		running = false;

		if (Input.GetKey (keyBindings ["moveLeft"]) || Input.GetKey (keyBindings ["moveLeftAlt"])) {
			moving -= 1;
		}
		if (Input.GetKey (keyBindings ["moveRight"]) || Input.GetKey (keyBindings ["moveRightAlt"])) {
			moving += 1;
		}
		
		if (Input.GetKey (keyBindings ["run"]) || Input.GetKey (keyBindings ["runAlt"])) {
			running = true;
		}
		
		if (Input.GetKeyDown (keyBindings ["runToggle"]) || Input.GetKeyDown (keyBindings ["runToggleAlt"])) {
			runToggle = runToggle ? false : true;
		}

		if (Input.GetKeyDown (keyBindings ["jump"]) || Input.GetKeyDown (keyBindings ["jumpAlt"])) {
			jumping = 1;
		} else {
			jumping = 0;
		}

		if (Input.GetKeyUp (keyBindings ["jump"]) || Input.GetKeyUp (keyBindings ["jumpAlt"])) {
			jumping = -1;
		}

		
		if (Input.GetKeyDown (keyBindings ["blink"]) || Input.GetKeyDown (keyBindings ["blinkAlt"])) {
			blinking = true;
		} else {
			blinking = false;
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
					string[] binding = new string[3];
					binding = line.Split ('=');
					
					keys.Add (binding [0], binding [1]);
					keys.Add (binding [0] + "Alt", binding [2]);
				}
			}
		}
		return keys;
	}
}
