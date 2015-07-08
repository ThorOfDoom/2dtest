using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelBuilder2 : MonoBehaviour
{
	// the total counts will be replaced in the preloader for the game
	public string tier;
	public int level;
	public bool buildAll;
	public bool buildMix;
	public bool spawnEnemies = true;
	public GameObject blockToUse;
	public GameObject spikeToUse;
	public GameObject enemyToUse;
	public GameObject module;
	public string debugSequence; // tier|levelnumber,tier|levelnumber,...

	int easyModuleCount;
	int mediumModulesCount;
	int hardModuleCount;
	int textureCount;
	int[] levelFormat;
	string[][] moduleSequence;
	string textureCode;
	ModuleData[] moduleData;
	int numberOfModules;
	int yOffset;
	BoxCollider2D levelBounds;
	Vector2 playerPosition;

	// Use this for initialization
	void Start ()
	{
		Init ();
		if (!string.IsNullOrEmpty (debugSequence)) {
			textureCode = Random.Range (1, textureCount).ToString ();
			GenerateDebugModuleSequence (debugSequence);
		} else if (buildAll) {
			textureCode = Random.Range (1, textureCount).ToString ();
			GenerateDebugModuleSequence ();
		} else if (buildMix) {	
			levelFormat = new int[]{5,5,0};
			textureCode = Random.Range (1, textureCount).ToString ();
			GenerateModuleSequence ();
		} else {
			GenerateLevelFormat ();
			GenerateModuleSequence ();
		}
		PopulateModuleData ();
		BuildLevel ();
		PlacePlayer ();
	}
    
	void Init ()
	{
		levelBounds = GetComponentInChildren<BoxCollider2D> ();
		easyModuleCount = The.data.EasyModuleCount;
		mediumModulesCount = The.data.MediumModuleCount;
		hardModuleCount = The.data.HardModuleCount;
		textureCount = The.data.TextureCount + 1;
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
				textureCode = Random.Range (1, textureCount).ToString ();
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
				rangeCap = easyModuleCount + 1;
				moduleSequence [i] [0] = "Easy";
			} else if (i < levelFormat [0] + levelFormat [1]) {
				rangeCap = mediumModulesCount + 1;
				moduleSequence [i] [0] = "Medium";
			} else {
				rangeCap = hardModuleCount + 1;
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
		moduleData = new ModuleData[moduleSequence.Length + 1];

		GetModuleData (0, "Modules/Start/" + tier);

		int n = moduleData.Length;
		for (int i = 1; i < n; i++) {
			GetModuleData (i, "Modules/" + moduleSequence [i - 1] [0] + "/" + moduleSequence [i - 1] [1]);
		}
	}

	void GetModuleData (int i, string path)
	{
		List<Vector4> blockData = new List<Vector4> ();
		List<Vector4> spikeData = new List<Vector4> ();
		Vector2[] enemySpawnPosition = null;
		int enemySpawnCounter = -1;

		TextAsset textFile = (TextAsset)Resources.Load (path, typeof(TextAsset));
		if (textFile == null) {
			Debug.Log ("Error: " + path + ".txt no such file");
		}

		System.IO.StringReader textStream = new System.IO.StringReader (textFile.text);
		string line;
		while ((line = textStream.ReadLine ()) != null) {
			if (!string.IsNullOrEmpty (line)) {
				string[] rawData = new string[4];
				rawData = line.Split (',');
				if (rawData [0].Equals ("blk")) {
					blockData.Add (new Vector4 (float.Parse (rawData [1]), float.Parse (rawData [2]), float.Parse (rawData [3]), float.Parse (rawData [4])));
				} else
					if (rawData [0].Equals ("spk")) {
					spikeData.Add (new Vector4 (float.Parse (rawData [1]), float.Parse (rawData [2]), float.Parse (rawData [3]), float.Parse (rawData [4])));
				} else if (rawData [0].Equals ("stt")) {
					moduleData [i].startPointHeight = (int)float.Parse (rawData [1]);
				} else if (rawData [0].Equals ("end")) {
					moduleData [i].endPointHeight = (int)float.Parse (rawData [1]);
				} else if (rawData [0].Equals ("pla")) {
					playerPosition = new Vector2 (float.Parse (rawData [1]) + 0.5f, float.Parse (rawData [2]) + 0.5f);
				} else if (rawData [0].Equals ("spc")) {
					enemySpawnCounter = int.Parse (rawData [1]);
					enemySpawnPosition = new Vector2[enemySpawnCounter];
				} else if (rawData [0].Equals ("enm") && enemySpawnPosition != null) {
					enemySpawnPosition [--enemySpawnCounter] = new Vector2 (float.Parse (rawData [1]), float.Parse (rawData [2]));
				} else {
					Debug.Log ("Error loading module: invalid formating.");
				}
			}
		}
		if (enemySpawnPosition != null && enemySpawnPosition.Length != 0) {
			Shuffle (enemySpawnPosition);
			//int n = Mathf.RoundToInt (enemySpawnPosition.Length / 2 + Random.value);
			int n = enemySpawnPosition.Length;
			moduleData [i].enemies = new Vector2[n];
			System.Array.Copy (enemySpawnPosition, moduleData [i].enemies, n);
			moduleData [i].enemyToUse = enemyToUse;
		}
		moduleData [i].blocks = blockData.ToArray ();
		moduleData [i].spikes = spikeData.ToArray ();
		moduleData [i].blockToUse = blockToUse;
		moduleData [i].spikeToUse = spikeToUse;
		moduleData [i].blockTexture = (Material)Resources.Load ("Textures/" + (i == 0 ? tier : moduleSequence [i - 1] [0]) + "/" + textureCode, typeof(Material));
		if (moduleData [i].blockTexture == null) {
			Debug.Log ("no texture found at: Textures/" + (i == 0 ? tier : moduleSequence [i - 1] [0]) + "/" + textureCode);
		}
		moduleData [i].name = path;
		moduleData [i].spawnEnemies = spawnEnemies;
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
		levelBounds.size = new Vector2 (numberOfModules * 36.0f, 60.0f + Mathf.Abs (yOffset));
		levelBounds.offset = new Vector2 (levelBounds.size.x / 2.0f, -(levelBounds.size.y / 2.0f) + 40.0f);
	}

	void PlacePlayer ()
	{
		GameObject player = GameObject.FindGameObjectsWithTag ("Player") [0];
		player.transform.position = playerPosition;
		player.GetComponent<Player> ().startingPosition = playerPosition;
	}

	//DEBUG
	void GenerateDebugModuleSequence ()
	{
		moduleSequence = new string[easyModuleCount + mediumModulesCount + hardModuleCount][];
		int n = 0;
		for (int i = 0; i < easyModuleCount; i++) {
			moduleSequence [i] = new string[2]{"Easy", (++n).ToString ()};
		}
		n = 0;
		for (int i = easyModuleCount; i < easyModuleCount+mediumModulesCount; i++) {
			moduleSequence [i] = new string[2]{"Medium", (++n).ToString ()};
		}
		n = 0;
		for (int i = easyModuleCount+mediumModulesCount; i < easyModuleCount+mediumModulesCount+hardModuleCount; i++) {
			moduleSequence [i] = new string[2]{"Hard", (++n).ToString ()};
		}
		Debug.Log (easyModuleCount + mediumModulesCount + hardModuleCount + " modules build");
	}

	void GenerateDebugModuleSequence (string sequence)
	{
		string[] tempArr = sequence.Split (',');
		moduleSequence = new string[tempArr.Length][];
		for (int i = 0; i < tempArr.Length; i++) {
			string[] temp = tempArr [i].Split ('|');
			moduleSequence [i] = new string[2]{temp [0], temp [1]};
		}
	}
}

public struct ModuleData
{
	public Vector4[] blocks; // this is an example of a  feature vector ???
	public Vector4[] spikes;
	public Vector2[] enemies;
	public int startPointHeight;
	public int endPointHeight;
	public GameObject blockToUse;
	public Material blockTexture;
	public GameObject spikeToUse;
	public GameObject enemyToUse;
	//DEBUG
	public string name;
	public bool spawnEnemies;
}
