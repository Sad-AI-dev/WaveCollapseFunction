using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Data 2D", menuName = "ScriptableObjects/2D WFC Tile Data")]
public class WFCTileData2D : ScriptableObject
{
    public int owner;

    [Header("Connection settings")]
    public WeightedChance<int> topConnections = new();
    public WeightedChance<int> rightConnections = new();
    public WeightedChance<int> bottomConnections = new();
    public WeightedChance<int> leftConnections = new();
}
