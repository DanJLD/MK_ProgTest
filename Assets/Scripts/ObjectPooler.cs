using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
	public enum ObjectPoolDistType
	{
		RANDOM, // pick randomly from set of prefabs
		SPREAD  // add an even amount of each prefab from the set, favouring those early in the list if an uneven amount is made.
	}


	[System.Serializable]
	public class Pool
	{
		public string tag;
		public List<TerrainPiece> prefabs;
		public int size;
		public ObjectPoolDistType distType;
	}
	public List<Pool> pools;
	public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
		int id;
		GameObject go;
		poolDictionary = new Dictionary<string, Queue<GameObject>>(); // create an empty dictionary of object queues (pools)

		foreach (Pool pool in pools) // for each pool...
		{
			Queue<GameObject> objectPool = new Queue<GameObject>();

			// populate the pool based on the distribution type
			for (int i = 0; i < pool.size; i++)
			{
				if (pool.distType == ObjectPoolDistType.RANDOM) // random selection
				{
					id = Random.Range(0, pool.prefabs.Count);
				}
				else // if (pool.distType == ObjectPoolDistType.SPREAD) // even distribution of pieces
				{
					id = i % pool.prefabs.Count; // counts from 0 to Count-1 repeatedly as i increments.
				}
				go = Instantiate(pool.prefabs[id].spawnPrefab);
				go.SetActive(false);
				objectPool.Enqueue(go);
			}

			// randomise queue

			poolDictionary.Add(pool.tag, objectPool);
		}
    }
}
