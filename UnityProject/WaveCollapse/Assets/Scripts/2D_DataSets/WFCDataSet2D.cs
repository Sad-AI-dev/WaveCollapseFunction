using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "2D data set", menuName = "ScriptableObjects/2D WFC Data set")]
public class WFCDataSet2D : ScriptableObject
{
    public int defaultTile = 0;
    public WFCTileData2D[] tiles;
}
