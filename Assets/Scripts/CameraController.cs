using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public GameObject player;
	public BoxCollider2D bounds;
	public Vector2 leash;

	private Vector3 _min;
	private Vector3 _max;
	private Transform _playerTransform;
	private Player _player;


	void Start ()
	{
		_min = bounds.bounds.min;
		_max = bounds.bounds.max;
		_playerTransform = player.GetComponent<Transform> ();
		_player = player.GetComponent<Player> ();
	}

	void Update ()
	{

		//Debug.Log (Camera.main.orthographicSize * ((float)Screen.width / Screen.height));

		//Debug.Log (Mathf.Abs (player.position.x) - Mathf.Abs (transform.position.x));

		//DrawLeash ();

		//Debug.Log ("CAMERA: " + (transform.position.x - leash.x / 2));
		//Debug.Log ("PLAYER: " + _playerTransform.position.x);
		
		float x = CheckXAxis ();
		float y = CheckYAxis ();


		//if(_player.lastGroundedLevel >)

		transform.position = new Vector3 (x, y, transform.position.z);
	}

	float CheckXAxis ()
	{
		float x = transform.position.x;
		
		if (false) {
			
		} else if (_playerTransform.position.x < (transform.position.x - leash.x / 2)) {
			//Debug.Log ("OUT TO THE LEFT!");
			x = _playerTransform.position.x + leash.x / 2;
		} else if (_playerTransform.position.x > (transform.position.x + leash.x / 2)) {
			//Debug.Log ("OUT TO THE RIGHT!");
			x = _playerTransform.position.x - leash.x / 2;
		}

		return x;
	}

	float CheckYAxis ()
	{
		float y = transform.position.y;

		if (false) {
			
		} else if (_playerTransform.position.y < (transform.position.y - leash.y / 2)) {
			//Debug.Log ("OUT TO THE BOTTOM!");
			y = _playerTransform.position.y + leash.y / 2;
		} else if (_playerTransform.position.y > (transform.position.y + leash.y / 2)) {
			//Debug.Log ("OUT TO THE TOP!");
			y = _playerTransform.position.y - leash.y / 2;
		}

		return y;
	}

	void DrawLeash ()// debug... kidna useless actually
	{


		float y = _player.lastGroundedLevel - _playerTransform.localScale.y;

		Debug.DrawLine (new Vector3 (transform.position.x - leash.x / 2, y, _playerTransform.position.z), 
		                new Vector3 (transform.position.x + leash.x / 2, y, _playerTransform.position.z),
		                Color.white, 0.0f, false);
		Debug.DrawLine (new Vector3 (transform.position.x - leash.x / 2, y + leash.y, _playerTransform.position.z), 
		                new Vector3 (transform.position.x + leash.x / 2, y + leash.y, _playerTransform.position.z),
		                Color.white, 0.0f, false);
		Debug.DrawLine (new Vector3 (transform.position.x - leash.x / 2, y, _playerTransform.position.z), 
		                new Vector3 (transform.position.x - leash.x / 2, y + leash.y, _playerTransform.position.z),
		                Color.white, 0.0f, false);
		Debug.DrawLine (new Vector3 (transform.position.x + leash.x / 2, y, _playerTransform.position.z), 
		                new Vector3 (transform.position.x + leash.x / 2, y + leash.y, _playerTransform.position.z),
		                Color.white, 0.0f, false);
	}
}