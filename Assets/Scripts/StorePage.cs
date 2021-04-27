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

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NALStudio.UI;
using NALStudio.GameLauncher.Cards;
using NALStudio.GameLauncher.Constants;
using NALStudio.GameLauncher.Games;
using TMPro;
using NALStudio.GameLauncher;
using System.Collections;
using Lean.Localization;
using System.Collections.Generic;

public class StorePage : MonoBehaviour
{
	[System.Serializable]
	public class AgeRatingDataHolder
	{
		public UniversalData.AgeRating AgeRating;
		public Sprite Image;
	}

	UniversalData openedData;
	public UITweener tweener;
	public RawImage image;
	public Texture2D thumbnailNullImage;
	public AspectRatioFitter ratioFitter;
	public GameObject earlyAccessBanner;
	public TextMeshProUGUI title;
	public RectTransform descriptionObject;
	public float defaultDescriptionObjectHeight;
	public float extendedDescriptionObjectHeight;
	public TextMeshProUGUI description;
	public TextMeshProUGUI developer;
	public TextMeshProUGUI publisher;
	public TextMeshProUGUI releaseDate;
	public TextMeshProUGUI version;
	public TextMeshProUGUI price;
	public enum ButtonMode { Install, Update, Uninstall, Queued, Downloading }
	public ButtonMode buttonMode;
	public TextMeshProUGUI buttonText;
	public Image AgeRatingImage;
	public AgeRatingDataHolder[] AgeRatingDatas;
	[Space(10f)]
	public DownloadHandler downloadHandler;
	public ToggleButton downloadsButton;
	public GameHandler gameHandler;
	public InstallDirHandler dirHandler;

	public void Open(UniversalData _data)
	{
		gameObject.SetActive(true);
		openedData = _data;
		earlyAccessBanner.SetActive(openedData.EarlyAccess);
		ratioFitter.aspectRatio = (float)openedData.ThumbnailTexture.width / openedData.ThumbnailTexture.height;
		image.texture = openedData.ThumbnailTexture;
		image.color = Color.white;
		title.text = openedData.DisplayName;

		float descriptionHeight = openedData.EarlyAccess ? defaultDescriptionObjectHeight : extendedDescriptionObjectHeight;
		descriptionObject.sizeDelta = new Vector2(descriptionObject.sizeDelta.x, descriptionHeight);
		descriptionObject.gameObject.SetActive(openedData.Description != null);
		description.text = openedData.Description;

		developer.text = openedData.Developer;
		publisher.text = openedData.Publisher;
		releaseDate.text = openedData.ReleaseDate;
		version.text = openedData.Remote.Version;
		price.text = openedData.Price > 0 ? $"€{openedData.Price}" : LeanLocalization.GetTranslationText("pricing-free", "Free");

		for (int i = 0; i < AgeRatingDatas.Length; i++)
		{
			AgeRatingDataHolder a = AgeRatingDatas[i];
			if (a.AgeRating == _data.Age)
			{
				AgeRatingImage.color = Color.white;
				AgeRatingImage.sprite = a.Image;
				break;
			}
		}

		buttonMode = ButtonMode.Install;
		if (openedData.Local != null)
		{
			if (openedData.Remote.Version != openedData.Local.Version)
				buttonMode = ButtonMode.Update;
			else
				buttonMode = ButtonMode.Uninstall;
		}
		if (downloadHandler.Queue.Contains(openedData))
			buttonMode = ButtonMode.Queued;
		else if (downloadHandler.currentlyDownloading?.Equals(openedData) == true)
			buttonMode = ButtonMode.Downloading;

		switch (buttonMode)
		{
			case ButtonMode.Install:
				buttonText.text = LeanLocalization.GetTranslationText("store_page-install", "INSTALL");
				break;
			case ButtonMode.Update:
				buttonText.text = LeanLocalization.GetTranslationText("store_page-update", "UPDATE");
				break;
			case ButtonMode.Uninstall:
				buttonText.text = LeanLocalization.GetTranslationText("store_page-uninstall", "UNINSTALL");
				break;
			case ButtonMode.Queued:
				buttonText.text = LeanLocalization.GetTranslationText("store_page-queued", "Queued...");
				break;
			case ButtonMode.Downloading:
				buttonText.text = LeanLocalization.GetTranslationText("store_page-downloading", "Downloading...");
				break;
		}

		tweener.DoTween();
	}

	IEnumerator InstallCustomDir(UniversalData openedData)
	{
		bool finished = false;
		bool success = false;
		string path = null;
		StartCoroutine(dirHandler.Prompt(openedData, (bool f, bool s, string p) =>
		{
			finished = f;
			success = s;
			path = p;
		}));
		yield return new WaitUntil(() => finished);
		if (success)
			downloadHandler.Queue.Add(openedData, Path.Combine(path, openedData.Name));

		Close(success);
	}

	public void ButtonClick(bool rightClick = false)
	{
		if (!rightClick)
		{
			bool openDownloads = true;
			switch (buttonMode)
			{
				case ButtonMode.Install:
				case ButtonMode.Update:
					downloadHandler.Queue.Add(openedData);
					break;
				case ButtonMode.Queued:
				case ButtonMode.Downloading:
					downloadHandler.Cancel(openedData);
					break;
				case ButtonMode.Uninstall:
					gameHandler.Uninstall(openedData);
					openDownloads = false;
					break;
			}
			Close(openDownloads);
		}
		else
		{
			switch (buttonMode)
			{
				case ButtonMode.Install:
					StartCoroutine(InstallCustomDir(openedData));
					break;
			}
		}
	}

	public void Close()
	{
		tweener.DoTween(true, true);
	}

	public void Close(bool openDownloads)
	{
		tweener.DoTween(true, true);
		if (openDownloads)
			downloadsButton.IsOn = true;
	}
}
