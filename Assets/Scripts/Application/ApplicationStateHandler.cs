﻿/*
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

using NALStudio.GameLauncher.Games;
using NALStudio.UI;
using UnityEngine;

public class ApplicationStateHandler : MonoBehaviour
{
	public GameHandler gameHandler;
	public TabGroup tabGroup;
	public DownloadsMenu downloadsMenu;

	void OnApplicationFocus(bool hasFocus)
	{
		#region Focused
		if (hasFocus || !SettingsManager.Settings.LimitFPS)
		{
			Application.targetFrameRate = -1;
		}
		#endregion
		#region Unfocused
		else
		{
			if (gameHandler.gameRunning || SettingsManager.Settings.LowPerfMode)
				Application.targetFrameRate = 1;
			else if (tabGroup.SelectedButton == tabGroup.tabButtons[0])
				Application.targetFrameRate = 30;
			else if (!downloadsMenu.trueOpened)
				Application.targetFrameRate = 1;
			else
				Application.targetFrameRate = Mathf.CeilToInt(1 / Time.fixedDeltaTime);
			// Manually empty garbage to free up RAM
			Resources.UnloadUnusedAssets();
			System.GC.Collect();
		}
		#endregion
	}
}
