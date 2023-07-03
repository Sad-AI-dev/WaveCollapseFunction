using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EditorUtil;

[CustomEditor(typeof(WaveCollapseSolver3D))]
public class WaveCollapseSolver3DEditor : Editor
{
    private WaveCollapseSolver3D solver;

    private void OnEnable()
    {
        if (!solver) {
            solver = target as WaveCollapseSolver3D;
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

        InspectorDrawerUtil.DrawHeader("Node Data Helper");
        //draw data mirroring button
        if (GUILayout.Button("Mirror Node Data")) {
            TryMirrorNodeData();
        }
        if (GUILayout.Button("Remove Single-sided Node Data")) {
            TryRemoveSingleSidedConnections();
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
        solver.InitializeDataSet();
    }

    private void CreatePreviewObjects()
    {
        for (int i = 0; i < solver.dataSet.nodes.Length; i++) {
            //store data
            WFCNodeData3D currentNode = solver.dataSet.nodes[i];
            int maxConnections = GetMaxConnections(currentNode);
            //build
            CreateBaseObjects(currentNode, i, maxConnections);
            CreateNeighbourObjects(currentNode, i);
        }
    }
    private int GetMaxConnections(WFCNodeData3D node)
    {
        //get all lengths
        int[] lengths = new int[] {
            node.northConnections.Count, node.eastConnections.Count,
            node.southConnections.Count, node.westConnections.Count,
            node.topConnections.Count, node.bottomConnections.Count
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

    private void CreateBaseObjects(WFCNodeData3D node, int index, int steps)
    {
        for (int i = 0; i < steps; i++) {
            PlacePrefab(node.prefab, solver.previewHolder, new Vector3(i * solver.previewSpacing.x, 0, index * solver.previewSpacing.y));
        }
    }

    private void CreateNeighbourObjects(WFCNodeData3D node, int index)
    {
        List<int>[] neighbourData = new List<int>[] {
            node.northConnections, node.eastConnections,
            node.southConnections, node.westConnections,
            node.topConnections, node.bottomConnections
        };

        for (int i = 0; i < 6; i++) {
            CreateNeighbourObjectSet(neighbourData[i], index, DirUtil.DirToV3((Direction)i));
        }
    }

    private void CreateNeighbourObjectSet(List<int> prefabs, int index, Vector3 offset) 
    {
        for (int i = 0; i < prefabs.Count; i++) {
            Vector3 pos = new Vector3(i * solver.previewSpacing.x, 0, index * solver.previewSpacing.y);
            PlacePrefab(solver.LookupTable[prefabs[i]], solver.previewHolder, pos + offset);
        }
    }

    //============== Generic Place Prefab ==================
    private void PlacePrefab(GameObject prefab, Transform holder, Vector3 localPosition)
    {
        GameObject obj = PrefabUtility.InstantiatePrefab(prefab, holder) as GameObject;
        obj.transform.localPosition = localPosition;
    }

    //============================================================================================================
    //=========================================== Data Mirroring =================================================
    //============================================================================================================
    private void TryMirrorNodeData()
    {
        InitializeSolverVars();

        for (int i = 0; i < solver.dataSet.nodes.Length; i++) {
            MirrorNodeData(solver.dataSet.nodes[i], i);
        }
    }

    private void MirrorNodeData(WFCNodeData3D node, int nodeIndex)
    {
        bool isDirty = false;
        for (int i = 0; i < 6; i++) {
            List<int> checkList = node.ConnectionsFromDirection((Direction)i);
            
            foreach (int connection in checkList) {
                List<int> mirrorList = solver.dataSet.nodes[connection].ConnectionsFromDirection(DirUtil.GetOpposite(i));

                if (!mirrorList.Contains(nodeIndex)) { //other node does not contain found connection, add to mirror connection
                    mirrorList.Add(nodeIndex);
                    isDirty = true;
                }
            }
        }
        if (isDirty) {
            EditorUtility.SetDirty(node); //mark scriptable object dirty
        }
    }

    //=================================== Remove Single-Sided Connections =============================
    private void TryRemoveSingleSidedConnections()
    {
        InitializeSolverVars();

        for (int i = 0; i < solver.dataSet.nodes.Length; i++) {
            RemoveSingleSidedConnections(solver.dataSet.nodes[i], i);
        }
    }

    private void RemoveSingleSidedConnections(WFCNodeData3D node, int nodeIndex)
    {
        bool isDirty = false;

        //test each direction
        for (int i = 0; i < 6; i++) {
            List<int> compareList = node.ConnectionsFromDirection((Direction)i);

            List<int> toRemove = new();
            //check each connection
            foreach (int connection in compareList) {
                List<int> otherConnections = solver.dataSet.nodes[connection].ConnectionsFromDirection(DirUtil.GetOpposite(i));

                //test if other list also contains the connection
                if (!otherConnections.Contains(nodeIndex)) {
                    toRemove.Add(connection);
                }
            }

            //remove single sided connections found
            foreach (int removeTarget in toRemove) {
                compareList.Remove(removeTarget);
                isDirty = true;
            }
        }

        if (isDirty) {
            EditorUtility.SetDirty(node);
        }
    }
}
