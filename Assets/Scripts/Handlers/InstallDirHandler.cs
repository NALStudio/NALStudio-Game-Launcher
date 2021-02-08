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

	string path;

	CardHandler.CardData cardData;

	public IEnumerator Prompt(CardHandler.CardData cData, Action<bool, bool, string> onComplete)
	{
		cardData = cData;
		gameObject.SetActive(true);
		pathInput.text = Constants.GamesPath;
		yield return new WaitUntil(() => close);
		close = false;
		tweener.DoTween(true);
		bool finished = false;
		tweener.OnComplete += () => finished = true;
		yield return new WaitUntil(() => finished);
		onComplete.Invoke(true, success, path == Constants.GamesPath ? null : path);
		gameObject.SetActive(false);
	}

	public void Close(bool _success)
	{
		close = true;
		if (!_success)
			success = false;
		else
			success = PathValid(path);
	}

	public void CustomLocation()
	{
		string[] locations = StandaloneFileBrowser.OpenFolderPanel(null, null, false);
		if (locations.Length > 0 && !string.IsNullOrEmpty(locations[0]))
		{
			pathInput.text = locations[0];
		}
	}

	bool PathValid(string path)
	{
		try
		{
			// Attempt to get a list of security permissions from the folder. 
			// This will raise an exception if the path is read only or do not have access to view the permissions. 
			System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(path);
			return Path.IsPathRooted(path) && Directory.Exists(path);
		}
		catch { }
		return false;
	}

	void Update()
	{
		path = pathInput.text;
		path = path.Replace('\\', '/');
		path = Regex.Replace(path, @"\/+", "/");
		path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		bool pValid = PathValid(path);
		invalidPath.SetActive(!pValid);
		installButton.interactable = pValid;
		pathText.text = Path.Combine(path, cardData != null ? cardData.title : "[ERROR]");
	}
}