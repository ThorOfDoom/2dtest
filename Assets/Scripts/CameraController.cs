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
	private float _cameraOrthographicWidth;
	private Vector2 _leash;
	private float _lastGround;


	void Start ()
	{
		_min = bounds.bounds.min;
		_max = bounds.bounds.max;
		_playerTransform = player.GetComponent<Transform> ();
		_player = player.GetComponent<Player> ();
		_cameraOrthographicWidth = Camera.main.orthographicSize * Camera.main.aspect;
		_leash = new Vector2 (leash.x / 2, leash.y / 2);
	}

	void Update ()
	{
		DrawLeash ();

		float x = CheckXAxis ();
		float y = CheckYAxis ();

		//if(_player.lastGroundedLevel >)

		transform.position = new Vector3 (x, y, transform.position.z);
	}

	float CheckXAxis ()
	{
		float x = transform.position.x;

		// Left movement
		if (_playerTransform.position.x < (transform.position.x - _leash.x)) {
			x = _playerTransform.position.x + _leash.x;

			if (x < (_min.x + _cameraOrthographicWidth)) {
				x = _min.x + _cameraOrthographicWidth;
			}

			return x;
		}

		// Right movement
		if (_playerTransform.position.x > (transform.position.x + _leash.x)) {
			x = _playerTransform.position.x - _leash.x;

			if (x > (_max.x - _cameraOrthographicWidth)) {
				x = _max.x - _cameraOrthographicWidth;
			}

			return x;
		}

		return x;
	}

	float CheckYAxis ()
	{
		float y = transform.position.y;

		// Downward movement
		if (_playerTransform.position.y < (transform.position.y - _leash.y)) {
			y = _playerTransform.position.y + _leash.y;

			/*if (!float.IsNaN (_player.lastGroundedLevel) && _player.grounded && (_player.lastGroundedLevel + _leash.y) < y) {
				_lastGround = _player.lastGroundedLevel;

				y = _lastGround + _leash.y;
			}*/

			if (y < (_min.y + Camera.main.orthographicSize)) {
				y = _min.y + Camera.main.orthographicSize;
			}

			return y;
		} 

		// Upward movement
		if (_playerTransform.position.y > (transform.position.y + _leash.y)) {
			y = _playerTransform.position.y - _leash.y;

			if (y > (_max.y - Camera.main.orthographicSize)) {
				y = _max.y - Camera.main.orthographicSize;
			}

			return y;
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