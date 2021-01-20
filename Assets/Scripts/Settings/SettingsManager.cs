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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
	[Serializable]
	class SettingsException : Exception
	{

		public SettingsException() : base() { }

		public SettingsException(string message) : base(message) { }

		public SettingsException(string message, Exception innerException) : base(message, innerException) { }
	}

	static string path = string.Empty;

	public static SettingsHolder Settings = null;
	public static bool Loaded = false;
	static SettingsManager active = null;

	void Awake()
	{
		if (active == null)
			active = this;
		else
			throw new SettingsException("Only one Settings Manger allowed per scene!");

		path = Path.Combine(Application.persistentDataPath, "settings.nal");
		Load();
		Loaded = true;
	}

#if UNITY_EDITOR
	void Reset()
	{
		UnityEditor.MonoScript monoScript = UnityEditor.MonoScript.FromMonoBehaviour(this);
		UnityEditor.MonoImporter.SetExecutionOrder(monoScript, -69);
	}
#endif

	public class SettingsHolder
	{
		public Dictionary<string, string> customGamePaths = new Dictionary<string, string>();
		public bool disableLogging = false;
		public bool allowInstallsDuringGameplay = true;
		public bool limitFPS = true;
		public bool lowPerfMode = false;

		public ParsableSettingsHolder ToParsable()
		{
			ParsableSettingsHolder parsable = new ParsableSettingsHolder();
			parsable.customGamePathKeys = customGamePaths.Keys.ToArray();
			parsable.customGamePathValues = customGamePaths.Values.ToArray();
			parsable.allowInstallsDuringGameplay = allowInstallsDuringGameplay;
			parsable.disableLogging = disableLogging;
			parsable.limitFPS = limitFPS;
			parsable.lowPerfMode = lowPerfMode;
			return parsable;
		}
	}

	[System.Serializable]
	public class ParsableSettingsHolder
	{
		public string[] customGamePathKeys = new string[0];
		public string[] customGamePathValues = new string[0];
		public bool allowInstallsDuringGameplay = true;
		public bool disableLogging = false;
		public bool limitFPS = true;
		public bool lowPerfMode = false;

		public SettingsHolder ToUsable()
		{
			SettingsHolder usable = new SettingsHolder();
			usable.customGamePaths = new Dictionary<string, string>();
			for (int i = 0; i < Mathf.Min(customGamePathKeys.Length, customGamePathValues.Length); i++)
			{
				usable.customGamePaths.Add(customGamePathKeys[i], customGamePathValues[i]);
			}
			usable.disableLogging = disableLogging;
			usable.allowInstallsDuringGameplay = allowInstallsDuringGameplay;
			return usable;
		}
	}

	public static void Save()
	{
		File.WriteAllText(path, JsonUtility.ToJson(Settings.ToParsable(), true));
	}

	public static void Load()
	{
		if (!File.Exists(path))
		{
			Settings = new SettingsHolder();
			Save();
		}
		else
		{
			try
			{
				Settings = JsonUtility.FromJson<ParsableSettingsHolder>(File.ReadAllText(path)).ToUsable();
			}
			catch 
			{
				Settings = new SettingsHolder();
				Save();
			}
		}
	}

	public static void ResetSettings()
	{
		Settings = new SettingsHolder();
		Save();
	}

	void OnApplicationQuit()
	{
		Save();
	}
}