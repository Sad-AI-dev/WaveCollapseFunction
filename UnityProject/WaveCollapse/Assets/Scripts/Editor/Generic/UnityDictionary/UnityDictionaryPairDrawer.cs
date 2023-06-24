using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(UnityDictionary<,>.Pair))]
public class UnityDictionaryPairDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        float partialSize = position.width * 0.5f;
        Rect keyRect = new Rect(position.x, position.y, partialSize, position.height);
        Rect valueRect = new Rect(position.x + partialSize + 5, position.y, position.width - partialSize + 5, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUIUtility.labelWidth = 40; //set label size
        EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), new GUIContent("Key"));
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), new GUIContent("Value"));

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
