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
using NALStudio.NALEditor;

[CustomEditor(typeof(FlexibleGridLayout)), CanEditMultipleObjects]
public class FlexibleGridLayoutEditor : NALEditor
{
    bool paddingFoldout;

	public override void OnInspectorGUI()
    {
        FlexibleGridLayout layout = (FlexibleGridLayout)target;
        serializedObject = new SerializedObject(layout);

        paddingFoldout = EditorGUILayout.Foldout(paddingFoldout, "Padding");
        EditorGUI.BeginChangeCheck();
        int left = layout.padding.left;
        int right = layout.padding.right;
        int top = layout.padding.top;
        int bottom = layout.padding.bottom;
		if (paddingFoldout)
		{
            EditorGUI.indentLevel++;
            left = EditorGUILayout.IntField("Left", layout.padding.left);
            right = EditorGUILayout.IntField("Right", layout.padding.right);
            top = EditorGUILayout.IntField("Top", layout.padding.top);
            bottom = EditorGUILayout.IntField("Bottom", layout.padding.bottom);
            EditorGUI.indentLevel--;
            GUILayout.Space(10f);
		}

        TextAnchor alignment = (TextAnchor)EditorGUILayout.EnumPopup(new GUIContent("Child Alignment", "Determines how your content will be aligned."), layout.childAlignment);
        if (EditorGUI.EndChangeCheck())
		{
            Undo.RecordObject(layout, "Changed Flexible Grid Layout");
            layout.padding.left = left;
            layout.padding.right = right;
            layout.padding.top = top;
            layout.padding.bottom = bottom;

            layout.childAlignment = alignment;

            layout.OnValidate();
		}

        GUILayout.Space(10f);
        DrawField(nameof(layout.fitType));

        EditorGUI.BeginDisabledGroup(layout.fitType != FlexibleGridLayout.FitType.FixedColumns && layout.fitType != FlexibleGridLayout.FitType.FixedRows);
        DrawField(nameof(layout.rows));
        DrawField(nameof(layout.columns));
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10f);

        EditorGUI.BeginDisabledGroup(layout.fitX && layout.fitY);
        DrawField(nameof(layout.cellSize));
        EditorGUI.EndDisabledGroup();
        DrawField(nameof(layout.spacing));

        GUILayout.Space(10f);

        EditorGUI.BeginDisabledGroup(layout.fitType != FlexibleGridLayout.FitType.FixedColumns && layout.fitType != FlexibleGridLayout.FitType.FixedRows);
        DrawField(nameof(layout.fitX));
        DrawField(nameof(layout.fitY));
        EditorGUI.EndDisabledGroup();

        Apply();
    }
}