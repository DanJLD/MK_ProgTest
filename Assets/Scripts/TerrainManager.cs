using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
	public List<TerrainPiece> groundBank;
	public List<TerrainPiece> platformBank;

	public List<GameObject> spawnedObjects;

	public float scrollSpeed = 1f;
	public float scrollIncreaseAmount = 0.05f;
	public float scrollIncreaseTickRate = 5f;

	public Vector2 groundOffset;

	public float destroyPoint = -50f;
	public float spawnPoint = 50f;

	public Vector3 nextSpawnLoc;

	// Start is called before the first frame update
	void Start()
    {
		// initialise a set of ground under the player
		float i = destroyPoint;
		while (i <= spawnPoint)
		{
			// spawn a random ground piece
			int rng = Random.Range(0, groundBank.Count);
			GameObject go = Instantiate(groundBank[rng].spawnPrefab, new Vector3(i, 0f, 0f), Quaternion.identity); 
			spawnedObjects.Add(go);
			i += groundBank[rng].xUnitsOccupied; // move spawn position forward
		}
		nextSpawnLoc = new Vector3(i, 0f, 0f);
    }

	// Update is called once per frame
	void Update()
	{
		// move all terrain objects down the field
		// avoiding the use of a endlessly moving metaObject to avoid infinitely increasing position values, and eventual floating point shenanigans
		Vector3 deltaPos = new Vector3(scrollSpeed * Time.deltaTime, 0f, 0f);
		for (int i = spawnedObjects.Count-1; i >= 0; i--) // backwards iteration so item deletion doesnt break things
		{
			spawnedObjects[i].transform.position -= deltaPos;
			if (spawnedObjects[i].transform.position.x <= destroyPoint)
			{
				Destroy(spawnedObjects[i]);
				spawnedObjects.RemoveAt(i);
			}
		}
		nextSpawnLoc -= deltaPos; // also move spawn position

		//spawn more ground if needed
		if (nextSpawnLoc.x <= spawnPoint)
		{
			SpawnGroundTile();
		}
    }

	void SpawnGroundTile()
	{
		// spawn a random ground piece
		int rng = Random.Range(0, groundBank.Count);
		GameObject go = Instantiate(groundBank[rng].spawnPrefab, new Vector3(nextSpawnLoc.x, 0f, 0f), Quaternion.identity);
		spawnedObjects.Add(go);
		nextSpawnLoc += new Vector3(groundBank[rng].xUnitsOccupied, 0f, 0f);
	}
}
