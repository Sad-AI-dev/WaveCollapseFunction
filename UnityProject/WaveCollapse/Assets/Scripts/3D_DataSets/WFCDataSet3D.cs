using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "3D Data Set", menuName = "ScriptableObjects/3D WFC Data set")]
public class WFCDataSet3D : ScriptableObject
{
    public int airNode = 0;
    public int defaultFloorNode = 1;

    public WFCNodeData3D[] nodes;
}
