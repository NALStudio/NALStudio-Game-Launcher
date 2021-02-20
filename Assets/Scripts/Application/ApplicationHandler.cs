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
using UnityEngine;
using UnityEngine.Analytics;

public class ApplicationHandler : MonoBehaviour
{
	public static bool HasFocus { get; private set; }

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
	}

	void OnApplicationFocus(bool focus)
	{
		HasFocus = focus;
	}
}
