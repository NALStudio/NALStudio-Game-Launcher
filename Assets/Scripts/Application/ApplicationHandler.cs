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
using UnityEngine.Analytics;
using NALStudio.GameLauncher.Constants;

public class ApplicationHandler : MonoBehaviour
{
	public static bool HasFocus { get; private set; }
	public static bool ShortcutsEnabled { get; private set; }

	void Awake()
	{
		if (Debug.isDebugBuild)
			Analytics.enabled = false;
	}

	void Start()
	{
		/* It was a fucking stupid idea to collect this information
		AnalyticsEvent.Custom("Application Start", new Dictionary<string, object>()
		{
			{ "Version", Application.version },
			{ "Graphics API", SystemInfo.graphicsDeviceVersion },
			{ "GPU", SystemInfo.graphicsDeviceName },
			{ "VRAM", SystemInfo.graphicsMemorySize },
			{ "CPU", SystemInfo.processorType },
			{ "CPU Core Count", SystemInfo.processorCount },
			{ "OS", SystemInfo.operatingSystem }
		});
		*/

		if ((!File.Exists(Constants.LaunchPath) || PlayerPrefs.GetString("shortcut_launcher_version", null) != System.Diagnostics.FileVersionInfo.GetVersionInfo(Path.Combine(Application.streamingAssetsPath, "NALStudioGameLauncherShortcutLaunch.exe")).FileVersion) && SettingsManager.Settings.AllowShortcuts)
		{
			bool createFile = true;
			System.Diagnostics.ProcessStartInfo registryStart = new System.Diagnostics.ProcessStartInfo
			{
				FileName = Path.Combine(Application.streamingAssetsPath, "NALStudioGameLauncherRegistry.exe"),
				Arguments = $"\"{Constants.LaunchPath}\"",
				CreateNoWindow = true
			};
			try
			{
				System.Diagnostics.Process.Start(registryStart);
				ShortcutsEnabled = true;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to start registry handler. Message: {e.Message}");

				createFile = false;
				ShortcutsEnabled = false;
				try
				{
					if (File.Exists(Constants.LaunchPath))
						File.Delete(Constants.LaunchPath);
				}
				catch (Exception f)
				{
					Debug.LogError($"Could not delete registry file. Message: {f.Message}");
				}
			}
			if (createFile)
			{
				File.Copy(Path.Combine(Application.streamingAssetsPath, "NALStudioGameLauncherShortcutLaunch.exe"), Constants.LaunchPath, true);
				PlayerPrefs.SetString("shortcut_launcher_version", System.Diagnostics.FileVersionInfo.GetVersionInfo(Constants.LaunchPath).FileVersion);
			}
		}
		else
		{
			ShortcutsEnabled = SettingsManager.Settings.AllowShortcuts;

			if (!SettingsManager.Settings.AllowShortcuts && File.Exists(Constants.LaunchPath))
			{
				Debug.Log("Deleting launch file, because user has disabled launch file updates.");
				try
				{
					if (File.Exists(Constants.LaunchPath))
						File.Delete(Constants.LaunchPath);
				}
				catch (Exception e)
				{
					Debug.LogError($"Launch file could not be deleted. Message: {e.Message}");
				}
			}
		}
	}

	void OnApplicationFocus(bool focus)
	{
		HasFocus = focus;
	}
}
