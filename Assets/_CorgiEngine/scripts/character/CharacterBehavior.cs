using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;
/// <summary>
/// This class will pilot the CorgiController component of your character.
/// This is where you'll implement all of your character's game rules, like jump, dash, shoot, stuff like that.
/// </summary>
public class CharacterBehavior : MonoBehaviour,CanTakeDamage
{
	/// the boxcollider2D that will be used to check if the character's head is colliding with anything (used when crouched mostly)
	public BoxCollider2D HeadCollider ;
	
	/// the current health of the character
	public int Health { get; private set; }	
	
	/// the various states of the character
	public CharacterBehaviorState BehaviorState { get; private set; }
	/// the default parameters of the character
	public CharacterBehaviorParameters DefaultBehaviorParameters;	
	/// the current behavior parameters (they can be overridden at times)
	public CharacterBehaviorParameters BehaviorParameters{ get { return _overrideBehaviorParameters ?? DefaultBehaviorParameters; } }
	/// the permissions associated to the character
	public CharacterBehaviorPermissions Permissions ;
	
	[Space(10)]	
	[Header("Particle Effects")]
	/// the effect that will be instantiated everytime the character touches the ground
	public ParticleSystem
		TouchTheGroundEffect;
	/// the effect that will be instantiated everytime the character touches the ground
	public ParticleSystem HurtEffect;
	
	[Space(10)]	
	[Header("Sounds")]
	// the sound to play when the player jumps
	public AudioClip
		PlayerJumpSfx;
	// the sound to play when the player gets hit
	public AudioClip PlayerHitSfx;
	
	/// is true if the character can jump
	public bool JumpAuthorized { 
		get { 
			if ((BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhere) || (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes))
				return true;
			
			if (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpOnGround)
				return _controller.State.IsGrounded;
			
			return false; 
		}
	}
	
	// associated gameobjects and positions
	private CameraController _sceneCamera;
	private CorgiController _controller;

	private Animator _animator;
	private CharacterJetpack _jetpack;
	private CharacterShoot _shoot;
	private Color _initialColor;
	
	// storage for overriding behavior parameters
	private CharacterBehaviorParameters _overrideBehaviorParameters;
	// storage for original gravity and timer
	private float _originalGravity;
	
	// the current normalized horizontal speed
	private float _normalizedHorizontalSpeed;
	
	// pressure timed jumps
	private float _jumpButtonPressTime = 0;
	private bool _jumpButtonPressed = false;
	private bool _jumpButtonReleased = false;
	private int _deltaTimeCounter = 0;
	private float _deltaTimeSum = 0f; 
	
	// true if the player is facing right
	private bool _isFacingRight = true;
	
	// INPUT AXIS
	private float _horizontalMove;
	private float _verticalMove;
	
	/// <summary>
	/// Initializes this instance of the character
	/// </summary>
	void Awake ()
	{		
		BehaviorState = new CharacterBehaviorState ();
		_sceneCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraController> ();
		_controller = GetComponent<CorgiController> ();
		_jetpack = GetComponent<CharacterJetpack> ();
		_shoot = GetComponent<CharacterShoot> ();
		Health = BehaviorParameters.MaxHealth;
		
		if (GetComponent<Renderer> () != null)
			_initialColor = GetComponent<Renderer> ().material.color;
	}
	
	/// <summary>
	/// Further initialization
	/// </summary>
	public void Start ()
	{
		// we get the animator
		_animator = GetComponent<Animator> ();
		// if the width of the character is positive, then it is facing right.
		_isFacingRight = transform.localScale.x > 0;
		
		_originalGravity = _controller.Parameters.Gravity;
		
		// we initialize all the controller's states with their default values.
		BehaviorState.Initialize ();
		BehaviorState.NumberOfJumpsLeft = BehaviorParameters.NumberOfJumps;
		

		BehaviorState.CanJump = true;		
	}
	
	/// <summary>
	/// This is called every frame.
	/// </summary>
	void Update ()
	{		
		// we send our various states to the animator.		
		UpdateAnimator ();
		// if the character is not dead
		if (!BehaviorState.IsDead) {
			GravityActive (true);
			// we handle horizontal and vertical movement				
			HorizontalMovement ();
			VerticalMovement ();			
			
			BehaviorState.CanShoot = true;
			
			// ladder climbing and wall clinging
			ClimbLadder ();
			WallClinging ();
			
			// If the character is dashing, we cancel the gravity
			if (BehaviorState.Dashing) {
				GravityActive (false);
				_controller.SetVerticalForce (0);
			}	
			// if the character is not firing, we reset the firingStop state.
			if (!BehaviorState.Firing) {
				BehaviorState.FiringStop = false;
			}
			// if the character can jump we handle press time controlled jumps
			if (JumpAuthorized) {	
				_deltaTimeSum += Time.deltaTime;
				_deltaTimeCounter += 1;
				// If the user releases the jump button and the character is jumping up and enough time since the initial jump has passed, then we make it stop jumping by applying a force down.
				if ((_jumpButtonPressTime != 0) 
					&& (Time.time - _jumpButtonPressTime >= BehaviorParameters.JumpMinimumAirTime) 
					&& (_controller.Speed.y > Mathf.Sqrt (Mathf.Abs (_controller.Parameters.Gravity))) 
					&& (_jumpButtonReleased)
					&& (!_jumpButtonPressed || BehaviorState.Jetpacking)) {
					_jumpButtonReleased = false;	
					if (BehaviorParameters.JumpIsProportionalToThePressTime) {
						//Debug.Log (_deltaTimeSum / _deltaTimeCounter);
						_controller.AddForce (new Vector2 (0, 12 * -Mathf.Abs (_controller.Parameters.Gravity) * (_deltaTimeSum / _deltaTimeCounter)/*Time.deltaTime*/));			
					}
					_deltaTimeSum = 0f;
					_deltaTimeCounter = 0;
				}
			}			
		} else {	
			// if the character is dead, we prevent it from moving horizontally		
			_controller.SetHorizontalForce (0);
		}
	}
	
	/// <summary>
	/// This is called once per frame, after Update();
	/// </summary>
	void LateUpdate ()
	{
		// if the character became grounded this frame, we reset the doubleJump flag so he can doubleJump again
		if (_controller.State.JustGotGrounded) {
			BehaviorState.NumberOfJumpsLeft = BehaviorParameters.NumberOfJumps;		
		}
		
	}
	
	/// <summary>
	/// This is called at Update() and sets each of the animators parameters to their corresponding State values
	/// </summary>
	private void UpdateAnimator ()
	{	
		
		UpdateAnimatorBool ("Grounded", _controller.State.IsGrounded);
		UpdateAnimatorFloat ("Speed", Mathf.Abs (_controller.Speed.x));
		UpdateAnimatorFloat ("vSpeed", _controller.Speed.y);
		UpdateAnimatorBool ("Running", BehaviorState.Running);
		UpdateAnimatorBool ("Dashing", BehaviorState.Dashing);
		UpdateAnimatorBool ("Crouching", BehaviorState.Crouching);
		UpdateAnimatorBool ("LookingUp", BehaviorState.LookingUp);
		UpdateAnimatorBool ("WallClinging", BehaviorState.WallClinging);
		UpdateAnimatorBool ("Jetpacking", BehaviorState.Jetpacking);
		UpdateAnimatorBool ("Diving", BehaviorState.Diving);
		UpdateAnimatorBool ("LadderClimbing", BehaviorState.LadderClimbing);
		UpdateAnimatorFloat ("LadderClimbingSpeed", BehaviorState.LadderClimbingSpeed);
		UpdateAnimatorBool ("FiringStop", BehaviorState.FiringStop);
		UpdateAnimatorBool ("Firing", BehaviorState.Firing);
		UpdateAnimatorInteger ("FiringDirection", BehaviorState.FiringDirection);
		UpdateAnimatorBool ("MeleeAttacking", BehaviorState.MeleeAttacking);
	}
	
	private void UpdateAnimatorBool (string parameterName, bool value)
	{
		if (_animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Bool))
			_animator.SetBool (parameterName, value);
	}
	private void UpdateAnimatorFloat (string parameterName, float value)
	{
		if (_animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Float))
			_animator.SetFloat (parameterName, value);
	}
	private void UpdateAnimatorInteger (string parameterName, int value)
	{
		if (_animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Int))
			_animator.SetInteger (parameterName, value);
	}
	
	/// <summary>
	/// Sets the horizontal move value.
	/// </summary>
	/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
	public void SetHorizontalMove (float value)
	{
		_horizontalMove = value;
	}
	
	/// <summary>
	/// Sets the vertical move value.
	/// </summary>
	/// <param name="value">Vertical move value, between -1 and 1
	public void SetVerticalMove (float value)
	{
		_verticalMove = value;
	}
	
	/// <summary>
	/// Called at Update(), handles horizontal movement
	/// </summary>
	private void HorizontalMovement ()
	{	
		// if movement is prevented, we exit and do nothing
		if (!BehaviorState.CanMoveFreely)
			return;				
		
		// If the value of the horizontal axis is positive, the character must face right.
		if (_horizontalMove > 0.1) {
			_normalizedHorizontalSpeed = _horizontalMove;
			if (!_isFacingRight)
				Flip ();
		}		
		// If it's negative, then we're facing left
		else if (_horizontalMove < -0.1) {
			_normalizedHorizontalSpeed = _horizontalMove;
			if (_isFacingRight)
				Flip ();
		} else {
			_normalizedHorizontalSpeed = 0;
		}
		
		// we pass the horizontal force that needs to be applied to the controller.
		var movementFactor = _controller.State.IsGrounded ? _controller.Parameters.SpeedAccelerationOnGround : _controller.Parameters.SpeedAccelerationInAir;
		_controller.SetHorizontalForce (Mathf.Lerp (_controller.Speed.x, _normalizedHorizontalSpeed * BehaviorParameters.MovementSpeed, Time.deltaTime * movementFactor));
		
	}
	
	/// <summary>
	/// Called at Update(), handles vertical movement
	/// </summary>
	private void VerticalMovement ()
	{
		
		// Looking up
		if ((_verticalMove > 0) && (_controller.State.IsGrounded)) {
			BehaviorState.LookingUp = true;		
			_sceneCamera.LookUp ();
		} else {
			BehaviorState.LookingUp = false;
			_sceneCamera.ResetLookUpDown ();
		}
	
		// Manages the ground touching effect
		if (_controller.State.JustGotGrounded) {
			Instantiate (TouchTheGroundEffect, new Vector2 (transform.position.x, transform.position.y - transform.localScale.y / 2), transform.rotation);	
		}
		
		// if the character is not in a position where it can move freely, we do nothing.
		if (!BehaviorState.CanMoveFreely)
			return;
		
		// Crouch Detection : if the player is pressing "down" and if the character is grounded and the crouch action is enabled
		if ((_verticalMove < -0.1) && (_controller.State.IsGrounded) && (Permissions.CrouchEnabled)) {
			BehaviorState.Crouching = true;
			BehaviorParameters.MovementSpeed = BehaviorParameters.CrouchSpeed;
			BehaviorState.Running = false;
			_sceneCamera.LookDown ();			
		} else {	
			// if the character is currently crouching, we'll check if it's in a tunnel
			if (BehaviorState.Crouching) {	
				if (HeadCollider == null) {
					BehaviorState.Crouching = false;
					return;
				}
				bool headCheck = Physics2D.OverlapCircle (HeadCollider.transform.position, HeadCollider.size.x / 2, _controller.PlatformMask);			
				// if the character is not crouched anymore, we set 
				if (!headCheck) {
					if (!BehaviorState.Running)
						BehaviorParameters.MovementSpeed = BehaviorParameters.WalkSpeed;
					BehaviorState.Crouching = false;
					BehaviorState.CanJump = true;
				} else {
					
					BehaviorState.CanJump = false;
				}
			}
		}
		
		if (BehaviorState.CrouchingPreviously != BehaviorState.Crouching) {
			Invoke ("RecalculateRays", Time.deltaTime * 10);		
		}
		
		BehaviorState.CrouchingPreviously = BehaviorState.Crouching;
		
		
	}
	
	/// <summary>
	/// Use this method to force the controller to recalculate the rays, especially useful when the size of the character has changed.
	/// </summary>
	public void RecalculateRays ()
	{
		_controller.SetRaysParameters ();
	}
	
	/// <summary>
	/// Causes the character to start running.
	/// </summary>
	public void RunStart ()
	{		
		// if the Run action is enabled in the permissions, we continue, if not we do nothing
		if (!Permissions.RunEnabled)
			return;
		// if the character is not in a position where it can move freely, we do nothing.
		if (!BehaviorState.CanMoveFreely)
			return;
		
		// if the player presses the run button and if we're on the ground and not crouching and we can move freely, 
		// then we change the movement speed in the controller's parameters.
		if (_controller.State.IsGrounded && !BehaviorState.Crouching) {
			BehaviorParameters.MovementSpeed = BehaviorParameters.RunSpeed;
			BehaviorState.Running = true;
		}
	}
	
	/// <summary>
	/// Causes the character to stop running.
	/// </summary>
	public void RunStop ()
	{
		// if the run button is released, we revert back to the walking speed.
		BehaviorParameters.MovementSpeed = BehaviorParameters.WalkSpeed;
		BehaviorState.Running = false;
	}
	
	/// <summary>
	/// Causes the character to start jumping.
	/// </summary>
	public void JumpStart ()
	{
		
		// if the Jump action is enabled in the permissions, we continue, if not we do nothing. If the player is dead, we do nothing.
		if (!Permissions.JumpEnabled || !JumpAuthorized || BehaviorState.IsDead)
			return;
		
		// we check if the character can jump without conflicting with another action
		if (_controller.State.IsGrounded 
			|| BehaviorState.LadderClimbing 
			|| BehaviorState.WallClinging 
			|| BehaviorState.NumberOfJumpsLeft > 0) 	
			BehaviorState.CanJump = true;
		else
			BehaviorState.CanJump = false;
					
		// if the player can't jump, we do nothing. 
		if ((!BehaviorState.CanJump) && !(BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes))
			return;
		
		// if the character is standing on a one way platform (layermask n°11) and is also pressing the down button,
		if (_verticalMove < 0 && _controller.State.IsGrounded) {
			if (_controller.StandingOn.layer == 11) {
				// we make it fall down below the platform by moving it just below the platform
				_controller.transform.position = new Vector2 (transform.position.x, transform.position.y - 0.1f);
				// we turn the boxcollider off for a few milliseconds, so the character doesn't get stuck mid platform
				StartCoroutine (_controller.DisableCollisions (0.3f));
				return;
			}
		}
		
		// we decrease the number of jumps left
		BehaviorState.NumberOfJumpsLeft = BehaviorState.NumberOfJumpsLeft - 1;
		
		BehaviorState.LadderClimbing = false;
		BehaviorState.CanMoveFreely = true;
		GravityActive (true);
		
		_jumpButtonPressTime = Time.time;
		_jumpButtonPressed = true;
		_jumpButtonReleased = false;
				
		_controller.SetVerticalForce (Mathf.Sqrt (2f * BehaviorParameters.JumpHeight * Mathf.Abs (_controller.Parameters.Gravity)));
		
		// we play the jump sound
		if (PlayerJumpSfx != null)
			SoundManager.Instance.PlaySound (PlayerJumpSfx, transform.position);
		
		// wall jump
		float wallJumpDirection;
		if (BehaviorState.WallClinging) {
			
			// If the character is colliding to the right with something (probably the wall)
			if (_controller.State.IsCollidingRight) {
				wallJumpDirection = -1f;
			} else {					
				wallJumpDirection = 1f;
			}
			_controller.SetForce (new Vector2 (wallJumpDirection * BehaviorParameters.WallJumpForce, Mathf.Sqrt (2f * BehaviorParameters.JumpHeight * Mathf.Abs (_controller.Parameters.Gravity))));
			BehaviorState.WallClinging = false;
		}	
		
	}
	
	/// <summary>
	/// Causes the character to stop jumping.
	/// </summary>
	public void JumpStop ()
	{
		_jumpButtonPressed = false;
		_jumpButtonReleased = true;
	}
	
	

	
	/// <summary>
	/// Called at Update(), handles the climbing of ladders
	/// </summary>	
	void ClimbLadder ()
	{
		// if the character is colliding with a ladder
		if (BehaviorState.LadderColliding) {
			// if the player is pressing the up key and not yet climbing a ladder, and not colliding with the top platform and not jetpacking
			if (_verticalMove > 0.1 && !BehaviorState.LadderClimbing && !BehaviorState.LadderTopColliding && !BehaviorState.Jetpacking) {			
				// then the character starts climbing
				BehaviorState.LadderClimbing = true;
				_controller.CollisionsOn ();
				
				// it can't move freely anymore
				BehaviorState.CanMoveFreely = false;
				// we make it stop shooting
				if (_shoot != null)
					_shoot.ShootStop ();
				// we initialize the ladder climbing speed to zero
				BehaviorState.LadderClimbingSpeed = 0;
				// we make sure the controller won't move
				_controller.SetHorizontalForce (0);
				_controller.SetVerticalForce (0);
				// we disable the gravity
				GravityActive (false);
			}			
			
			// if the character is climbing the ladder (which means it previously connected with it)
			if (BehaviorState.LadderClimbing) {
				// we prevent it from shooting
				BehaviorState.CanShoot = false;
				// we disable the gravity
				GravityActive (false);
				
				if (!BehaviorState.LadderTopColliding)
					_controller.CollisionsOn ();
				
				// we set the vertical force according to the ladder climbing speed
				_controller.SetVerticalForce (_verticalMove * BehaviorParameters.LadderSpeed);
				// we set pass that speed to the climbing speed state.
				BehaviorState.LadderClimbingSpeed = Mathf.Abs (_verticalMove);				
			}
			
			// if the character is grounded AND climbing
			if (BehaviorState.LadderClimbing && _controller.State.IsGrounded && !BehaviorState.LadderTopColliding) {				
				// we make it stop climbing, it has reached the ground.
				BehaviorState.LadderColliding = false;
				BehaviorState.LadderClimbing = false;
				BehaviorState.CanMoveFreely = true;
				BehaviorState.LadderClimbingSpeed = 0;	
				GravityActive (true);			
			}			
		}
		
		// If the character is colliding with the top of the ladder and is pressing down and is not on the ladder yet and is standing on the ground, we make it go down.
		if (BehaviorState.LadderTopColliding && _verticalMove < -0.1 && !BehaviorState.LadderClimbing && _controller.State.IsGrounded) {			
			_controller.CollisionsOff ();
			// we force its position to be a bit lower
			transform.position = new Vector2 (transform.position.x, transform.position.y - 0.1f);
			// we initiate the climbing.
			BehaviorState.LadderClimbing = true;
			BehaviorState.CanMoveFreely = false;
			BehaviorState.LadderClimbingSpeed = 0;			
			_controller.SetHorizontalForce (0);
			_controller.SetVerticalForce (0);
			GravityActive (false);
		}		
	}
	
	/// <summary>
	/// Causes the character to dash or dive (depending on the vertical movement at the start of the dash)
	/// </summary>
	public void Dash ()
	{	
		// declarations	
		float _dashDirection;
		float _boostForce;
				
		// if the Dash action is enabled in the permissions, we continue, if not we do nothing
		if (!Permissions.DashEnabled || BehaviorState.IsDead)
			return;
		// if the character is not in a position where it can move freely, we do nothing.
		if (!BehaviorState.CanMoveFreely)
			return;
		
		// If the user presses the dash button and is not aiming down
		if (_verticalMove > -0.8) {	
			// if the character is allowed to dash
			if (BehaviorState.CanDash) {
				// we set its dashing state to true
				BehaviorState.Dashing = true;
				
				// depending on its direction, we calculate the dash parameters to apply				
				if (_isFacingRight) {
					_dashDirection = 1f;
				} else {
					_dashDirection = -1f;
				}
				_boostForce = _dashDirection * BehaviorParameters.DashForce;
				BehaviorState.CanDash = false;
				// we launch the boost corountine with the right parameters
				StartCoroutine (Boost (BehaviorParameters.DashDuration, _boostForce, 0, "dash"));
			}			
		}
		// if the user presses the dash button and is aiming down
		if (_verticalMove < -0.8) {
			// we start the dive coroutine
			StartCoroutine (Dive ());
		}		
		
	}
	
	/// <summary>
	/// Coroutine used to move the player in a direction over time
	/// </summary>
	IEnumerator Boost (float boostDuration, float boostForceX, float boostForceY, string name)
	{
		float time = 0f; 
		
		// for the whole duration of the boost
		while (boostDuration > time) {
			// we add the force passed as a parameter
			if (boostForceX != 0) {
				_controller.AddForce (new Vector2 (boostForceX, 0));
			}
			if (boostForceY != 0) {
				_controller.AddForce (new Vector2 (0, boostForceY));
			}
			time += Time.deltaTime;
			// we keep looping for the duration of the boost
			yield return 0; 
		}
		// once the boost is complete, if we were dashing, we make it stop and start the dash cooldown
		if (name == "dash") {
			BehaviorState.Dashing = false;
			GravityActive (true);
			yield return new WaitForSeconds (BehaviorParameters.DashCooldown); 
			BehaviorState.CanDash = true; 
		}	
		if (name == "wallJump") {
			// so far we do nothing, but you could use it to trigger a sound or an effect when walljumping
		}		
	}
	
	/// <summary>
	/// Coroutine used to make the player dive vertically
	/// </summary>
	IEnumerator Dive ()
	{	
		// Shake parameters : intensity, duration (in seconds) and decay
		Vector3 ShakeParameters = new Vector3 (1.5f, 0.5f, 1f);
		BehaviorState.Diving = true;
		// while the player is not grounded, we force it to go down fast
		while (!_controller.State.IsGrounded) {
			_controller.SetVerticalForce (-Mathf.Abs (_controller.Parameters.Gravity) * 2);
			yield return 0; //go to next frame
		}
		
		// once the player is grounded, we shake the camera, and restore the diving state to false
		_sceneCamera.Shake (ShakeParameters);		
		BehaviorState.Diving = false;
	}
	
	/// <summary>
	/// Makes the player stick to a wall when jumping
	/// </summary>
	private void WallClinging ()
	{
		// if the wall clinging action is enabled in the permissions, we continue, if not we do nothing
		if (!Permissions.WallClingingEnabled)
			return;
			
		if (!_controller.State.IsCollidingLeft && !_controller.State.IsCollidingRight) {
			BehaviorState.WallClinging = false;
		}
		
		// if the character is not in a position where it can move freely, we do nothing.
		if (!BehaviorState.CanMoveFreely)
			return;
		
		// if the character is in the air and touching a wall and moving in the opposite direction, then we slow its fall
		if ((!_controller.State.IsGrounded) && (((_controller.State.IsCollidingRight) && (_horizontalMove > -0.1f)) || ((_controller.State.IsCollidingLeft) && (_horizontalMove < 0.1f)))) {
			if (_controller.Speed.y < 0) {
				BehaviorState.WallClinging = true;
				_controller.SlowFall (BehaviorParameters.WallClingingSlowFactor);
			}
		} else {
			BehaviorState.WallClinging = false;
			_controller.SlowFall (0f);
		}
	}
	
	/// <summary>
	/// Activates or desactivates the gravity for this character only.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, activates the gravity. If set to <c>false</c>, turns it off.</param>
	private void GravityActive (bool state)
	{
		if (state == true) {
			if (_controller.Parameters.Gravity == 0) {
				_controller.Parameters.Gravity = _originalGravity;
			}
		} else {
			if (_controller.Parameters.Gravity != 0)
				_originalGravity = _controller.Parameters.Gravity;
			_controller.Parameters.Gravity = 0;
		}
	}
	
	/// <summary>
	/// Coroutine used to make the character's sprite flicker (when hurt for example).
	/// </summary>
	IEnumerator Flicker (Color initialColor, Color flickerColor, float flickerSpeed)
	{
		if (GetComponent<Renderer> () != null) {			
			for (var n = 0; n < 10; n++) {
				GetComponent<Renderer> ().material.color = initialColor;
				yield return new WaitForSeconds (flickerSpeed);
				GetComponent<Renderer> ().material.color = flickerColor;
				yield return new WaitForSeconds (flickerSpeed);
			}
			GetComponent<Renderer> ().material.color = initialColor;
		}				
	}
	
	/// <summary>
	/// makes the character colliding again with layer 12 (Projectiles) and 13 (Enemies)
	/// </summary>
	/// <returns>The layer collision.</returns>
	IEnumerator ResetLayerCollision (float delay)
	{
		yield return new WaitForSeconds (delay);
		Physics2D.IgnoreLayerCollision (9, 12, false);
		Physics2D.IgnoreLayerCollision (9, 13, false);
	}
	
	/// <summary>
	/// Kills the character, sending it in the air
	/// </summary>
	public void Kill ()
	{
		// we make it ignore the collisions from now on
		_controller.CollisionsOff ();
		GetComponent<Collider2D> ().enabled = false;
		// we set its dead state to true
		BehaviorState.IsDead = true;
		// we set its health to zero (useful for the healthbar)
		Health = 0;
		// we send it in the air
		_controller.ResetParameters ();
		ResetParameters ();
		_controller.SetForce (new Vector2 (0, 10));		
	}
	
	/// <summary>
	/// Called to disable the player (at the end of a level for example. 
	/// It won't move and respond to input after this.
	/// </summary>
	public void Disable ()
	{
		enabled = false;
		_controller.enabled = false;
		GetComponent<Collider2D> ().enabled = false;		
	}
	
	/// <summary>
	/// Makes the player respawn at the location passed in parameters
	/// </summary>
	/// <param name="spawnPoint">The location of the respawn.</param>
	public void RespawnAt (Transform spawnPoint)
	{
		// we make sure the character is facing right
		if (!_isFacingRight) {
			Flip ();
		}
		// we raise it from the dead (if it was dead)
		BehaviorState.IsDead = false;
		// we re-enable its 2D collider
		GetComponent<Collider2D> ().enabled = true;
		// we make it handle collisions again
		_controller.CollisionsOn ();
		transform.position = spawnPoint.position;
		Health = BehaviorParameters.MaxHealth;
	}
	
	/// <summary>
	/// Called when the player takes damage
	/// </summary>
	/// <param name="damage">The damage applied.</param>
	/// <param name="instigator">The damage instigator.</param>
	public void TakeDamage (int damage, GameObject instigator)
	{
		// we play the sound the player makes when it gets hit
		if (PlayerHitSfx != null)
			SoundManager.Instance.PlaySound (PlayerHitSfx, transform.position);
				
		// When the character takes damage, we create an auto destroy hurt particle system
		Instantiate (HurtEffect, transform.position, transform.rotation);
		// we prevent the character from colliding with layer 12 (Projectiles) and 13 (Enemies)
		Physics2D.IgnoreLayerCollision (9, 12, true);
		Physics2D.IgnoreLayerCollision (9, 13, true);
		StartCoroutine (ResetLayerCollision (0.5f));
		// We make the character's sprite flicker
		if (GetComponent<Renderer> () != null) {
			Color flickerColor = new Color32 (255, 20, 20, 255); 
			StartCoroutine (Flicker (_initialColor, flickerColor, 0.05f));	
		}	
		// we decrease the character's health by the damage
		Health -= damage;
		if (Health <= 0) {
			LevelManager.Instance.KillPlayer ();
		}
	}
	
	/// <summary>
	/// Called when the character gets health (from a stimpack for example)
	/// </summary>
	/// <param name="health">The health the character gets.</param>
	/// <param name="instigator">The thing that gives the character health.</param>
	public void GiveHealth (int health, GameObject instigator)
	{
		// this function adds health to the character's Health and prevents it to go above MaxHealth.
		Health = Mathf.Min (Health + health, BehaviorParameters.MaxHealth);
	}
	
	/// <summary>
	/// Flips the character and its dependencies (jetpack for example) horizontally
	/// </summary>
	private void Flip ()
	{
		// Flips the character horizontally
		transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		_isFacingRight = transform.localScale.x > 0;
		
		// we flip the emitters individually because they won't flip otherwise.
		
		if (_jetpack != null) {
			if (_jetpack.Jetpack != null)
				_jetpack.Jetpack.transform.eulerAngles = new Vector3 (_jetpack.Jetpack.transform.eulerAngles.x, _jetpack.Jetpack.transform.eulerAngles.y + 180, _jetpack.Jetpack.transform.eulerAngles.z);
		}
		if (_shoot != null) {
			_shoot.Flip ();		
		}
	}
	
	
	public void ResetParameters ()
	{
		_overrideBehaviorParameters = DefaultBehaviorParameters;
	}
	
	/// <summary>
	/// Called when the character collides with something else
	/// </summary>
	/// <param name="other">The other collider.</param>
	public void OnTriggerEnter2D (Collider2D collider)
	{
		
		var parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D> ();
		if (parameters == null)
			return;
		// if the other collider has behavior parameters, we override ours with them.
		_overrideBehaviorParameters = parameters.BehaviorParameters;
	}	
	
	/// <summary>
	/// Called when the character is colliding with something else
	/// </summary>
	/// <param name="other">The other collider.</param>
	public void OnTriggerStay2D (Collider2D collider)
	{
	}	
	
	/// <summary>
	/// Called when the character exits a collider
	/// </summary>
	/// <param name="collider">The other collider.</param>
	public void OnTriggerExit2D (Collider2D collider)
	{		
		
		var parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D> ();
		if (parameters == null)
			return;
		
		// if the other collider had behavior parameters, we reset ours
		_overrideBehaviorParameters = null;
	}	
}
