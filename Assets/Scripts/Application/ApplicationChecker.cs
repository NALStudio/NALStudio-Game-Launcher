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
using TMPro;
using UnityEngine.UI;
using Lean.Localization;
using System.Linq;

public class ApplicationChecker : MonoBehaviour
{
	[Serializable]
	public class WarningInfo
	{
		public BannerState triggerState;
		public Sprite sprite;
		public Color spriteColor;
		public string localizationKey;
		public OnBannerClick onClick;
	}

	[Flags] // Flags go in powers of two.
	public enum BannerState { None = 0, Update = 1, NoInternet = 2 }
	public enum OnBannerClick { None = 0, OpenRelease = 1, OpenSettings = 2 }

	#region Variables
	public GameObject banner;
	public TextMeshProUGUI text;
	public Image image;
	public WarningInfo[] warningInfos;
	WarningInfo currentInfo = null;
	BannerState bannerState = BannerState.None;
	#endregion

	public void BannerButtonClick()
	{
		switch (currentInfo.onClick)
		{
			case OnBannerClick.OpenRelease:
				Application.OpenURL("https://github.com/NALStudio/NALStudio-Game-Launcher/releases/latest");
				break;
			case OnBannerClick.OpenSettings:
				System.Diagnostics.Process.Start("ms-settings:network-status");
				break;
		}
	}

	void Awake()
	{
		StartCoroutine(CheckForUpdate());
		NetworkManager.InternetAvailabilityChange += (available) =>
		{
			if (available)
			{
				if ((bannerState & BannerState.NoInternet) != 0)
					bannerState &= BannerState.NoInternet;
			}
			else if ((bannerState & BannerState.NoInternet) == 0)
			{
				bannerState |= BannerState.NoInternet;
			}
			UpdateBanner();
		};

		UpdateBanner();
	}

	void UpdateBanner()
	{
		WarningInfo info = null;

		for (int i = 0; i < warningInfos.Length; i++)
		{
			WarningInfo w = warningInfos[i];
			if (bannerState.HasFlag(w.triggerState))
			{
				info = w;
				break;
			}
		}

		currentInfo = info;

		if (currentInfo == null)
		{
			banner.SetActive(false);
			return;
		}

		text.text = LeanLocalization.GetTranslationText(currentInfo.localizationKey);
		image.sprite = currentInfo.sprite;
		image.color = currentInfo.spriteColor;
		banner.SetActive(true);
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
				if ((bannerState & BannerState.Update) == 0)
					bannerState |= BannerState.Update;
				UpdateBanner();
			}
		}
	}
}
