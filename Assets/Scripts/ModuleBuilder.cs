using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModuleBuilder : MonoBehaviour
{

	public Vector2 offset;
	public string moduleName;
	public GameObject blockToUse;

	List<Vector4> moduleData = new List<Vector4> ();

	// Use this for initialization
	void Start ()
	{
		LoadLevelData ();
		BuildModule (moduleData);
	}
	
	void BuildModule (List<Vector4> boxData)
	{
		foreach (Vector4 data in boxData) {
			InstantiateBlock (data.x, data.y, data.z, data.w);
		}
	}

	void InstantiateBlock (float x, float y, float width, float height)
	{
		GameObject block = 
			(GameObject)Instantiate (blockToUse, 
			                         new Vector3 (x + offset.x, y + offset.y, 0), 
			                         Quaternion.identity);
		block.GetComponent<BackgroundTiling> ().UpdateBlock (width, height);
		block.transform.parent = transform;
	}

	void LoadLevelData ()
	{
		TextAsset textFile = (TextAsset)Resources.Load ("Modules/" + moduleName, typeof(TextAsset));
		System.IO.StringReader textStream = new System.IO.StringReader (textFile.text);
		string line;
		
		while ((line = textStream.ReadLine()) != null) {
			if (!string.IsNullOrEmpty (line)) {
				string[] rawData = new string[4];
				rawData = line.Split (',');
				if (rawData.Length == 4) {
					moduleData.Add (new Vector4 (float.Parse (rawData [0]), float.Parse (rawData [1]), 
					                             float.Parse (rawData [2]), float.Parse (rawData [3])));
				} else {
					Debug.Log ("Error loading module: invalid formating.");
				}
			}
		}
	}
}
