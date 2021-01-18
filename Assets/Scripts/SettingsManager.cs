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

	public static SettingsHolder Settings;
	static SettingsManager active;

	void Awake()
	{
		if (active == null)
			active = this;
		else
			throw new SettingsException("Only one Settings Manger allowed per scene!");

		path = Path.Combine(Application.persistentDataPath, "settings.nal");
		Load();
	}

	public class SettingsHolder
	{
		public List<string> customGamePaths = new List<string>();
	}

	public static void Save()
	{
		File.WriteAllText(path, JsonUtility.ToJson(Settings, true));
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
			Settings = JsonUtility.FromJson<SettingsHolder>(File.ReadAllText(path));
			if (Settings == null)
			{
				Settings = new SettingsHolder();
				Save();
			}
		}
	}

	void OnApplicationQuit()
	{
		Save();
	}
}
