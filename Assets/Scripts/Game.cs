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

using Lean.Localization;
using NALStudio.GameLauncher.Cards;
using NALStudio.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NALStudio.GameLauncher.Games
{
	public class Game : MonoBehaviour
	{
		public GameHandler.GameData gameData;
		[HideInInspector]
		public CardHandler.CardData cardData;
		public RawImage thumbnail;
		[Space(10f)]
		public GameObject morePage;
		public UITweener morePageTweener;
		public TextMeshProUGUI nameText;
		public TextMeshProUGUI playtimeText;
		public TextMeshProUGUI versionText;

		[HideInInspector]
		public GameHandler gameHandler;

		void Update()
		{
			if (cardData.thumbnailTexture != thumbnail.texture && cardData.thumbnailTexture != null)
			{
				thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio =
					cardData.thumbnailTexture.width / (float)cardData.thumbnailTexture.height;
				thumbnail.texture = cardData.thumbnailTexture;
			}

			if (!morePageTweener.gameObject.activeSelf)
				morePage.SetActive(false);
		}

		public void LoadAssets(GameHandler.GameData _gameData)
		{
			gameData = _gameData;
			nameText.text = gameData.name;

			string playtimeFormat = LeanLocalization.GetTranslationText("units-minutes", "Minutes");
			double time = gameData.play_time / 60d;
			if (time >= 60)
			{
				time /= 60d;
				playtimeFormat = LeanLocalization.GetTranslationText("units-hours", "Hours");
			}
			playtimeText.text = $"{time:0.0} {playtimeFormat}";

			versionText.text = gameData.version;
		}

		public void StartGame()
		{
			gameHandler.StartGame(gameData);
		}

		public void MorePage(bool open)
		{
			if (open)
			{
				morePage.SetActive(open);
				morePageTweener.gameObject.SetActive(open);
			}
			else
			{
				morePageTweener.DoTween(true, true);
			}
		}
	}
}
