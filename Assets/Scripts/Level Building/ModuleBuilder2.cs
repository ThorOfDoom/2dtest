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
		if (moduleData.enemies != null && moduleData.enemies.Length != 0) {
			SpawnEnemies ();
		}
		
		_yOffset = yOffset - moduleData.startPointHeight;
		transform.position += new Vector3 (numberOfModules * 36, _yOffset, 0.0f);
		
		CleanUpBuildingBlocks ();


		return _yOffset + moduleData.endPointHeight;
	}

	void InitBuildingBlocks ()
	{
		blockToUse = (GameObject)Instantiate (moduleData.blockToUse);
		blockToUse.name = "block";
		MeshRenderer meshRenderer = blockToUse.GetComponentInChildren<MeshRenderer> ();
		meshRenderer.material = moduleData.blockTexture;

		spikeToUse = (GameObject)Instantiate (moduleData.spikeToUse);
		spikeToUse.name = "spike";
	}

	void CleanUpBuildingBlocks ()
	{
		Destroy (blockToUse);
		Destroy (spikeToUse);
	}

	void PlaceBlocks ()
	{
		GameObject blockContainer = new GameObject ("Blocks");
		blockContainer.transform.parent = transform;
		for (int i = 0; i < moduleData.blocks.Length; i++) {
			InstantiateBlock (moduleData.blocks [i].x, moduleData.blocks [i].y, 
			                  moduleData.blocks [i].z, moduleData.blocks [i].w,
			                  blockContainer.transform);
		}
	}
	
	void InstantiateBlock (float x, float y, float width, float height, Transform container)
	{
		GameObject block = 
			(GameObject)Instantiate (blockToUse, 
			                         new Vector3 (x, y, 0), 
			                         Quaternion.identity);
		
		block.GetComponent<BackgroundTiling> ().UpdateBlock (width, height);
		block.transform.parent = container;
	}
	
	void PlaceSpikes ()
	{
		GameObject spikeContainer = new GameObject ("Spikes");
		spikeContainer.transform.parent = transform;
		for (int i = 0; i < moduleData.spikes.Length; i++) {
			InstantiateSpike (moduleData.spikes [i].x, moduleData.spikes [i].y, 
			                  moduleData.spikes [i].z, moduleData.spikes [i].w,
			                  spikeContainer.transform);
		}
	}
	
	void InstantiateSpike (float x, float y, float width, float rawRotation, Transform container)
	{
		GameObject spike = 
			(GameObject)Instantiate (spikeToUse, 
			                         new Vector3 (x, y, 0), 
                                     Quaternion.identity);
		spike.GetComponent<SpikeTiling> ().UpdateSpike (width, (int)Mathf.Round (rawRotation));
		spike.transform.parent = container;
	}

	void SpawnEnemies ()
	{
		GameObject enemyContainer = new GameObject ("Enemies");
		enemyContainer.transform.parent = transform;
		for (int i = 0; i < moduleData.enemies.Length; i++) {
			// TODO adjust the spawn adjust for different sized enemies
			InstantiateEnemy (moduleData.enemies [i].x + 0.5f, moduleData.enemies [i].y + 0.5f, enemyContainer.transform);
		}
	}

	void InstantiateEnemy (float x, float y, Transform container)
	{
		GameObject enemy = (GameObject)Instantiate (moduleData.enemyToUse, new Vector3 (x, y, 0), Quaternion.identity);
		enemy.transform.parent = container;
	}
}
