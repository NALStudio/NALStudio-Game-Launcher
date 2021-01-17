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

using NALStudio.GameLauncher.Constants;
using SFB;
using System;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NALStudio.GameLauncher.Cards;
using System.Text.RegularExpressions;
using NALStudio.UI;

public class InstallDirHandler : MonoBehaviour
{
	public GameObject invalidPath;

	public TMP_InputField pathInput;
	public TextMeshProUGUI pathText;

	public Button installButton;

	public UITweener tweener;

	bool close;
	bool success;

	CardHandler.CardData cardData;

	public IEnumerator Prompt(CardHandler.CardData cData, Action<bool, bool, string> onComplete)
	{
		gameObject.SetActive(true);
		pathInput.text = Constants.GamesPath;
		yield return new WaitWhile(() => !close);
		close = false;
		tweener.DoTween(true);
		bool finished = false;
		tweener.OnComplete += () => finished = true;
		yield return new WaitWhile(() => !finished);
		onComplete.Invoke(true, success, pathText.text == Constants.GamesPath ? null : pathText.text);
		gameObject.SetActive(false);
		cardData = cData;
	}

	public void Close(bool _success)
	{
		close = true;
		success = _success;
	}

	public void CustomLocation()
	{
		string[] locations = StandaloneFileBrowser.OpenFolderPanel(null, Constants.GamesPath, false);
		if (locations.Length > 0 && !string.IsNullOrEmpty(locations[0]))
		{
			pathInput.text = locations[0];
		}
	}

	bool PathValid(string path)
	{
		try
		{
			Path.GetFullPath(path);
			if (Path.IsPathRooted(path))
			{
				foreach (DriveInfo di in DriveInfo.GetDrives())
				{
					if (di.Name == path.Substring(0, 3))
						return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	void Update()
	{
		string path = pathInput.text;
		path = path.Replace('\\', '/');
		path = Regex.Replace(path, @"\/+", "/");
		path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		path = Path.Combine(path ?? "[ERROR]", cardData?.title ?? "[ERROR]");
		bool pValid = PathValid(path);
		invalidPath.SetActive(!pValid);
		installButton.interactable = pValid;
		pathText.text = path;
	}
}
