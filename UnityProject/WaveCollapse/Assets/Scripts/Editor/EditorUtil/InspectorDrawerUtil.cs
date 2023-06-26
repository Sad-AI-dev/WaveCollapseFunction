using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorUtil {
    public static class InspectorDrawerUtil
    {
        //================ Headers =================
        public static void DrawHeader(string text)
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        public static void DrawCenteredHeader(string text)
        {
            EditorGUILayout.Space(10f);
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField(text, headerStyle ,GUILayout.ExpandWidth(true));
        }

        //================ Editor-side Fields ==================
        public static void DrawObjectField<T> (ref T var, string fieldName = "", bool allowSceneObjects = false) where T : Object
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(fieldName, GUILayout.Width(100), GUILayout.ExpandWidth(true));
            var = EditorGUILayout.ObjectField(var, typeof(T), allowSceneObjects) as T;
            EditorGUILayout.EndHorizontal();
        }
    }
}
