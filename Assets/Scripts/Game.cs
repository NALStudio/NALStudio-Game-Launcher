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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
		public Toggle morePageToggle;
		public TextMeshProUGUI nameText;
		public TextMeshProUGUI playtimeText;
		public TextMeshProUGUI versionText;

		[HideInInspector]
		public GameHandler gameHandler;
		[HideInInspector]
		public StorePage storePage;

		void Update()
		{
			if (cardData.thumbnailTexture != thumbnail.texture && cardData.thumbnailTexture != null)
			{
				thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio =
					cardData.thumbnailTexture.width / (float)cardData.thumbnailTexture.height;
				thumbnail.texture = cardData.thumbnailTexture;
			}
		}

		public void OpenStorePage()
		{
			if (cardData != null)
				storePage.Open(cardData);
			else
				Debug.LogError("No CardData specified!");
		}

		public void Uninstall()
		{
			gameHandler.Uninstall(gameData);
		}

		public void CreateShortcut()
		{
			string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			string shortcutPath = Path.Combine(deskDir, $"{gameData.name}.url");
			if (File.Exists(shortcutPath))
			{
				for (int i = 1; File.Exists(shortcutPath); i++)
					shortcutPath = Path.Combine(deskDir, $"{gameData.name} ({i}).url");
			}

			string fileUrl = Path.Combine(Constants.Constants.GamesPath, gameData.name, GameHandler.gameLaunchFilePath);
			File.Copy(Path.Combine(Application.streamingAssetsPath, "NALStudioGameLauncherLaunch.exe"), fileUrl, true);

			string iconPath = Path.Combine(Constants.Constants.GamesPath, gameData.name, gameData.executable_path);

			string[] shortcutLines = new string[]
			{
				"[InternetShortcut]",
				"URL=file:///" + $"{fileUrl}",
				"IconIndex=0",
				$"IconFile={iconPath}"
			};
			File.WriteAllLines(shortcutPath, shortcutLines);
		}

		public void LoadAssets(GameHandler.GameData _gameData)
		{
			gameData = _gameData;
			nameText.text = gameData.name;
			versionText.text = gameData.version;
		}

		public void StartGame()
		{
			gameHandler.StartGame(gameData);
		}

		void MorePageHandler(bool open)
		{
			if (open)
			{
				string playtimeFormat = LeanLocalization.GetTranslationText("units-minutes", "Minutes");
				float time = PlayerPrefs.GetFloat($"playtime/{gameData.name}", 0);
				if (time >= 60)
				{
					time /= 60f;
					playtimeFormat = LeanLocalization.GetTranslationText("units-hours", "Hours");
				}
				playtimeText.text = $"{time:0.0} {playtimeFormat}";
			}
			morePageToggle.isOn = open;
			morePage.SetActive(open);
		}

		public void MorePage(bool open)
		{
			if (gameHandler != null)
			{
				foreach (Game g in gameHandler.gameScripts)
					g.MorePage(false, this);
			}
			MorePageHandler(open);
		}

		public void MorePage(bool open, Game sender)
		{
			if (!ReferenceEquals(sender, this))
				MorePageHandler(open);
		}
	}
}
