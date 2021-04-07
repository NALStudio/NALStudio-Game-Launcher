using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NALStudio.NALEditor
{
	public class NALEditor : Editor
	{
		new protected SerializedObject serializedObject;
		protected SerializedProperty currentProperty;

		string selectedPropertyPath;
		protected SerializedProperty selectedProperty;

		protected void DrawProperties(SerializedProperty property, bool drawChildren)
		{
			string lastPropPath = string.Empty;
			foreach (SerializedProperty p in property)
			{
				if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
				{
					EditorGUILayout.BeginHorizontal();
					p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
					EditorGUILayout.EndHorizontal();

					if (p.isExpanded)
					{
						EditorGUI.indentLevel++;
						DrawProperties(p, drawChildren);
						EditorGUI.indentLevel--;
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath))
						continue;

					lastPropPath = p.propertyPath;
					EditorGUILayout.PropertyField(p, drawChildren);
				}
			}
		}

		protected void DrawSidebar(SerializedProperty property)
		{
			foreach (SerializedProperty p in property)
			{
				if (GUILayout.Button(p.displayName))
					selectedPropertyPath = p.propertyPath;
			}

			if (!string.IsNullOrEmpty(selectedPropertyPath))
				selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
		}

		protected void DrawField(string propertyName, bool relative)
		{
			if (relative && currentProperty != null)
				EditorGUILayout.PropertyField(currentProperty.FindPropertyRelative(propertyName), true);
			else if (serializedObject != null)
				EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), true);
		}

		protected void DrawField(string propertyName)
		{
			if (serializedObject != null)
				EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), true);
		}

		protected void Apply()
		{
			serializedObject.ApplyModifiedProperties();
		}
	}
}