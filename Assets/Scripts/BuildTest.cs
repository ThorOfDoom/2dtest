using UnityEngine;
using System.Collections;

public class BuildTest : MonoBehaviour {

	public Transform obstacle;
	private int numberOfModules = 0;
	private int yOffSet;
	int startPointHeight;
	int endPointHeight;
	int startEndDifference;

	// Use this for initialization

	void Start() 
	{

	}

	void Easy01()
	{
		startPointHeight = 5;

		for (int y = 0; y <= 19; y++) 
		{
			for (int x = 0; x <= 35; x++) 
			{
				//roof
				if (y >= 16)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);
				}

				//floor
				if (y <= 4)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);		
				}

				//left collum
				if (x >= 6 && x <= 10 && y >= 5 && y <= 7)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);
				}
				//middle collum
				if (x >= 15 && x <= 20 && y >= 5 && y <= 9 )
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);
				}
				//right collum
				if (x >= 25 && x <= 30 && y >= 5 && y <= 7 )
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36) , y, 0), Quaternion.identity);
				}
			}
		}
		numberOfModules += 1;
		endPointHeight = 5;
		startEndDifference = 0;
	}

	void Easy02()
	{
		startPointHeight = 15;
		endPointHeight = 3;
		
		for (int y = 0; y <= 19; y++) 
		{
			for (int x = 0; x <= 35; x++) 
			{
				//roof
				if (y >= 18)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);
				}
				
				//floor
				if (y <= 1)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);		
				}
				
				//left collum
				if (x <= 4 && y >= 2 && y >= 14)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);
				}
				//middle left collum
				if (x >= 8 && x <= 15 && y >= 5 && y <= 17)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36), y, 0), Quaternion.identity);
				}
				//middle right collum
				if (x >= 20 && x <= 25 && y >= 2 && y <= 13)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36) , y, 0), Quaternion.identity);
				}
				//middle right collum
				if (x >= 31 && y >= 5 && y <= 17)
				{
					Instantiate(obstacle, new Vector3(x +(numberOfModules * 36) , y, 0), Quaternion.identity);
				}
			}
		}
		numberOfModules += 1;
	}
}
