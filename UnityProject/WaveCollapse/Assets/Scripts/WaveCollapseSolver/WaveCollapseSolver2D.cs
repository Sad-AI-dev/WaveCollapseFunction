using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCollapseSolver2D : MonoBehaviour
{
    public WFCDataSet2D dataSet;

    [Header("Generation Settings")]
    [SerializeField] private Transform tileHolder;
    [SerializeField] private Vector2Int gridSize;

    [Header("Preview Settigns")]
    public Transform previewHolder;
    public Vector2 previewSpacing;

    //vars
    public GameObject[] lookupTable { get; private set; }

    private void Start()
    {
        GenerateLookupTable();
    }

    //=========== Initialize Vars =================
    public void GenerateLookupTable()
    {
        lookupTable = new GameObject[dataSet.tiles.Length];
        for (int i = 0; i < dataSet.tiles.Length; i++) {
            lookupTable[i] = dataSet.tiles[i].prefab;
        }
    }
}
