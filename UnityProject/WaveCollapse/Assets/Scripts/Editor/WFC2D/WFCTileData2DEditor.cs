using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WFCTileData2D))]
public class WFCTileData2DEditor : Editor
{
    private WFCTileData2D tileData;

    //preview vars
    private WFCDataSet2D dataSet;


    private void OnEnable()
    {
        if (!tileData) {
            tileData = target as WFCTileData2D;
        }
    }

    //================ Draw Inspector Options ================
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        //draw editor fields
        DrawObjectField(ref dataSet, "Data Set");

        //draw preview buttons
    }

    private void DrawObjectField<T> (ref T var, string fieldName = "") where T : Object
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(fieldName, GUILayout.Width(100), GUILayout.ExpandWidth(true));
        var = EditorGUILayout.ObjectField(var, typeof(T), false) as T;
        EditorGUILayout.EndHorizontal();
    }

    //=============== Create Scene Preview ====================
    private void DestroyPreview()
    {

    }
    
    private void CreatePreview()
    {

    }
}
