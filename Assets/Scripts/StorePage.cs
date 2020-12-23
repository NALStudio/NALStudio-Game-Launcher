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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NALStudio.UI;
using NALStudio.GameLauncher.Cards;
using NALStudio.GameLauncher.Constants;
using NALStudio.GameLauncher.Games;
using TMPro;
using NALStudio.Encryption;
using static NALStudio.GameLauncher.Games.GameHandler;
using NALStudio.GameLauncher;
using System;
using NALStudio.Coroutines;

public class StorePage : MonoBehaviour
{
	CardHandler.CardData openedData;
	public UITweener tweener;
	public RawImage image;
	public AspectRatioFitter ratioFitter;
	public GameObject earlyAccessBanner;
	public TextMeshProUGUI title;
	public TextMeshProUGUI developer;
	public TextMeshProUGUI publisher;
	public TextMeshProUGUI price;
	public Button button;
	public enum ButtonMode { Install, Update, Uninstall, Queued, Downloading }
	public ButtonMode buttonMode;
	public TextMeshProUGUI buttonText;
	[Space(10f)]
	public DownloadHandler downloadHandler;
	public ToggleButton downloadsButton;
	public GameHandler gameHandler;

	GameData openedGamedata;

	public void Open(CardHandler.CardData cardData)
	{
		gameObject.SetActive(true);
		openedData = cardData;
		if (cardData.early_access)
			earlyAccessBanner.SetActive(true);
		else
			earlyAccessBanner.SetActive(false);
		ratioFitter.aspectRatio = (float)cardData.thumbnailTexture.width / cardData.thumbnailTexture.height;
		image.texture = cardData.thumbnailTexture;
		title.text = cardData.title;
		developer.text = cardData.developer;
		publisher.text = cardData.publisher;
		string priceText = $"€{cardData.price}";
		if (cardData.price == 0)
			priceText = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
		price.text = priceText;

		buttonMode = ButtonMode.Install;
		string gameDataPath = Path.Combine(Constants.GamesPath, cardData.title, GameHandler.gamedataFileName);
		if (File.Exists(gameDataPath))
		{
			string encrypted = File.ReadAllText(gameDataPath);
			string decrypted = EncryptionHelper.DecryptString(encrypted);
			openedGamedata = JsonUtility.FromJson<GameData>(decrypted);

			if (cardData.version != openedGamedata.version)
				buttonMode = ButtonMode.Update;
			else
				buttonMode = ButtonMode.Uninstall;
		}
		if (downloadHandler.Queue.Contains(cardData))
			buttonMode = ButtonMode.Queued;
		else if (downloadHandler.currentlyDownloading == cardData)
			buttonMode = ButtonMode.Downloading;

		switch (buttonMode)
		{
			case ButtonMode.Install:
				buttonText.text = Lean.Localization.LeanLocalization.GetTranslationText("store_page-install", "INSTALL");
				break;
			case ButtonMode.Update:
				buttonText.text = Lean.Localization.LeanLocalization.GetTranslationText("store_page-update", "UPDATE");
				break;
			case ButtonMode.Uninstall:
				buttonText.text = Lean.Localization.LeanLocalization.GetTranslationText("store_page-uninstall", "UNINSTALL");
				break;
			case ButtonMode.Queued:
				buttonText.text = Lean.Localization.LeanLocalization.GetTranslationText("store_page-queued", "Queued...");
				break;
			case ButtonMode.Downloading:
				buttonText.text = Lean.Localization.LeanLocalization.GetTranslationText("store_page-downloading", "Downloading...");
				break;
		}

		tweener.DoTween();
	}

	public void ButtonClick()
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
				gameHandler.Uninstall(openedGamedata);
				openDownloads = false;
				break;
		}
		Close(openDownloads);
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
