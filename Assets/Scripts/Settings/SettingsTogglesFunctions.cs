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
	public enum TogglesSetTypes
	{
		allowInstallsDuringGameplay,
		disableLogging,
		limitFPS,
		lowPerf,
		discordIntegration,
		shortcuts
	}

	public Toggle[] toggles;
	public TogglesSetTypes[] setTypes;
	[Header("FPS Limit Disable UI")]
	public TextMeshProUGUI lowPerfText;
	public Graphic lowPerfCheckmark;
	Color lowPerfTextNormalColor;
	public Color limitDisabledColor;
	public Toggle lowPerfToggle;
	public float fadeDuration;

	void Awake()
	{
		for (int i = 0; i < Mathf.Min(toggles.Length, setTypes.Length); i++)
		{
			toggles[i].isOn = (setTypes[i]) switch
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
		lowPerfTextNormalColor = lowPerfText.color;
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

		lowPerfText.CrossFadeColor(!limit ? limitDisabledColor : lowPerfTextNormalColor, fadeDuration, true, true);
		lowPerfCheckmark.CrossFadeColor(!limit ? limitDisabledColor : lowPerfTextNormalColor, fadeDuration, true, true);
		lowPerfToggle.interactable = limit;
		if (!limit)
			lowPerfToggle.GetComponent<ToggleCheckmarkSprite>().SetToggle(false);
		else
			lowPerfToggle.GetComponent<ToggleCheckmarkSprite>().SetToggle(lowPerfToggle.isOn);
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