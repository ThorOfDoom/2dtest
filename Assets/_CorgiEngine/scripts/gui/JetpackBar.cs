using UnityEngine;
using System.Collections;
/// <summary>
/// Manages the jetpack bar
/// </summary>
public class JetpackBar : MonoBehaviour 
{
	/// the healthbar's foreground bar
	public Transform ForegroundBar;
	/// the color when at max fuel
	public Color MaxFuelColor = new Color(36/255f, 199/255f, 238/255f);
	/// the color for min fuel
	public Color MinFuelColor = new Color(24/255f, 164/255f, 198/255f);
	
	private CharacterBehavior _character;
	private CharacterJetpack _jetpack;

	/// <summary>
	/// Initialization, gets the player
	/// </summary>
	void Start()
	{
		_character = GameManager.Instance.Player;
		if (_character!=null)
			_jetpack=_character.GetComponent<CharacterJetpack>();
	}

	/// <summary>
	/// Every frame, sets the foreground sprite's width to match the character's remaining fuel.
	/// </summary>
	public void Update()
	{
		if (_jetpack==null)
			return;
		if (_character==null)
			return;
		
		float jetpackPercent = _character.BehaviorState.JetpackFuelDurationLeft / (float) _jetpack.JetpackFuelDuration;
		ForegroundBar.localScale = new Vector3(jetpackPercent,1,1);		
	}	
}
