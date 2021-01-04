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

using NALStudio.GameLauncher.Games;
using NALStudio.UI;
using UnityEngine;

public class ApplicationStateHandler : MonoBehaviour
{
    public GameHandler gameHandler;
	public TabGroup tabGroup;
	public DownloadsMenu downloadsMenu;
#if UNITY_EDITOR
	public bool EditorOverride;
	public int overrideFPS;
#endif

	void OnApplicationFocus(bool hasFocus)
	{
#if UNITY_EDITOR
			if (EditorOverride)
			{
				Application.targetFrameRate = overrideFPS;
				return;
			}
#endif
		if (hasFocus)
			Application.targetFrameRate = -1;
		else if (tabGroup.SelectedButton == tabGroup.tabButtons[0])
			Application.targetFrameRate = 30;
		else if (!downloadsMenu.trueOpened || gameHandler.GetActiveData() != null)
			Application.targetFrameRate = 1;
		else
			Application.targetFrameRate = Mathf.CeilToInt(1 / Time.fixedUnscaledDeltaTime);
	}
}
