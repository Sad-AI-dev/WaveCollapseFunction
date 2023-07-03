using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node Data 3D", menuName = "ScriptableObjects/3D WFC Node Data")]
public class WFCNodeData3D : ScriptableObject
{
    public GameObject prefab;
    public float weight = 1f;

    [Header("Connection settings")]
    public List<int> northConnections = new();
    public List<int> eastConnections = new();
    public List<int> southConnections = new();
    public List<int> westConnections = new();
    public List<int> topConnections = new();
    public List<int> bottomConnections = new();

    public List<int> ConnectionsFromDirection(Direction dir)
    {
        return dir switch { 
            Direction.north => northConnections,
            Direction.east => eastConnections,
            Direction.south => southConnections,
            Direction.west => westConnections,
            Direction.up => topConnections,
            Direction.down => bottomConnections,
            _ => null //should never happen
        };
    }
}
