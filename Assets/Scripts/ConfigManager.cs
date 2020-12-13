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
using System.IO;
using UnityEngine;

namespace NALStudio.ConfigManager
{
	public static class ConfigManager
	{
		public static SettingsHolder Settings = new SettingsHolder();
		static readonly string settingsFileLocation = Path.Combine(Application.persistentDataPath, "settings.nal");

		[System.Serializable]
		public class SettingsHolder
		{
			public static List<string> installDirs = new List<string>() { Path.GetFullPath("Games") };
		}

		public static void SaveSettings()
		{
			string json = JsonUtility.ToJson(Settings, true);
			File.WriteAllText(settingsFileLocation, json);
		}

		public static void LoadSettings()
		{
			string json = File.ReadAllText(settingsFileLocation);
			Settings = JsonUtility.FromJson<SettingsHolder>(json);
		}
	}
}
