using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Camera))]
/// <summary>
/// The Corgi Engine's Camera Controller. Handles camera movement, shakes, player follow.
/// </summary>
public class CameraController : MonoBehaviour 
{
	
	/// True if the camera should follow the player
	public bool FollowsPlayer{get;set;}
	
	[Space(10)]	
	[Header("Distances")]
	/// How far ahead from the Player the camera is supposed to be		
	public float HorizontalLookDistance = 3;
	/// How high (or low) from the Player the camera should move when looking up/down
	public float VerticalLookDistance = 3;
	/// Minimal distance that triggers look ahead
	public float LookAheadTrigger = 0.1f;
	
	
	[Space(10)]	
	[Header("Movement Speed")]
	/// How fast the camera goes back to the Player
	public float ResetSpeed = 0.5f;
	/// How fast the camera moves
	public float CameraSpeed = 0.3f;
	
	[Space(10)]	
	[Header("Camera Zoom")]
	[Range (1, 20)]
	public float MinimumZoom=5f;
	[Range (1, 20)]
	public float MaximumZoom=10f;	
	public float ZoomSpeed=0.4f;
	
	// Private variables
	
	private Transform _target;
	private CorgiController _targetController;
	private LevelLimits _levelBounds;
	
	private float xMin;
	private float xMax;
	private float yMin;
	private float yMax;	
	
	private float offsetZ;
	private Vector3 lastTargetPosition;
	private Vector3 currentVelocity;
	private Vector3 lookAheadPos;
	
	private float shakeIntensity;
	private float shakeDecay;
	private float shakeDuration;
	
	private float _currentZoom;	
	private Camera _camera;
	
	private Vector3 _lookDirectionModifier;
	
	/// <summary>
	/// Initialization
	/// </summary>
	void Start ()
	{		
		// we get the camera component
		_camera=GetComponent<Camera>();
	
		// We make the camera follow the player
		FollowsPlayer=true;
		_currentZoom=MinimumZoom;
		
		// player and level bounds initialization
		_target = GameManager.Instance.Player.transform;
		if (_target.GetComponent<CorgiController>()==null)
			return;
		_targetController=_target.GetComponent<CorgiController>();
		_levelBounds = GameObject.FindGameObjectWithTag("LevelBounds").GetComponent<LevelLimits>();		
		
		// we store the target's last position
		lastTargetPosition = _target.position;
		offsetZ = (transform.position - _target.position).z;
		transform.parent = null;
		
		_lookDirectionModifier=new Vector3(0,0,0);
		
		Zoom();
			
			
	}
	
	
	/// <summary>
	/// Every frame, we move the camera if needed
	/// </summary>
	void LateUpdate () 
	{
		// if the camera is not supposed to follow the player, we do nothing
		if (!FollowsPlayer)
			return;
			
		Zoom();
			
		// if the player has moved since last update
		float xMoveDelta = (_target.position - lastTargetPosition).x;
		
		bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadTrigger;
		
		if (updateLookAheadTarget) 
		{
			lookAheadPos = HorizontalLookDistance * Vector3.right * Mathf.Sign(xMoveDelta);
		} 
		else 
		{
			lookAheadPos = Vector3.MoveTowards(lookAheadPos, Vector3.zero, Time.deltaTime * ResetSpeed);	
		}
		
		Vector3 aheadTargetPos = _target.position + lookAheadPos + Vector3.forward * offsetZ + _lookDirectionModifier;
				
		Vector3 newCameraPosition = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref currentVelocity, CameraSpeed);
				
		Vector3 shakeFactorPosition = new Vector3(0,0,0);
		
		// If shakeDuration is still running.
		if (shakeDuration>0)
		{
			shakeFactorPosition= Random.insideUnitSphere * shakeIntensity * shakeDuration;
			shakeDuration-=shakeDecay*Time.deltaTime ;
		}		
		newCameraPosition = newCameraPosition+shakeFactorPosition;		
		
		// Clamp to level boundaries
		float posX = Mathf.Clamp(newCameraPosition.x, xMin, xMax);
		float posY = Mathf.Clamp(newCameraPosition.y, yMin, yMax);
		float posZ = newCameraPosition.z;
		
		// We move the actual transform
		transform.position=new Vector3(posX, posY, posZ);
		
		lastTargetPosition = _target.position;		
	}
	
	/// <summary>
	/// Handles the zoom of the camera based on the main character's speed
	/// </summary>
	private void Zoom()
	{
	
		float characterSpeed=Mathf.Abs(_targetController.Speed.x);
		float currentVelocity=0f;
		
		_currentZoom=Mathf.SmoothDamp(_currentZoom,(characterSpeed/10)*(MaximumZoom-MinimumZoom)+MinimumZoom,ref currentVelocity,ZoomSpeed);
			
		_camera.orthographicSize=_currentZoom;
		GetLevelBounds();
	}
	
	/// <summary>
	/// Gets the levelbounds coordinates to lock the camera into the level
	/// </summary>
	private void GetLevelBounds()
	{
		// camera size calculation (orthographicSize is half the height of what the camera sees.
		float cameraHeight = Camera.main.orthographicSize * 2f;		
		float cameraWidth = cameraHeight * Camera.main.aspect;
		
		xMin = _levelBounds.LeftLimit+(cameraWidth/2);
		xMax = _levelBounds.RightLimit-(cameraWidth/2); 
		yMin = _levelBounds.BottomLimit+(cameraHeight/2); 
		yMax = _levelBounds.TopLimit-(cameraHeight/2);	
	}
	
	/// <summary>
	/// Use this method to shake the camera, passing in a Vector3 for intensity, duration and decay
	/// </summary>
	/// <param name="shakeParameters">Shake parameters : intensity, duration and decay.</param>
	public void Shake(Vector3 shakeParameters)
	{
		shakeIntensity = shakeParameters.x;
		shakeDuration=shakeParameters.y;
		shakeDecay=shakeParameters.z;
	}

	/// <summary>
	/// Moves the camera up
	/// </summary>
	public void LookUp()
	{
		_lookDirectionModifier = new Vector3(0,VerticalLookDistance,0);
	}	
	/// <summary>
	/// Moves the camera down
	/// </summary>
	public void LookDown()
	{
		_lookDirectionModifier = new Vector3(0,-VerticalLookDistance,0);
	}
	/// <summary>
	/// Resets the look direction modifier
	/// </summary>
	public void ResetLookUpDown()
	{	
		_lookDirectionModifier = new Vector3(0,0,0);
	}
	
	
}