using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Data 2D", menuName = "ScriptableObjects/2D WFC Tile Data")]
public class WFCTileData2D : ScriptableObject
{
    public GameObject prefab;
    public float weight = 1f;

    [Header("Connection settings")]
    public List<int> topConnections = new();
    public List<int> rightConnections = new();
    public List<int> bottomConnections = new();
    public List<int> leftConnections = new();

    public List<int> ConnectionsFromDirection(Direction dir)
    {
        return dir switch { 
            Direction.north => topConnections,
            Direction.east => rightConnections,
            Direction.south => bottomConnections,
            Direction.west => leftConnections,
            _ => null //should never happen
        };
    }
}
