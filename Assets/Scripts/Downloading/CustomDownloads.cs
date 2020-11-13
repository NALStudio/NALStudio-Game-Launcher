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

public class CustomDownloads : MonoBehaviour
{
	public DownloadHandler downloadHandler;
	GitHubHelper githubHelper;

	void Reset()
	{
		downloadHandler = GetComponent<DownloadHandler>();
	}

	IEnumerator Start()
	{
		githubHelper = gameObject.AddComponent<GitHubHelper>();
		string downloadLink = null;
		ulong size = 0;
		yield return StartCoroutine(githubHelper.GetDownloadLink("NALStudio", "Test-Repo",
			value => downloadLink = value, value => size = Convert.ToUInt64(value)));
		if (downloadLink != null)
		{
			downloadHandler.QueueDownload(downloadLink, size);
		}
		else
			Debug.LogError("No download link defined!");
	}
}
