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

	public static ObjectPooler Instance;

	[System.Serializable]
	public class Pool
	{
		public string tag;
		public List<TerrainPiece> prefabs;
		public int size;
		public ObjectPoolDistType distType;
	}
	public List<Pool> pools;
	public Dictionary<string, List<GameObject>> poolDictionary; // dictionary of object pools
	private Dictionary<string, int> poolRngCounters; // accompanying dictionary of counters for use in logical operation on the above poolDictionary

	void Awake()
    {
		Instance = this; // singleton setup

		int id;
		GameObject go;
		poolDictionary = new Dictionary<string, List<GameObject>>(); // create an empty dictionary of object queues (pools)
		poolRngCounters = new Dictionary<string, int>();

		foreach (Pool pool in pools) // for each pool...
		{
			List<GameObject> objectPool = new List<GameObject>();

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

				// apply properties to gameObject
				go.GetComponent<TerrainPieceInstance>().terrainInfo.xUnitsOccupied = pool.prefabs[id].xUnitsOccupied;
				// add more here as needed

				objectPool.Add(go);
			}

			// randomise queue
			objectPool = RandomiseList(objectPool);
			poolDictionary.Add(pool.tag, objectPool);

			poolRngCounters.Add(pool.tag, 0); // initialise counter for each pool
		}
    }

	public GameObject SpawnFromPool (string tag, Vector3 pos, Quaternion rot)
	{
		if (!poolDictionary.ContainsKey(tag)) // check that tag exists
		{
			print("Error: Pool does not contain tag " + tag);
			return null;
		}

		GameObject go = null;

		if (poolDictionary[tag].Count > 0)
		{
			int i = 0;
			bool targetAcquired = false;
			while (!targetAcquired) // search for inactive tile, starting from the front of the list
			{
				if (poolDictionary[tag][i].gameObject.activeSelf == false)
				{
					go = poolDictionary[tag][i];
					targetAcquired = true;
				}
				else
				{
					i++;
				}
				if (i >= poolDictionary[tag].Count) // if entire list is active, more tiles are needed
				{
					print("Warning: Pool exhausted. Creating a new pool tile." + tag);
					// create new object for pool
					targetAcquired = true;
					return null; // temp
				}
			}

			// initialise tile
			//go = poolDictionary[tag][0];
			go.SetActive(true);
			go.transform.position = pos;
			go.transform.rotation = rot;
			poolDictionary[tag].RemoveAt(i); // dequeue
			poolDictionary[tag].Add(go); // add to end of queue

			// Occasionally randomise list to ensure no repeat patterns
			// > count number of spawns. If all have spawned, randomise list again
			poolRngCounters[tag]++;
			if (poolRngCounters[tag] >= poolDictionary[tag].Count)
			{
				poolDictionary[tag] = RandomiseList(poolDictionary[tag]);
				//poolRngCounters[tag] = 0;
			}
			
		}
		else // pool is empty
		{
			print("Warning: Pool is empty. Creating a new pool tile." + tag);
			// create more objects for object pool
			go = null; // temperary
		}
		return go;
	}

	List<GameObject> RandomiseList(List<GameObject> list)
	{
		int rng;
		
		for (int i = list.Count - 1; i >= 0; i--) // backwards iteration so item deletion doesnt break things
		{
			rng = Random.Range(0, i); // select a random element from 0 to i
			list.Add(list[rng]);
			list.RemoveAt(rng); // take it out, and place it at the end
			// repeat until every element has been moved once.
		}
		return list;
	}
}
