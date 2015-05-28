using UnityEngine;
using System.Collections;
using System;

public class BuildTest : MonoBehaviour
{

	public GameObject obstacle;
	public GameObject previouse;
	public GameObject current;
	public GameObject next;
	public GameObject previouseContainer;
	public GameObject currentContainer;
	public GameObject nextContainer;
	public GameObject levelBounds;
	public int moduleTypes;
	BoxCollider2D boundingBox;
	private int numberOfModules = 0;
	int startPointHeight;
	int endPointHeight;
	int startEndDifference;
	float yOffset;
	int moduleHeight = 20;

	// Use this for initialization

	void Start ()
	{
		boundingBox = levelBounds.GetComponent<BoxCollider2D> ();
		startPointHeight = 0;
		endPointHeight = 0;
		System.Random r = new System.Random ();



		StartModule (currentContainer);

		for (int i = 0; i < 10; i++) {
			int rand = (int)(r.Next (1, moduleTypes + 1));
			switch (rand) {
			case 1:
				Easy01 (currentContainer);
				break;
			case 2:
				Easy02 (currentContainer);
				break;
			case 3:
				Easy03 (currentContainer);
				break;
			case 4:
				Easy04 (currentContainer);
				break;
			case 5:
				Easy05 (currentContainer);
				break;
			default:
				break;
			}
		}

		//Easy02 (currentContainer);
		Debug.Log (numberOfModules);
		boundingBox.size = new Vector2 (numberOfModules * 36.0f, 20.0f + Mathf.Abs (yOffset));
		boundingBox.offset = new Vector2 ((numberOfModules * 36.0f) / 2.0f, (yOffset + 20) / 2.0f);
		/*
		Easy01 (previouseContainer);
		Easy01 (currentContainer);
		//MoveObstacles (current, 36.0f);
		Easy01 (nextContainer);

		//MoveObstacles (next, 72.0f);
		Invoke ("UpdateWorld", 2);*/

	}
	/*
	void UpdateWorld ()
	{
		Transform ContainerTransform = previouseContainer.transform;
		foreach (Transform child in ContainerTransform) {
			Destroy (child.gameObject);
		}
		ContainerTransform = currentContainer.transform;
		foreach (Transform child in ContainerTransform) {
			child.gameObject.transform.parent = previouseContainer.transform;
			Destroy (child.gameObject);
		}
		
	}*/


	void StartModule (GameObject container)
	{
		for (int y = -(moduleHeight); y < 2*moduleHeight; y++) {
			for (int x = 0; x < 36; x++) {
				if ((x <= 17) ||
					(x >= 18 && y >= 14) ||
					(x >= 18 && y <= 2)) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
			}
		}
		numberOfModules += 1;
		endPointHeight = 3;
	}

	void Easy01 (GameObject container)
	{
		startPointHeight = 5;
		yOffset += endPointHeight - startPointHeight;



		for (int y = -(moduleHeight); y < 2*moduleHeight; y++) {
			for (int x = 0; x < 36; x++) {
				//roof
				if (y >= 16) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}

				//floor
				if (y <= 4) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;		
				}

				//left collum
				if (x >= 6 && x <= 10 && y >= 5 && y <= 7) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//middle collum
				if (x >= 15 && x <= 20 && y >= 5 && y <= 9) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//right collum
				if (x >= 25 && x <= 30 && y >= 5 && y <= 7) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
			}
		}
		numberOfModules += 1;
		endPointHeight = 5;
	}

	void Easy02 (GameObject container)
	{
		startPointHeight = 15;
		yOffset += endPointHeight - startPointHeight;
		
		for (int y = -moduleHeight; y < 2*moduleHeight; y++) {
			for (int x = 0; x < 36; x++) {
				//roof
				if (y >= 18) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				
				//floor
				if (y <= 1) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;		
				}
				
				//left collum
				if (x <= 4 && y >= 2 && y <= 14) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//middle left collum
				if (x >= 8 && x <= 15 && y >= 5 && y <= 17) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//middle right collum
				if (x >= 20 && x <= 27 && y >= 2 && y <= 13) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//middle right collum
				if (x >= 31 && y >= 5 && y <= 17) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
			}
		}
		numberOfModules += 1;
		endPointHeight = 2;
	}
	
	void Easy03 (GameObject container)
	{
		startPointHeight = 13;
		yOffset += endPointHeight - startPointHeight;
		
		for (int y = -moduleHeight; y < 2*moduleHeight; y++) {
			for (int x = 0; x < 36; x++) {
				//roof
				if (y >= 17) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				
				//floor
				if (y <= 0) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;		
				}
				
				//left collum
				if (x <= 6 && y >= 1 && y <= 12) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//middle  collum
				if (x >= 14 && x <= 22 && y >= 1 && y <= 7) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//right collum
				if (x >= 30 && y >= 1 && y <= 2) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//small weird block
				if (x >= 30 && y >= 7 && y <= 11) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				//big weird block
				if (x >= 14 && y >= 12 && y <= 16) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
			}
		}
		numberOfModules += 1;
		endPointHeight = 3;
	}
	
	void Easy04 (GameObject container)
	{
		startPointHeight = 5;
		yOffset += endPointHeight - startPointHeight;
		
		for (int y = -moduleHeight; y < 2*moduleHeight; y++) {
			for (int x = 0; x < 36; x++) {
				//roof
				if (y >= 17) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				
				//floor
				if (y <= 4) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;		
				}
				
				//lower block
				if (x >= 8 && x <= 28 && y >= 5 && y <= 6) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				} else if (x >= 10 && x <= 26 && y >= 7 && y <= 8) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				} else if (x >= 14 && x <= 22 && y >= 9 && y <= 10) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				} else if (x >= 16 && x <= 20 && y >= 11 && y <= 12) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}

			}
		}
		numberOfModules += 1;
		endPointHeight = 5;
	}
	
	void Easy05 (GameObject container)
	{
		startPointHeight = 15;
		yOffset += endPointHeight - startPointHeight;
		
		for (int y = -moduleHeight; y < 2*moduleHeight; y++) {
			for (int x = 0; x < 36; x++) {
				//roof
				if (y >= 18) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				
				//floor
				if (y <= 1) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;		
				}
				
				//block
				if (x <= 29 && y >= 12 && y <= 14) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				} 
				if (x >= 6 && y >= 5 && y <= 7) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				} 
				if (x >= 33 && y >= 8 && y <= 17) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				} 
				
				if (x <= 2 && y >= 2 && y <= 11) {
					GameObject block = (GameObject)Instantiate (obstacle, new Vector3 (x + (numberOfModules * 36), y + yOffset, 0), Quaternion.identity);
					block.transform.parent = container.transform;
				}
				
			}
		}
		numberOfModules += 1;
		endPointHeight = 2;
	}
}
