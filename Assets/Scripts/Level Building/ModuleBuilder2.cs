using UnityEngine;
using System.Collections;

public class ModuleBuilder2 : MonoBehaviour
{
	public ModuleData moduleData;
	int _yOffset;
	GameObject blockToUse;
	GameObject spikeToUse;

	public int Build (int numberOfModules, int yOffset)
	{
		InitBuildingBlocks ();
		PlaceBlocks ();
		if (moduleData.spikes.Length != 0) {
			PlaceSpikes ();
		}
		
		_yOffset = yOffset - moduleData.startPointHeight;
		transform.position += new Vector3 (numberOfModules * 36, _yOffset, 0.0f);
		
		CleanUpBuildingBlocks ();


		return _yOffset + moduleData.endPointHeight;
	}

	void InitBuildingBlocks ()
	{
		blockToUse = (GameObject)Instantiate (moduleData.blockToUse);
		MeshRenderer meshRenderer = blockToUse.GetComponentInChildren<MeshRenderer> ();
		meshRenderer.material = moduleData.blockTexture;

		spikeToUse = (GameObject)Instantiate (moduleData.spikeToUse);
	}

	void CleanUpBuildingBlocks ()
	{
		Destroy (blockToUse);
		Destroy (spikeToUse);
	}

	void PlaceBlocks ()
	{
		foreach (Vector4 data in moduleData.blocks) {
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
		foreach (Vector4 data in moduleData.spikes) {
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
}
