/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModuleBuilder : MonoBehaviour
{

	public static void Build (List<Vector4[]> moduelData, GameObject[] buildingBlocks)
	{
		Vector3 offset = new Vector3 (moduelData [2] [0].x, moduelData [2] [0].y, 0.0f);
		ModuleBuilder.PlaceBlocks (moduelData [0], buildingBlocks [0]);
		if (moduelData [1].Length != 0) {
			ModuleBuilder.PlaceSpikes (moduelData [1], buildingBlocks [1]);
		}
	}

	void PlaceBlocks (Vector4[] blocks, GameObject blockToUse)
	{
		foreach (Vector4 data in blocks) {
			InstantiateBlock (data.x, data.y, data.z, data.w, blockToUse);
		}
	}
	
	void InstantiateBlock (float x, float y, float width, float height, GameObject blockToUse)
	{
		GameObject block = 
			(GameObject)Instantiate (blockToUse, 
			                         new Vector3 (x, y, 0), 
			                         Quaternion.identity);
		block.GetComponent<BackgroundTiling> ().UpdateBlock (width, height);
		block.transform.parent = transform;
	}
	
	void PlaceSpikes (Vector4[] spikes, GameObject spikeToUse)
	{
		foreach (Vector4 data in spikes) {
			InstantiateSpike (data.x, data.y, data.z, data.w, spikeToUse);
		}
	}
	
	void InstantiateSpike (float x, float y, float width, float rawRotation, GameObject spikeToUse)
	{
		GameObject spike = 
			(GameObject)Instantiate (spikeToUse, 
			                         new Vector3 (x, y, 0), 
			                         Quaternion.identity);
		spike.GetComponent<SpikeTiling> ().UpdateSpike (width, (int)Mathf.Round (rawRotation));
		spike.transform.parent = transform;
	}
}*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModuleBuilder : MonoBehaviour
{

	public string moduleName;
	public GameObject blockToUse;
	public GameObject spikeToUse;
	public int _yOffset;
	List<Vector4> blockData = new List<Vector4> ();
	List<Vector4> spikeData = new List<Vector4> ();
	int startPointHeight;
	int endPointHeight;

	public int BuildModule (int numberOfModules, int yOffset)
	{
		LoadLevelData ();
		PlaceBlocks ();
		if (spikeData.Count != 0) {
			PlaceSpikes ();
		}

		_yOffset = yOffset - startPointHeight;
		transform.position += new Vector3 (numberOfModules * 36, _yOffset, 0.0f);

		return _yOffset + endPointHeight;
	}
	
	void PlaceBlocks ()
	{
		foreach (Vector4 data in blockData) {
			InstantiateBlock (data.x, data.y, data.z, data.w);
		}
	}
	
	void InstantiateBlock (float x, float y, float width, float height)
	{
		GameObject block = 
			(GameObject)Instantiate (blockToUse, 
			                         new Vector3 (x, y, 0), 
			                         Quaternion.identity);
		block.GetComponent<BackgroundTiling> ().UpdateBlock (width, height);
		block.transform.parent = transform;
	}

	void PlaceSpikes ()
	{
		foreach (Vector4 data in spikeData) {
			InstantiateSpike (data.x, data.y, data.z, data.w);
		}
	}
	
	void InstantiateSpike (float x, float y, float width, float rawRotation)
	{
		GameObject spike = 
			(GameObject)Instantiate (spikeToUse, 
			                         new Vector3 (x, y, 0), 
			                         Quaternion.identity);
		spike.GetComponent<SpikeTiling> ().UpdateSpike (width, (int)Mathf.Round (rawRotation));
		spike.transform.parent = transform;
	}

	void LoadLevelData ()
	{
		TextAsset textFile = (TextAsset)Resources.Load ("Modules/Easy/" + moduleName, typeof(TextAsset));
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
					startPointHeight = (int)float.Parse (rawData [1]);
				} else if (rawData [0].Equals ("end")) {
					endPointHeight = (int)float.Parse (rawData [1]);
				} else {
					Debug.Log ("Error loading module: invalid formating.");
				}
			}
		}
	}
}