/*
 ██████   █████   █████████   █████        █████████   █████                   █████  ███          
░░██████ ░░███   ███░░░░░███ ░░███        ███░░░░░███ ░░███                   ░░███  ░░░           
 ░███░███ ░███  ░███    ░███  ░███       ░███    ░░░  ███████   █████ ████  ███████  ████   ██████ 
 ░███░░███░███  ░███████████  ░███       ░░█████████ ░░░███░   ░░███ ░███  ███░░███ ░░███  ███░░███
 ░███ ░░██████  ░███░░░░░███  ░███        ░░░░░░░░███  ░███     ░███ ░███ ░███ ░███  ░███ ░███ ░███
 ░███  ░░█████  ░███    ░███  ░███      █ ███    ░███  ░███ ███ ░███ ░███ ░███ ░███  ░███ ░███ ░███
 █████  ░░█████ █████   █████ ███████████░░█████████   ░░█████  ░░████████░░████████ █████░░██████ 
░░░░░    ░░░░░ ░░░░░   ░░░░░ ░░░░░░░░░░░  ░░░░░░░░░     ░░░░░    ░░░░░░░░  ░░░░░░░░ ░░░░░  ░░░░░░       

Copyright © 2020 NALStudio. All Rights Reserved.
*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlexibleGridLayout)), CanEditMultipleObjects]
public class FlexibleGridLayoutEditor : Editor
{
    public SerializedProperty childAlignmentSerialized;
    public SerializedProperty rowsSerialized;
    public SerializedProperty columnsSerialized;
    public SerializedProperty cellSizeSerialized;
    public SerializedProperty spacingSerialized;
    public SerializedProperty fitXSerialized;
    public SerializedProperty fitYSerialized;

    static bool paddingFoldout;

    void OnEnable()
    {
        rowsSerialized = serializedObject.FindProperty("rows");
        columnsSerialized = serializedObject.FindProperty("columns");
        cellSizeSerialized = serializedObject.FindProperty("cellSize");
        spacingSerialized = serializedObject.FindProperty("spacing");
        fitXSerialized = serializedObject.FindProperty("fitX");
        fitYSerialized = serializedObject.FindProperty("fitY");
    }

    public override void OnInspectorGUI()
    {
        FlexibleGridLayout layout = (FlexibleGridLayout)target;
        Color defaultColor = GUI.color;
        Color defaultContentColor = GUI.contentColor;
        paddingFoldout = EditorGUILayout.Foldout(paddingFoldout, "Padding");
		if (paddingFoldout)
		{
            layout.padding.left = EditorGUILayout.IntField("     Left", layout.padding.left);
            layout.padding.right = EditorGUILayout.IntField("     Right", layout.padding.right);
            layout.padding.top = EditorGUILayout.IntField("     Top", layout.padding.top);
            layout.padding.bottom = EditorGUILayout.IntField("     Bottom", layout.padding.bottom);
            GUILayout.Space(10f);
		}
        layout.childAlignment = (TextAnchor)EditorGUILayout.EnumPopup(new GUIContent("Child Alignment", "Determines how your content will be aligned."), layout.childAlignment);
        GUILayout.Space(10f);
        layout.fitType = (FlexibleGridLayout.FitType)EditorGUILayout.EnumPopup(new GUIContent("Fit Type", "Determines how your content will be fitted."), layout.fitType);
        if(layout.fitType == FlexibleGridLayout.FitType.FixedColumns || layout.fitType == FlexibleGridLayout.FitType.FixedRows)
        {
            layout.rows = EditorGUILayout.DelayedIntField(new GUIContent("Rows"), layout.rows);
            layout.columns = EditorGUILayout.DelayedIntField(new GUIContent("Columns"), layout.columns);
            GUILayout.Space(10f);
            EditorGUI.BeginDisabledGroup(layout.fitX && layout.fitY);
            layout.cellSize = EditorGUILayout.Vector2Field("Cell Size", layout.cellSize);
            EditorGUI.EndDisabledGroup();
            layout.spacing = EditorGUILayout.Vector2Field("Spacing", layout.spacing);
            GUILayout.Space(10f);
            layout.fitX = EditorGUILayout.Toggle("Fit X", layout.fitX);
            layout.fitY = EditorGUILayout.Toggle("Fit Y", layout.fitY);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            layout.rows = EditorGUILayout.DelayedIntField(new GUIContent("Rows"), layout.rows);
            layout.columns = EditorGUILayout.DelayedIntField(new GUIContent("Columns"), layout.columns);
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(10f);
            EditorGUI.BeginDisabledGroup(layout.fitX && layout.fitY);
            layout.cellSize = EditorGUILayout.Vector2Field("Cell Size", layout.cellSize);
            EditorGUI.EndDisabledGroup();
            layout.spacing = EditorGUILayout.Vector2Field("Spacing", layout.spacing);
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Space(10f);
            layout.fitX = EditorGUILayout.Toggle("Fit X", layout.fitX);
            layout.fitY = EditorGUILayout.Toggle("Fit Y", layout.fitY);
            EditorGUI.EndDisabledGroup();
        }
    }
}