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
using NALStudio.IO;
using NALStudio.UI;
using System;
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
		public UniversalData data;
		public RawImage thumbnail;
		public TextMeshProUGUI nameText;
		[Header("More Page")]
		public GameObject morePage;
		public Toggle morePageToggle;
		public TextMeshProUGUI playtimeText;
		public TextMeshProUGUI sizeText;
		public TextMeshProUGUI versionText;
		public UITweener[] closeTweeners;
		[Header("Update Tooltip")]
		public GameObject updateAvailable;
		public NALTooltipTrigger tooltipTrigger;
		[Header("Shortcut")]
		public Button ShortcutButton;
		public TextMeshProUGUI ShortcutText;
		public Color ShortcutTextNormal;
		public Color ShortcutTextDisabled;

		[HideInInspector]
		public GameHandler gameHandler;
		[HideInInspector]
		public StorePage storePage;

		void Update()
		{
			if (data.ThumbnailTexture != thumbnail.texture && data.ThumbnailTexture != null)
			{
				thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio =
					data.ThumbnailTexture.width / (float)data.ThumbnailTexture.height;
				thumbnail.texture = data.ThumbnailTexture;
			}
			if (data == null || data.Remote == null || data.Local == null)
				updateAvailable.SetActive(false);
			else
				updateAvailable.SetActive(data.Remote.Version != data.Local.Version);
		}

		public void BrowseLocals()
		{
			if (data?.Local?.LocalsPath != null)
				Application.OpenURL(data.Local.LocalsPath);
		}

		public void OpenStorePage()
		{
			if (data != null)
				storePage.Open(data);
			else
				Debug.LogError("No data specified!");
		}

		public void Uninstall()
		{
			gameHandler.Uninstall(data);
		}

		public void CreateShortcut()
		{
			string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			string shortcutPath = Path.Combine(deskDir, $"{data.Name}.url");
			if (File.Exists(shortcutPath))
			{
				for (int i = 1; File.Exists(shortcutPath); i++)
					shortcutPath = Path.Combine(deskDir, $"{data.Name} ({i}).url");
			}

			string iconPath = Path.Combine(data.Local.LocalsPath, data.Local.ExecutablePath);

			string[] shortcutLines = new string[]
			{
				"[InternetShortcut]",
				$"URL=nalstudiogamelauncher://rungameid/{data.UUID}",
				"IconIndex=0",
				$"IconFile={iconPath}"
			};
			File.WriteAllLines(shortcutPath, shortcutLines);
		}

		public void LoadAssets(UniversalData _data)
		{
			data = _data;
			nameText.text = data.DisplayName;
			versionText.text = data.Local.Version;
			// Update so that it sets the thumbnail
			// immediately if found.
			Update();
		}

		public void StartGame()
		{
			gameHandler.StartGame(data);
		}

		void MorePageAnimationEnd()
		{
			morePage.SetActive(false);
		}

		void MorePageHandler(bool open)
		{
			closeTweeners[0].OnComplete -= MorePageAnimationEnd;
			morePageToggle.isOn = open;

			if (!open)
			{
				foreach (UITweener tweener in closeTweeners)
					tweener.DoTween(true);
				closeTweeners[0].OnComplete += MorePageAnimationEnd;
				return;
			}

			#region Playtime
			playtimeText.text = Math.Convert.MinutesToReadable(data.Playtime);
			#endregion
			#region Shortcut Button
			bool shortcuts = ApplicationHandler.ShortcutsEnabled;
			ShortcutButton.interactable = shortcuts;
			ShortcutText.color = shortcuts ? ShortcutTextNormal : ShortcutTextDisabled;
			#endregion
			#region Game Size
			sizeText.text = LeanLocalization.GetTranslationText("global-calculating");
			StartCoroutine(NALDirectory.GetSize(data.Local.LocalsPath,
				(size) => sizeText.text = $"{Math.Convert.BytesToMB(size):0.0}{LeanLocalization.GetTranslationText("units-megabyte_short", "MB")}"));
			#endregion
			morePage.SetActive(false); // Fixes animation not playing sometimes
			morePage.SetActive(true);
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
