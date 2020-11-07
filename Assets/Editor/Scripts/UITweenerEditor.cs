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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NALStudio.UI.Editors
{
	[CustomEditor(typeof(UITweener)), CanEditMultipleObjects]
	public class UITweenerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			UITweener tweener = (UITweener)target;

			if (tweener.objectToAnimate == null)
				tweener.objectToAnimate = tweener.gameObject;
			tweener.objectToAnimate = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Object To Animate", "Game Object will default to component's parent object if left empty."), tweener.objectToAnimate, typeof(GameObject), true);

			GUILayout.Space(10f);

			tweener.animationType = (UIAnimationTypes)EditorGUILayout.EnumPopup(new GUIContent("Animation Type"), tweener.animationType);
			tweener.easeType = (LeanTweenType)EditorGUILayout.EnumPopup(new GUIContent("Ease Type"), tweener.easeType);
			tweener.duration = EditorGUILayout.FloatField(new GUIContent("Duration", "The duration of the animation."), tweener.duration);
			tweener.delay = EditorGUILayout.FloatField(new GUIContent("Delay", "The delay for the start of the animation."), tweener.delay);

			GUILayout.Space(10f);

			EditorGUI.BeginDisabledGroup(tweener.pingpong);
			tweener.loop = EditorGUILayout.Toggle(new GUIContent("Loop", "Loops infinitely by playing the animation over and over again."), tweener.loop);
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(tweener.loop);
			tweener.pingpong = EditorGUILayout.Toggle(new GUIContent("Ping-Pong", "Loops infinitely by playing the animation and then reversing it."), tweener.pingpong);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10f);

			tweener.startPositionOffset = EditorGUILayout.Toggle(new GUIContent("Start Position Offset", "Set a custom start position offset."), tweener.startPositionOffset);
			if (!tweener.startPositionOffset)
				tweener.from = Vector3.zero;
			if (!tweener.endPositionOffset)
				tweener.to = Vector3.one;
			EditorGUI.BeginDisabledGroup(!tweener.startPositionOffset);
			if(tweener.animationType == UIAnimationTypes.Fade)
					tweener.from.x = EditorGUILayout.Slider(new GUIContent("From"), tweener.from.x, 0f, 1f);
			else
					tweener.from = EditorGUILayout.Vector3Field(new GUIContent("From"), tweener.from);
			EditorGUI.EndDisabledGroup();
			tweener.endPositionOffset = EditorGUILayout.Toggle(new GUIContent("End Position Offset", "Set a custom end position offset."), tweener.endPositionOffset);
			EditorGUI.BeginDisabledGroup(!tweener.endPositionOffset);
			if(tweener.animationType == UIAnimationTypes.Fade)
					tweener.to.x = EditorGUILayout.Slider(new GUIContent("To"), tweener.to.x, 0f, 1f);
			else
					tweener.to = EditorGUILayout.Vector3Field(new GUIContent("To"), tweener.to);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10f);

			tweener.showOnEnable = EditorGUILayout.Toggle(new GUIContent("Show On Enable", "Triggers the animation when the gameobject has been set active."), tweener.showOnEnable);
			tweener.workOnDisable = EditorGUILayout.Toggle(new GUIContent("Work On Disable", "Continue the animation even if the animated object is disabled."), tweener.workOnDisable);
		}
	}
}