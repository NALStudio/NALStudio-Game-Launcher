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

using NALStudio.GameLauncher;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsTogglesFunctions : MonoBehaviour
{
	[Serializable]
	public class ToggleData
	{
		public Toggle toggle;
		public TogglesSetTypes setType;
		public Graphic checkmark;
		public TextMeshProUGUI text;
	}

	public enum TogglesSetTypes
	{
		allowInstallsDuringGameplay,
		disableLogging,
		limitFPS,
		lowPerf,
		discordIntegration,
		shortcuts
	}

	public ToggleData[] toggleDatas;
	[Header("Disabled UI")]
	public Color normalColor;
	public Color disabledColor;
	public float fadeDuration;

	void Awake()
	{
		for (int i = 0; i < toggleDatas.Length; i++)
		{
			ToggleData t = toggleDatas[i];
			t.toggle.isOn = t.setType switch
			{
				TogglesSetTypes.allowInstallsDuringGameplay => SettingsManager.Settings.AllowInstallsDuringGameplay,
				TogglesSetTypes.disableLogging => SettingsManager.Settings.DisableLogging,
				TogglesSetTypes.limitFPS => SettingsManager.Settings.LimitFPS,
				TogglesSetTypes.lowPerf => SettingsManager.Settings.LowPerfMode,
				TogglesSetTypes.discordIntegration => SettingsManager.Settings.EnableDiscordIntegration,
				TogglesSetTypes.shortcuts => SettingsManager.Settings.AllowShortcuts,
				_ => false,
			};
		}
	}

	public void DisableToggle(bool disable, TogglesSetTypes type)
	{
		ToggleData d = null;
		for (int i = 0; i < toggleDatas.Length; i++)
		{
			ToggleData check = toggleDatas[i];
			if (check.setType == type)
				d = check;
		}
		if (d == null)
		{
			Debug.LogError($"No toggle of type: {type:F} found!");
			return;
		}

		d.text.CrossFadeColor(disable ? disabledColor : normalColor, fadeDuration, true, true);
		d.checkmark.CrossFadeColor(disable ? disabledColor : normalColor, fadeDuration, true, true);
		d.toggle.interactable = !disable;
		if (disable)
			d.toggle.GetComponent<ToggleCheckmarkSprite>().SetToggle(false);
		else
			d.toggle.GetComponent<ToggleCheckmarkSprite>().SetToggle(d.toggle.isOn);
	}

	public void AllowInstallsGameplay(bool allow)
	{
		SettingsManager.Settings.AllowInstallsDuringGameplay = allow;
	}

	public void DisableLogging(bool disable)
	{
		if (disable)
			Debug.Log("Debug Logging disabled by user.");
		SettingsManager.Settings.DisableLogging = disable;
		if (!disable)
			Debug.Log("Debug Logging enabled by user.");
	}

	public void LimitFPS(bool limit)
	{
		SettingsManager.Settings.LimitFPS = limit;
		DisableToggle(!limit, TogglesSetTypes.lowPerf);
	}

	public void LowPerfMode(bool on)
	{
		SettingsManager.Settings.LowPerfMode = on;
	}

	public void DiscordIntegration(bool on)
	{
		SettingsManager.Settings.EnableDiscordIntegration = on;
		DiscordHandler.SetClient();
	}

	public void Shortcuts(bool on)
	{
		SettingsManager.Settings.AllowShortcuts = on;
	}
}