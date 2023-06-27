using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile2D : MonoBehaviour
{
    public Vector2Int posInGrid;
    
    //uncollapsed data
    [SerializeField] private WeightedChance<int> possibilities;
    public int Entropy { get { return possibilities.Count; } }
    
    //collapsed info
    public bool isCollapsed;
    public int id;

    //vars
    private WaveCollapseSolver2D solver;
    private GameObject createdTile;

    //======================================
    public void Initialize(WaveCollapseSolver2D solver)
    {
        this.solver = solver;
        possibilities = new WeightedChance<int>();
        isCollapsed = true;
        //create default tile
        CreateTile(solver.dataSet.defaultTile);
    }

    //============= Collapse Tile =================
    public void Collapse()
    {
        CreateTile(possibilities.GetRandom());
        possibilities.Clear();
    }

    private void CreateTile(int tileToCreate)
    {
        id = tileToCreate;
        createdTile = Instantiate(solver.LookupTable[tileToCreate], transform);
        isCollapsed = true;
    }

    //============= Perpetuate ==================
    public void Perpetuate(WFCTileData2D placedTile, Direction dir) //dir is the direction this tile is compared to the placed tile
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
        for (int i = 0; i < solver.dataSet.tiles.Length; i++) {
            possibilities.Add(i, solver.dataSet.tiles[i].weight);
        }
    }
}
