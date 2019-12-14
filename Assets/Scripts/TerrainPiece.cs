using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainPiece", menuName = "TerrainPiece", order = 1)]
public class TerrainPiece : ScriptableObject
{
	public GameObject spawnPrefab;
	public float xUnitsOccupied = 0f; // horizontal units this piece occupies
	public float minPrevYHeight = 0f; // minimum height of previous spawned piece required to access this piece
}
