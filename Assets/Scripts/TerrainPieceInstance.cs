using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPieceInstance : MonoBehaviour
{
	[System.Serializable]
	public struct TerrainInfo
	{
		public float xUnitsOccupied;
		public float minPrevYHeight;
		// add more as needed
	}
	public TerrainInfo terrainInfo;
}
