using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AccountHandler : MonoBehaviour
{
	/*
	Requires a redirect??


	public void AuthenticateDiscord()
	{
		
	}

	IEnumerator DiscordCall(string URL, Dictionary<string, string> headers, Action<string> response)
	{
		var request = UnityWebRequest.Post(URL, );
		foreach (KeyValuePair<string, string> header in headers)
		{
			request.SetRequestHeader(header.Key, header.Value);
		}
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.ConnectionError
			|| request.result == UnityWebRequest.Result.DataProcessingError
			|| request.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.LogError($"Authentication failed: {request.error}");
			yield break;
		}

		yield return new WaitWhile(() => request.downloadProgress < 1f);
		response?.Invoke(request.downloadHandler.text);
	}
	*/
}
