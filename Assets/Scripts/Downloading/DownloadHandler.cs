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

using NALStudio.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadHandler : MonoBehaviour
{
	List<string> URLQueue = new List<string>();
	List<ulong> sizes = new List<ulong>();
	UnityWebRequest downloadRequest = null;
	ulong downloadSize = 0;

	public void QueueDownload(string URL, ulong size)
	{
		URLQueue.Add(URL);
		sizes.Add(size);
	}

	IEnumerator DownloadZip(string GameName, string URL)
	{
		downloadRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
		downloadRequest.downloadHandler = new DownloadHandlerFile("F:/TestDownload/test.zip");
		yield return downloadRequest.SendWebRequest();
		if (downloadRequest.isHttpError || downloadRequest.isNetworkError)
		{
			if (downloadRequest.isNetworkError)
				Debug.LogWarning($"No internet connection! Detailed error: {downloadRequest.error}");
			else
				Debug.LogError($"That download does not exist! Detailed error: {downloadRequest.error}");
			yield break;
		}
		else
			downloadRequest = null;
	}

	void Update()
	{
		if(downloadRequest != null)
		{
			print(downloadRequest.downloadedBytes);
			print(downloadSize);
			print(Convert.BytesToMB(downloadSize));
		}
		else if (URLQueue.Count > 0)
		{
			string URL = URLQueue[0];
			downloadSize = sizes[0];
			URLQueue.RemoveAt(0);
			sizes.RemoveAt(0);
			StartCoroutine(DownloadZip("Test", URL));
		}
	}
}
