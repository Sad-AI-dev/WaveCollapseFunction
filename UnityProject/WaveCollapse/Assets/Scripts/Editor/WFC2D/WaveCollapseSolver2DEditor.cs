using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EditorUtil;

[CustomEditor(typeof(WaveCollapseSolver2D))]
public class WaveCollapseSolver2DEditor : Editor
{
    private WaveCollapseSolver2D solver;

    private void OnEnable()
    {
        if (!solver) {
            solver = target as WaveCollapseSolver2D;
        }
    }

    //================= Preview Buttons ======================
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Space(10f);
        //draw preview buttons
        if (GUILayout.Button("Remove Preview")) {
            DestroyPreview();
        }
        if (GUILayout.Button("Build Preview")) {
            DestroyPreview();
            CreatePreview();
        }

        InspectorDrawerUtil.DrawHeader("Tile Data Helper");
        //draw data mirroring button
        if (GUILayout.Button("Mirror Tile Data")) {
            TryMirrorTileData();
        }
    }

    //==================================================================================================
    //====================================== PREVIEWS ==================================================
    //==================================================================================================
    //============ Destroy Preview ================
    private void DestroyPreview()
    {
        for (int i = solver.previewHolder.childCount - 1; i >= 0; i--) {
            DestroyImmediate(solver.previewHolder.GetChild(i).gameObject);
        }
    }

    //============== Preview Creation ==============
    private void CreatePreview()
    {
        InitializeSolverVars();
        CreatePreviewObjects();
    }
    private void InitializeSolverVars()
    {
        solver.GenerateLookupTable();
    }

    private void CreatePreviewObjects()
    {
        for (int i = 0; i < solver.dataSet.tiles.Length; i++) {
            //store data
            WFCTileData2D currentTile = solver.dataSet.tiles[i];
            int maxConnections = GetMaxConnections(currentTile);
            //build
            CreateBaseObjects(currentTile, i, maxConnections);
            CreateNeighbourObjects(currentTile, i);
        }
    }
    private int GetMaxConnections(WFCTileData2D tile)
    {
        //get all lengths
        int[] lengths = new int[] { 
            tile.topConnections.Count,
            tile.rightConnections.Count,
            tile.bottomConnections.Count,
            tile.leftConnections.Count 
        };

        //search for largest
        int longestIndex = 0;
        for (int i = 1; i < lengths.Length; i++) {
            if (lengths[i] > lengths[longestIndex]) {
                longestIndex = i;
            }
        }

        //return results
        return lengths[longestIndex];
    }

    private void CreateBaseObjects(WFCTileData2D tile, int index, int steps)
    {
        for (int i = 0; i < steps; i++) {
            PlacePrefab(tile.prefab, solver.previewHolder, solver.previewSpacing * new Vector2(i, index));
        }
    }

    private void CreateNeighbourObjects(WFCTileData2D tile, int index)
    {
        List<int>[] neighbourData = new List<int>[] {
            tile.topConnections.GetKeyList(),
            tile.rightConnections.GetKeyList(),
            tile.bottomConnections.GetKeyList(),
            tile.leftConnections.GetKeyList()
        };

        for (int i = 0; i < 4; i++) {
            CreateNeighbourObjectSet(neighbourData[i], index, DirToOffset(i));
        }
    }
    private Vector2 DirToOffset(int dir) //dir : 0 = up, 1 = right, 2 = down, 3 = left
    {
        return dir switch {
            0 => new Vector2(0, 1),
            1 => new Vector2(1, 0),
            2 => new Vector2(0, -1),
            3 => new Vector2(-1, 0),
            _ => Vector2.zero, //should never be used
        };
    }

    private void CreateNeighbourObjectSet(List<int> prefabs, int index, Vector2 offset) 
    {
        for (int i = 0; i < prefabs.Count; i++) {
            PlacePrefab(solver.lookupTable[prefabs[i]], solver.previewHolder, (solver.previewSpacing * new Vector2(i, index)) + offset);
        }
    }

    //============== Generic Place Prefab ==================
    private void PlacePrefab(GameObject prefab, Transform holder, Vector2 localPosition)
    {
        GameObject obj = PrefabUtility.InstantiatePrefab(prefab, holder) as GameObject;
        obj.transform.localPosition = localPosition;
    }

    //============================================================================================================
    //=========================================== Data Mirroring =================================================
    //============================================================================================================
    private void TryMirrorTileData()
    {
        InitializeSolverVars();

        for (int i = 0; i < solver.dataSet.tiles.Length; i++) {
            MirrorTileData(solver.dataSet.tiles[i], i);
        }
    }

    private void MirrorTileData(WFCTileData2D tile, int tileIndex)
    {
        //mirror top
        foreach (int connection in tile.topConnections.Keys()) {
            if(!solver.dataSet.tiles[connection].bottomConnections.Contains(tileIndex)) { //data is not yet mirrored
                solver.dataSet.tiles[connection].bottomConnections.Add(tileIndex, 1f);
            }
        }
        //mirror right
        foreach (int connection in tile.rightConnections.Keys()) {
            if(!solver.dataSet.tiles[connection].leftConnections.Contains(tileIndex)) { //data is not yet mirrored
                solver.dataSet.tiles[connection].leftConnections.Add(tileIndex, 1f);
            }
        }
        //mirror bottom
        foreach (int connection in tile.bottomConnections.Keys()) {
            if(!solver.dataSet.tiles[connection].topConnections.Contains(tileIndex)) { //data is not yet mirrored
                solver.dataSet.tiles[connection].topConnections.Add(tileIndex, 1f);
            }
        }
        //mirror left
        foreach (int connection in tile.leftConnections.Keys()) {
            if(!solver.dataSet.tiles[connection].rightConnections.Contains(tileIndex)) { //data is not yet mirrored
                solver.dataSet.tiles[connection].rightConnections.Add(tileIndex, 1f);
            }
        }
    }
}
