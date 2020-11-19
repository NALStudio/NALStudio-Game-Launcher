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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NALStudio.ConfigManager
{
	public class ConfigManager : MonoBehaviour
	{
		public SettingsHolder Settings = new SettingsHolder();

		[Serializable]
		public class SettingsHolder
		{
			public List<string> installDirs;
		}

		string settingsFileLocation = Path.Combine(Application.persistentDataPath, "settings.nal");

		public void SaveSettings()
		{
			string json = JsonUtility.ToJson(Settings);
			File.WriteAllText(settingsFileLocation, json);
		}

		public void LoadSettings()
		{
			string json = File.ReadAllText(settingsFileLocation);
			Settings = JsonUtility.FromJson<SettingsHolder>(json);
		}
	}
}
