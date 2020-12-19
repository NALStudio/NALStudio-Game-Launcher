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
using TMPro;

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
	public enum ButtonMode { Install, Update, Uninstall }
	public ButtonMode buttonMode;
	public TextMeshProUGUI buttonText;
	[Space(10f)]
	public NALStudio.GameLauncher.DownloadHandler downloadHandler;

	public class GameData
	{
		public string version;
	}

	public void Open(CardHandler.CardData cardData)
	{
		gameObject.SetActive(true);
		tweener.DoTween();
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
		if (cardData.gameData != null)
		{
			if (cardData.gameData.version != cardData.version)
				buttonMode = ButtonMode.Update;
			else
				buttonMode = ButtonMode.Uninstall;
		}

		switch (buttonMode)
		{
			case ButtonMode.Install:
				buttonText.text = "INSTALL";
				break;
			case ButtonMode.Update:
				buttonText.text = "UPDATE";
				break;
			case ButtonMode.Uninstall:
				buttonText.text = "UNINSTALL";
				break;
		}
	}

	public void ButtonClick()
	{
		downloadHandler.Queue.Add(openedData);
	}

	public void Close()
	{
		tweener.DoTween(true, true);
	}
}
