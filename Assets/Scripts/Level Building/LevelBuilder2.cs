using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelBuilder2 : MonoBehaviour
{
	// the total counts will be replaced in the preloader for the game
	public string tier;
	public int level;
	public int totalEasyLevels;
	public int totalMediumLevels;
	public int totalHardLevels;
	public int totalEasyModules;
	public int totalMediumModules;
	public int totalHardModules;
	public int totalTextureCount;
	public GameObject blockToUse;
	public GameObject spikeToUse;
	public GameObject module;

	int[] levelFormat;
	string[][] moduleSequence;
	string textureCode;
	ModuleData[] moduleData;
	int numberOfModules;
	int yOffset;

	// Use this for initialization
	void Start ()
	{
		Init ();
		GenerateLevelFormat ();
		GenerateModuleSequence ();
		PopulateModuleData ();
		BuildLevel ();
	}
    
	void Init ()
	{
		// file name count starts at 1
		totalEasyLevels++;
		totalEasyModules++;
		totalMediumLevels++;
		totalMediumModules++;
		totalHardLevels++;
		totalHardModules++;
		totalTextureCount++;
		if (levelFormat == null) {
			levelFormat = new int[3];
		}
		numberOfModules = 0;
		yOffset = 0;
	}


	void GenerateLevelFormat ()
	{
		string[] tempStructure;
		int compareableTextureCode;
		// EasyModules,MediumModules,HardModlues,Texture | texture == 0 ? use random texture
		TextAsset rawLevelData = (TextAsset)Resources.Load ("Levels/" + tier + "/" + level, typeof(TextAsset));
		if (rawLevelData != null) {
			tempStructure = rawLevelData.text.Split (',');
			
			if (!int.TryParse (tempStructure [0], out levelFormat [0]) || 
				!int.TryParse (tempStructure [1], out levelFormat [1]) || 
				!int.TryParse (tempStructure [2], out levelFormat [2]) || 
				!int.TryParse (tempStructure [3], out compareableTextureCode)) {
				Debug.Log ("Error: could not parse level structure \n");
			}
			if (compareableTextureCode == 0) { // choose random texture 
				textureCode = Random.Range (1, totalTextureCount).ToString ();
			} else {
				textureCode = tempStructure [3].Trim ();
			}
		} else {
			Debug.Log ("Error: could not load level data located at: \"Resources/Levels/" + 
				tier + "/" + level + ".txt\" file not found");
		}
	}


	void GenerateModuleSequence ()
	{
		int totalModuleCount = levelFormat [0] + levelFormat [1] + levelFormat [2];
		moduleSequence = new string[totalModuleCount][];
		int rangeCap;
		for (int i = 0; i < totalModuleCount; i++) {
			moduleSequence [i] = new string[2];
			if (i < levelFormat [0]) {
				rangeCap = totalEasyModules;
				moduleSequence [i] [0] = "Easy";
			} else if (i < levelFormat [0] + levelFormat [1]) {
				rangeCap = totalMediumModules;
				moduleSequence [i] [0] = "Medium";
			} else {
				rangeCap = totalHardModules;
				moduleSequence [i] [0] = "Hard";
			}
			moduleSequence [i] [1] = Random.Range (1, rangeCap).ToString ();
		}
		Shuffle (moduleSequence);
	}
    

	static void Shuffle<T> (T[] array)
	{
		int n = array.Length;
		for (int i = 0; i < n; i++) {
			int r = i + (int)(Random.value * (n - i));
			T t = array [r];
			array [r] = array [i];
			array [i] = t;
		}
	}


	void PopulateModuleData ()
	{
		moduleData = new ModuleData[moduleSequence.Length];
		int n = moduleSequence.Length;
		for (int i = 0; i < n; i++) {
			List<Vector4> blockData = new List<Vector4> ();
			List<Vector4> spikeData = new List<Vector4> ();

			TextAsset textFile = (TextAsset)Resources.Load ("Modules/" + moduleSequence [i] [0] + "/" + 
				moduleSequence [i] [1], typeof(TextAsset));
			if (textFile == null) {
				Debug.Log ("Error: Modules/" + moduleSequence [i] [0] + "/" + moduleSequence [i] [1] + " no such file");
			}
			System.IO.StringReader textStream = new System.IO.StringReader (textFile.text);
			string line;
			while ((line = textStream.ReadLine()) != null) {
				if (!string.IsNullOrEmpty (line)) {
					string[] rawData = new string[4];
					rawData = line.Split (',');
					if (rawData [0].Equals ("blk")) {
						blockData.Add (new Vector4 (float.Parse (rawData [1]), float.Parse (rawData [2]), 
						                            float.Parse (rawData [3]), float.Parse (rawData [4])));
					} else if (rawData [0].Equals ("spk")) {
						spikeData.Add (new Vector4 (float.Parse (rawData [1]), float.Parse (rawData [2]), 
						                            float.Parse (rawData [3]), float.Parse (rawData [4])));
					} else if (rawData [0].Equals ("stt")) {
						moduleData [i].startPointHeight = (int)float.Parse (rawData [1]);
					} else if (rawData [0].Equals ("end")) {
						moduleData [i].endPointHeight = (int)float.Parse (rawData [1]);
					} else {
						Debug.Log ("Error loading module: invalid formating.");
					}
				}
			}
			moduleData [i].blocks = blockData.ToArray ();
			moduleData [i].spikes = spikeData.ToArray ();
			moduleData [i].blockToUse = blockToUse;
			moduleData [i].blockTexture = (Material)Resources.Load ("Textures/" + moduleSequence [i] [0] + "/" + 
				textureCode, typeof(Material));
			if (moduleData [i].blockTexture == null) {
				Debug.Log ("no texture found at: Textures/" + moduleSequence [i] [0] + "/" + 
					textureCode);
			}
			moduleData [i].spikeToUse = spikeToUse;
		}
	}


	void BuildLevel ()
	{
		int n = moduleData.Length;
		for (int i = 0; i < n; i++) {
			GameObject tempModule = (GameObject)Instantiate (module);
			ModuleBuilder2 mb = tempModule.GetComponent<ModuleBuilder2> ();
			mb.moduleData = moduleData [i];
			yOffset = mb.Build (numberOfModules++, yOffset);
			tempModule.transform.parent = transform;
		}
	}
}

public struct ModuleData
{
	public Vector4[] blocks; // this is an example of a  feature vector ???
	public Vector4[] spikes;
	public int startPointHeight;
	public int endPointHeight;
	public GameObject blockToUse;
	public Material blockTexture;
	public GameObject spikeToUse;
}
