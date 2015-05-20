using UnityEngine;
using System.Collections;

[AddComponentMenu( "Utilities/LifeBar")]
public class LifeBar : MonoBehaviour
{
	public Rect startRect = new Rect (10, 10, 75, 50); // The rect the window is initially displayed at.
	public bool updateColor = true; // Do you want the color to change if the FPS gets low
	public bool allowDrag = true; // Do you want to allow the dragging of the FPS window
	//public  float frequency = 0.5F; // The update frequency of the fps
	//public int nbDecimal = 1; // How many decimal do you want to display
	public int lowestGreenPercent = 66;
	public int lowestYellowPercent = 33;
	public int lowestRedPercent = 10;

	private GUIStyle style; // The style the text will be displayed at, based en defaultSkin.label.

	/*
	void OnGUI ()
	{
		// Copy the default label skin, change the color and the alignement
		if (style == null) {
			style = new GUIStyle (GUI.skin.label);
			style.normal.textColor = Color.white;
			style.alignment = TextAnchor.MiddleCenter;
		}
		
		GUI.color = updateColor ? color : Color.white;
		startRect = GUI.Window (0, startRect, DoMyWindow, "");
	}
	
	void DoMyWindow (int windowID)
	{
		GUI.Label (new Rect (0, 0, startRect.width, startRect.height), sFPS + " FPS", style);
		if (allowDrag)
			GUI.DragWindow (new Rect (0, 0, Screen.width, Screen.height));
	}*/
}