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

	// Start is called before the first frame update
	void Start()
    {
		// initialise a set of ground under the player
		float i = destroyPoint;
		while (i <= spawnPoint)
		{
			// spawn a random ground piece
			int rng = Random.Range(0, groundBank.Count);
			GameObject go = Instantiate(groundBank[rng].spawnPrefab, groundOffset, Quaternion.identity); 
			go.transform.position += new Vector3(i, 0f, 0f); // move it into position
			i += groundBank[rng].xUnitsOccupied; // move spawn position forward
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
