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
using Microsoft.Win32;
using NALStudio.GameLauncher.Constants;
using System.Security.Permissions;
using System.Security.Principal;

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
		AnalyticsEvent.Custom("Application Start", new Dictionary<string, object>()
		{
			{ "Version", Application.version },
			{ "Graphics API", SystemInfo.graphicsDeviceVersion },
			{ "GPU", SystemInfo.graphicsDeviceName },
			{ "VRAM", SystemInfo.graphicsMemorySize },
			{ "CPU", SystemInfo.processorType },
			{ "CPU Core Count", SystemInfo.processorCount },
			{ "OS", SystemInfo.operatingSystem },
			{ "Internet", Application.internetReachability != NetworkReachability.NotReachable }
		});

		if (!File.Exists(Constants.LaunchPath))
		{
			File.Copy(Path.Combine(Application.streamingAssetsPath, "NALStudioGameLauncherShortcutLaunch.exe"), Constants.LaunchPath);

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
				Debug.LogError(e.Message);

				ShortcutsEnabled = false;
				File.Delete(Constants.LaunchPath);
			}
		}
		else
		{
			ShortcutsEnabled = true;
		}
	}

	void OnApplicationFocus(bool focus)
	{
		HasFocus = focus;
	}
}
