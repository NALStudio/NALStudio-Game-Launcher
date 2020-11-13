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

using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GitHubHelper : MonoBehaviour
{
    public IEnumerator GetDownloadLink(string author, string repo, Action<string> downloadURL, Action<string> size)
	{
		UnityWebRequest relReq = UnityWebRequest.Get($"https://api.github.com/repos/{author}/{repo}/releases");
		yield return relReq.SendWebRequest();

		if (relReq.isNetworkError || relReq.isHttpError)
		{
			if (relReq.isNetworkError)
				Debug.LogWarning($"No internet connection! Detailed error: {relReq.error}");
			else
				Debug.LogError($"That repository does not exist! Detailed error: {relReq.error}");
			downloadURL(null);
			size(null);
			yield break;
		}
		else
		{
			JSONNode releaseInfo = JSON.Parse(relReq.downloadHandler.text);

			foreach (JSONNode thing in releaseInfo[0]["assets"])
			{
				if (string.Equals(Path.GetExtension(thing["browser_download_url"]), ".nal", StringComparison.OrdinalIgnoreCase))
				{
					downloadURL(thing["browser_download_url"]);
					size(thing["size"]);
					yield break;
				}
			}
			Debug.LogError("No .nal download file found!");
		}
	}
}
