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
		discordIntegration
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
				TogglesSetTypes.allowInstallsDuringGameplay => SettingsManager.Settings.allowInstallsDuringGameplay,
				TogglesSetTypes.disableLogging => SettingsManager.Settings.disableLogging,
				TogglesSetTypes.limitFPS => SettingsManager.Settings.limitFPS,
				TogglesSetTypes.lowPerf => SettingsManager.Settings.lowPerfMode,
				TogglesSetTypes.discordIntegration => SettingsManager.Settings.enableDiscordIntegration,
				_ => false,
			};
		}
		lowPerfTextNormalColor = lowPerfText.color;
	}

	public void AllowInstallsGameplay(bool allow)
	{
		SettingsManager.Settings.allowInstallsDuringGameplay = allow;
	}

	public void DisableLogging(bool disable)
	{
		if (disable)
			Debug.Log("Debug Logging disabled by user.");
		SettingsManager.Settings.disableLogging = disable;
		if (!disable)
			Debug.Log("Debug Logging enabled by user.");
	}

	public void LimitFPS(bool limit)
	{
		SettingsManager.Settings.limitFPS = limit;

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
		SettingsManager.Settings.lowPerfMode = on;
	}

	public void DiscordIntegration(bool on)
	{
		SettingsManager.Settings.enableDiscordIntegration = on;
	}
}