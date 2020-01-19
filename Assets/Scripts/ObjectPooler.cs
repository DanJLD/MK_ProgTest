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

	#region Singleton
	public static ObjectPooler Instance;
	//private void Awake()
	//{
	//	Instance = this;
	//}
	#endregion

	[System.Serializable]
	public class Pool
	{
		public string tag;
		public List<TerrainPiece> prefabs;
		public int size;
		public ObjectPoolDistType distType;
	}
	public List<Pool> pools;
	public Dictionary<string, List<GameObject>> poolDictionary;

    void Awake()
    {
		Instance = this; // singleton setup

		int id;
		GameObject go;
		poolDictionary = new Dictionary<string, List<GameObject>>(); // create an empty dictionary of object queues (pools)

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
		}
    }

	public GameObject SpawnFromPool (string tag, Vector3 pos, Quaternion rot)
	{
		if (!poolDictionary.ContainsKey(tag))
		{
			print("Error: Pool does not contain tag " + tag);
			return null;
		}

		GameObject go;

		if (poolDictionary[tag].Count > 0)
		{
			go = poolDictionary[tag][0];
			go.SetActive(true);
			go.transform.position = pos;
			go.transform.rotation = rot;
			poolDictionary[tag].RemoveAt(0); // dequeue
			poolDictionary[tag].Add(go); // add to end of queue
		}
		else // pool is empty
		{
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
			rng = Random.Range(0, i); // select a random position from 1 to i
			list.Add(list[rng]); 
			list.RemoveAt(rng); // take it out, and place it at the end
			// repeat until every element has been moved once.
		}
		return list;
	}
}
