using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node3D : MonoBehaviour
{
    public Vector3Int posInGrid;
    
    //uncollapsed data
    [SerializeField] private WeightedChance<int> possibilities;
    public int Entropy { get { return possibilities.Count; } }
    
    //collapsed info
    public bool isCollapsed;
    public int id;

    //vars
    private WaveCollapseSolver3D solver;
    private GameObject createdTile;

    //======================================
    public void Initialize(WaveCollapseSolver3D solver)
    {
        this.solver = solver;
        possibilities = new WeightedChance<int>();
        ResetTile();
    }

    //============= Collapse Tile =================
    public void Collapse()
    {
        CreateNode(possibilities.GetRandom());
        possibilities.Clear();
    }

    public void CreateNode(int tileToCreate)
    {
        if (tileToCreate == -1) { return; }
        id = tileToCreate;
        createdTile = Instantiate(solver.LookupTable[tileToCreate], transform);
        isCollapsed = true;
    }

    //============= Perpetuate ==================
    public void Perpetuate(WFCNodeData3D placedTile, Direction dir) //dir is the direction this tile is compared to the placed tile
    {
        List<int> possibleConnections = placedTile.ConnectionsFromDirection(dir);
        List<int> connections = new List<int>(possibilities.Keys());

        for (int i = 0; i < connections.Count; i++) {
            if (!possibleConnections.Contains(connections[i])) {
                //connection is no longer possible
                possibilities.Remove(connections[i]);
            }
        }
    }

    //============= Reset ====================
    public void ResetTile()
    {
        //remove chosen tile
        if (createdTile) {
            Destroy(createdTile);
            id = -1;
        }
        //restore options
        ResetPossibilities();
        //uncollapse
        isCollapsed = false;
    }

    private void ResetPossibilities()
    {
        possibilities.Clear(); //clear out

        //fill data
        for (int i = 0; i < solver.dataSet.nodes.Length; i++) {
            possibilities.Add(i, solver.dataSet.nodes[i].weight);
        }
    }
}
