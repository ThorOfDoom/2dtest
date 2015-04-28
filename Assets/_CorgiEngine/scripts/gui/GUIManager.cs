using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// Handles all GUI effects and changes
/// </summary>
public class GUIManager : MonoBehaviour 
{
	/// the game object that contains the heads up display (avatar, health, points...)
	public GameObject HUD;
	/// the pause screen game object
	public GameObject PauseScreen;	
	/// the time splash gameobject
	public GameObject TimeSplash;
	/// the points counter
	public Text PointsText;
	/// the level display
	public Text LevelText;
	/// the screen used for all fades
	public Image Fader;
	/// the jetpack bar
	public GameObject JetPackBar;
	
	private static GUIManager _instance;
	
	// Singleton pattern
	public static GUIManager Instance
	{
		get
		{
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<GUIManager>();
			return _instance;
		}
	}

	/// <summary>
	/// Initialization
	/// </summary>
	public void Start()
	{
		RefreshPoints();
		
	}
	
	/// <summary>
	/// Sets the HUD active or inactive
	/// </summary>
	/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
	public void SetHUDActive(bool state)
	{
		HUD.SetActive(state);
		PointsText.enabled=state;
		LevelText.enabled=state;
	}

	/// <summary>
	/// Sets the pause.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, sets the pause.</param>
	public void SetPause(bool state)
	{
		PauseScreen.SetActive(state);
	}

	/// <summary>
	/// Sets the jetpackbar active or not.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, sets the pause.</param>
	public void SetJetpackBar(bool state)
	{
		JetPackBar.SetActive(state);
	}

	/// <summary>
	/// Sets the time splash.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, turns the timesplash on.</param>
	public void SetTimeSplash(bool state)
	{
		TimeSplash.SetActive(state);
	}
	
	/// <summary>
	/// Sets the text to the game manager's points.
	/// </summary>
	public void RefreshPoints()
	{
		PointsText.text="$"+GameManager.Instance.Points.ToString("000000");		
	}
	
	/// <summary>
	/// Sets the level name in the HUD
	/// </summary>
	public void SetLevelName(string name)
	{
		LevelText.text=name;		
	}
	
	/// <summary>
	/// Fades the fader in or out depending on the state
	/// </summary>
	/// <param name="state">If set to <c>true</c> fades the fader in, otherwise out if <c>false</c>.</param>
	public void FaderOn(bool state,float duration)
	{
		Fader.gameObject.SetActive(true);
		if (state)
			StartCoroutine(CorgiTools.FadeImage(Fader,duration, new Color(0,0,0,1f)));
		else
			StartCoroutine(CorgiTools.FadeImage(Fader,duration,new Color(0,0,0,0f)));
	}
	

}
