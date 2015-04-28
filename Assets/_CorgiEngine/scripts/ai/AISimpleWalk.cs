using UnityEngine;
using System.Collections;
/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AISimpleWalk : MonoBehaviour,IPlayerRespawnListener
{
	/// The speed of the agent
	public float Speed;
	/// The initial direction
	public bool GoesRightInitially=true;
	

	// private stuff
    private CorgiController _controller;
    private Vector2 _direction;
	private Vector2 _startPosition;
	
	/// <summary>
	/// Initialization
	/// </summary>
	public void Start ()
    {
		// we get the CorgiController2D component
		_controller = GetComponent<CorgiController>();
		// initialize the start position
		_startPosition = transform.position;
		// initialize the direction
        _direction = GoesRightInitially ? Vector2.right : -Vector2.right;
	}
	
	/// <summary>
	/// Every frame, moves the agent and checks if it can shoot at the player.
	/// </summary>
	public void Update () 
	{

        // moves the agent in its current direction
		_controller.SetHorizontalForce(_direction.x * Speed);
				
		// if the agent is colliding with something, make it turn around
		if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight))
		{
			_direction = -_direction;
			transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
		}
	}

	/// <summary>
	/// When the player respawns, we reinstate this agent.
	/// </summary>
	/// <param name="checkpoint">Checkpoint.</param>
	/// <param name="player">Player.</param>
	public void onPlayerRespawnInThisCheckpoint (CheckPoint checkpoint, CharacterBehavior player)
	{
		_direction = new Vector2(-1,0);
		transform.localScale=new Vector3(1,1,1);
		transform.position=_startPosition;
		gameObject.SetActive(true);
	}

}
