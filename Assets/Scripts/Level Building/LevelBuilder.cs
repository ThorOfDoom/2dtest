using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LevelBuilder : MonoBehaviour
{
	
	public int numberOfEasyModules;
	public int totalEasyModules;/*
	public int numberOfMediumModules;
	public int totalMdeiumModules;
	public int numberOfHardModules;
	public int totalHardModules;*/
	public GameObject moduleBuilder;

	BoxCollider2D levelBounds;
	int numberOfModules;
	int yOffset;


	void Start ()
	{
		levelBounds = GetComponentInChildren<BoxCollider2D> ();
		numberOfModules = 0;
		yOffset = 0;

		for (int i = 0; i < numberOfEasyModules; i++) {
			GameObject mbObj = (GameObject)Instantiate (moduleBuilder, new Vector3 (0, 0, 0), Quaternion.identity);
			ModuleBuilder mb = mbObj.GetComponent<ModuleBuilder> ();
			mb.transform.parent = transform;
			mb.moduleName = Random.Range (1, totalEasyModules).ToString ();
			//mb.moduleName = (i + 1).ToString ();
			yOffset = mb.BuildModule (numberOfModules++, yOffset);

		}
	}


	List<Vector4[]> LoadLevelData (string rawModuleData)
	{
		System.IO.StringReader textStream = new System.IO.StringReader (rawModuleData);
		string line;
		List<Vector4[]> moduleData = new List<Vector4[]> ();
		
		List<Vector4> blockData = new List<Vector4> ();
		List<Vector4> spikeData = new List<Vector4> ();
		Vector4[] offset = new Vector4[1];
        
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
					offset [0].x = float.Parse (rawData [1]);
				} else if (rawData [0].Equals ("end")) {
					offset [0].y = float.Parse (rawData [1]);
				} else {
					Debug.Log ("Error loading module: invalid formating.");
				}
			}
		}

		moduleData.Add (blockData.ToArray ());
		moduleData.Add (spikeData.ToArray ());
		moduleData.Add (offset);

		return moduleData;
	}
}
