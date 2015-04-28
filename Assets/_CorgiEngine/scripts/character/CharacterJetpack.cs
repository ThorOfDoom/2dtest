using UnityEngine;
using System.Collections;

/// <summary>
/// Add this class to a character so it can jetpack in the air.
/// </summary>
public class CharacterJetpack : MonoBehaviour 
{
	
	/// the jetpack associated to the character
	public ParticleSystem Jetpack;
	
	/// the force applied by the jetpack
	public float JetpackForce = 2.5f;	
	/// true if the character has unlimited fuel for its jetpack
	public bool JetpackUnlimited = false;
	/// the maximum duration (in seconds) of the jetpack
	public float JetpackFuelDuration = 5f;
	/// the jetpack refuel cooldown
	public float JetpackRefuelCooldown=1f;
	
	private CharacterBehavior _characterBehavior;
	private CorgiController _controller;
	
	// Use this for initialization
	void Start () 
	{
		// initialize the private vars
		_characterBehavior = GetComponent<CharacterBehavior>();
		_controller = GetComponent<CorgiController>();
	
		if (Jetpack!=null)
		{
			Jetpack.enableEmission=false;
			GUIManager.Instance.SetJetpackBar (!JetpackUnlimited);
			_characterBehavior.BehaviorState.JetpackFuelDurationLeft = JetpackFuelDuration;		
		}
	}
	
	/// <summary>
	/// Causes the character to start its jetpack.
	/// </summary>
	public void JetpackStart()
	{
		// if the Jetpack action is enabled in the permissions, we continue, if not we do nothing
		if ((!_characterBehavior.Permissions.JetpackEnabled)||(!_characterBehavior.BehaviorState.CanJetpack)||(_characterBehavior.BehaviorState.IsDead))
			return;
		
		// if the character is not in a position where it can move freely, we do nothing.
		if (!_characterBehavior.BehaviorState.CanMoveFreely)
			return;
		
		// if the jetpack is not unlimited and if we don't have fuel left
		if ((!JetpackUnlimited) && (_characterBehavior.BehaviorState.JetpackFuelDurationLeft <= 0f)) 
		{
			// we stop the jetpack and exit
			JetpackStop();
			_characterBehavior.BehaviorState.CanJetpack=false;
			return;
		}
		
		// we set the vertical force and set the various states
		
		_controller.SetVerticalForce(JetpackForce);
		_characterBehavior.BehaviorState.Jetpacking=true;
		_characterBehavior.BehaviorState.CanMelee=false;
		_characterBehavior.BehaviorState.CanJump=false;
		Jetpack.enableEmission=true;
		// if the jetpack is not unlimited, we start burning fuel
		if (!JetpackUnlimited) 
		{
			StartCoroutine (JetpackFuelBurn ());
			
		}
	}
	
	/// <summary>
	/// Causes the character to stop its jetpack.
	/// </summary>
	public void JetpackStop()
	{
		if (Jetpack==null)
			return;
		_characterBehavior.BehaviorState.Jetpacking=false;
		_characterBehavior.BehaviorState.CanMelee=true;
		Jetpack.enableEmission=false;
		_characterBehavior.BehaviorState.CanJump=true;
		// if the jetpack is not unlimited, we start refueling
		if (!JetpackUnlimited)
			StartCoroutine (JetpackRefuel());
	}
	
	
	/// <summary>
	/// Burns the jetpack fuel
	/// </summary>
	/// <returns>The fuel burn.</returns>
	private IEnumerator JetpackFuelBurn()
	{
		// while the character is jetpacking and while we have fuel left, we decrease the remaining fuel
		float timer=_characterBehavior.BehaviorState.JetpackFuelDurationLeft;
		while ((timer > 0) && (_characterBehavior.BehaviorState.Jetpacking))
		{
			timer -= Time.deltaTime;
			_characterBehavior.BehaviorState.JetpackFuelDurationLeft=timer;
			yield return 0;
		}
	}
	
	/// <summary>
	/// Refills the jetpack fuel
	/// </summary>
	/// <returns>The fuel refill.</returns>
	private IEnumerator JetpackRefuel()
	{
		// we wait for a while before starting to refill
		yield return new WaitForSeconds (JetpackRefuelCooldown);
		// then we progressively refill the jetpack fuel
		float timer=_characterBehavior.BehaviorState.JetpackFuelDurationLeft;
		while ((timer < JetpackFuelDuration) && (!_characterBehavior.BehaviorState.Jetpacking))
		{
			timer += Time.deltaTime/2;
			_characterBehavior.BehaviorState.JetpackFuelDurationLeft=timer;
			// we prevent the character to jetpack again while at low fuel and refueling
			if ((!_characterBehavior.BehaviorState.CanJetpack) && (timer > 1f))
				_characterBehavior.BehaviorState.CanJetpack=true;
			yield return 0;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
