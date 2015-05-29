using UnityEngine;
using System.Collections;


public class LimitFps : MonoBehaviour
{
	public int vSync;
	public int desiredFrameRate;


	void Start ()
	{
		QualitySettings.vSyncCount = vSync;
		Application.targetFrameRate = desiredFrameRate;
	}
}
