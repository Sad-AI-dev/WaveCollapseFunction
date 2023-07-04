using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            MarkSceneDirty();
        }
        if (GUILayout.Button("Build Preview")) {
            DestroyPreview();
            CreatePreview();
            MarkSceneDirty();
        }

        InspectorDrawerUtil.DrawHeader("Tile Data Helper");
        //draw data mirroring button
        if (GUILayout.Button("Mirror Tile Data")) {
            TryMirrorTileData();
        }
    }
    private void MarkSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
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
            tile.topConnections,
            tile.rightConnections,
            tile.bottomConnections,
            tile.leftConnections
        };

        for (int i = 0; i < 4; i++) {
            CreateNeighbourObjectSet(neighbourData[i], index, DirUtil.DirToV2((Direction)i));
        }
    }

    private void CreateNeighbourObjectSet(List<int> prefabs, int index, Vector2 offset) 
    {
        for (int i = 0; i < prefabs.Count; i++) {
            PlacePrefab(solver.LookupTable[prefabs[i]], solver.previewHolder, (solver.previewSpacing * new Vector2(i, index)) + offset);
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
        bool isDirty = false;
        for (int i = 0; i < 4; i++) {
            List<int> checkList = tile.ConnectionsFromDirection((Direction)i);
            
            foreach (int connection in checkList) {
                List<int> mirrorList = solver.dataSet.tiles[connection].ConnectionsFromDirection((Direction)((i + 2) % 4));

                if (!mirrorList.Contains(tileIndex)) {
                    mirrorList.Add(tileIndex);
                    isDirty = true;
                }
            }
        }
        if (isDirty) {
            EditorUtility.SetDirty(tile); //mark scriptable object dirty
            AssetDatabase.SaveAssetIfDirty(tile);
        }
    }
}
