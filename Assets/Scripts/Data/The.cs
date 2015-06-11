using UnityEngine;
using System.Collections;
using System.IO;

public class The : MonoBehaviour
{

	public static The data;

	public int EasyModuleCount;
	public int MediumModuleCount;
	public int HardModuleCount;
	public int TextureCount;

	void Awake ()
	{
		if (data == null) {
			DontDestroyOnLoad (gameObject);
			data = this;
		} else if (data != this) {
			Destroy (gameObject);
		}
	}

	void Start ()
	{
		SetValues ();
	}

	void SetValues ()
	{
		GetEasyModuleCount ();
		GetMediumModuleCount ();
		GetHardModuleCount ();
		GetTextureCount ();
	}

	void GetEasyModuleCount ()
	{
		var modules = Resources.LoadAll ("Modules/Easy", typeof(TextAsset));
		EasyModuleCount = modules.Length;
	}
	
	void GetMediumModuleCount ()
	{
		var modules = Resources.LoadAll ("Modules/Medium", typeof(TextAsset));
		MediumModuleCount = modules.Length;
	}
	
	void GetHardModuleCount ()
	{
		var modules = Resources.LoadAll ("Modules/Hard", typeof(TextAsset));
		HardModuleCount = modules.Length;
	}
	
	void GetTextureCount ()
	{// textures are allways mad ein sets so we only need to check one directory
		var textures = Resources.LoadAll ("Textures/Easy", typeof(Material));
		TextureCount = textures.Length;
	}
}
