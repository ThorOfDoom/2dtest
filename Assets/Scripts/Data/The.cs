using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class The : MonoBehaviour
{

	public static The data;

	public int EasyModuleCount;
	public int MediumModuleCount;
	public int HardModuleCount;
	public int TextureCount;
	public Dictionary<string, int[]> BackgroundTextureCounts;

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
		GetBackgroundTextureCounts ();
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

	void GetBackgroundTextureCounts ()
	{
		BackgroundTextureCounts = new Dictionary<string, int[]> ();

		var tier1 = Resources.LoadAll ("Backgrounds/Easy/1", typeof(TextAsset));
		var tier2 = Resources.LoadAll ("Backgrounds/Easy/2", typeof(TextAsset));
		var tier3 = Resources.LoadAll ("Backgrounds/Easy/3", typeof(TextAsset));
		BackgroundTextureCounts.Add ("Easy", new int[3]{tier1.Length, tier2.Length, tier3.Length});
		/*
		tier1 = Resources.LoadAll ("Backgrounds/Medium/1", typeof(TextAsset));
		tier2 = Resources.LoadAll ("Backgrounds/Medium/2", typeof(TextAsset));
		tier3 = Resources.LoadAll ("Backgrounds/Medium/3", typeof(TextAsset));
		BackgroundTextureCounts.Add ("Medium", new int[3]{tier1.Length, tier2.Length, tier3.Length});
		
		tier1 = Resources.LoadAll ("Backgrounds/Hard/1", typeof(TextAsset));
		tier2 = Resources.LoadAll ("Backgrounds/Hard/2", typeof(TextAsset));
		tier3 = Resources.LoadAll ("Backgrounds/Hard/3", typeof(TextAsset));
		BackgroundTextureCounts.Add ("Hard", new int[3]{tier1.Length, tier2.Length, tier3.Length});
		*/
	}
}
