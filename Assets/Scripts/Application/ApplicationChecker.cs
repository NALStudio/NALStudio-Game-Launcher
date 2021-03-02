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
using UnityEngine.Events;
using UnityEngine.Networking;
using SimpleJSON;
using NALStudio.GameLauncher.Networking;

public class ApplicationChecker : MonoBehaviour
{
    #region Variables
    public UnityEvent OnUpdateAvailable;
	public UnityEvent<bool> NoInternet;
	#endregion

    public void OpenRelease() { Application.OpenURL("https://github.com/NALStudio/NALStudio-Game-Launcher/releases/latest"); }

	void Awake()
	{
		StartCoroutine(CheckForUpdate());
		NetworkManager.InternetAvailabilityChange += (available) => NoInternet?.Invoke(!available);
	}

	IEnumerator CheckForUpdate()
	{
		UnityWebRequest req = UnityWebRequest.Get("https://api.github.com/repos/NALStudio/NALStudio-Game-Launcher/releases/latest");
		yield return req.SendWebRequest();
		if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.DataProcessingError || req.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.LogError(req.error);
		}
		else
		{
			yield return new WaitWhile(() => req.downloadProgress < 1f);
			string json = req.downloadHandler.text;
			JSONNode node = JSON.Parse(json);
			if (node["tag_name"] != Application.version)
			{
				Debug.Log($"Found an update for version: {node["tag_name"]}. Current Version: \"{Application.version}\"");
				OnUpdateAvailable?.Invoke();
			}
		}
	}
}
