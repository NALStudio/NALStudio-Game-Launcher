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

using NALStudio.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace NALStudio.GameLauncher.Games
{
	public class GameHandler : MonoBehaviour
	{
		public float gridHeight;
		public float verticalSpacing;
		[Space(10f)]
		public bool gameRunning;
		GameData gameRunningData;
		DateTime gameRunningStartTime;
		Process gameRunningProcess;
		[Space(10f)]
		public GameObject gamePrefab;
		public Cards.CardHandler cardHandler;
		public float cardAnimationBasedelay;
		public float cardAnimationDuration = 0.5f;
		[HideInInspector]
		public const string gamedataFileName = "data.nal";
		[HideInInspector]
		public List<GameData> gameDatas = new List<GameData>();
		[HideInInspector]
		public List<Game> gameScripts = new List<Game>();

		List<GameObject> games = new List<GameObject>();
		List<UITweener> gameTweeners = new List<UITweener>();

		RectTransform rectTransform;
		GridLayoutGroup gridLayout;


		public class GameData
		{
			public string name;
			public string version;
			public ulong play_time;
			public string executable_path;
		}

		void Start()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();

			LoadGames();
		}

#if UNITY_EDITOR
		void Reset()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();
			CalculateCellSize();
		}
#endif

		IEnumerator Uninstaller(string path)
		{
			int tries = 0;
			while (Directory.Exists(path))
			{
				tries++;
				try
				{
					Directory.Delete(path, true);
				}
				catch (IOException e)
				{
					UnityEngine.Debug.LogWarning(e.Message);
				}
				if (tries > 10)
				{
					UnityEngine.Debug.LogError($"Could not delete game at path {path}");
					break;
				}
				yield return null;
			}
		}

		public void Uninstall(GameData game)
		{
			StartCoroutine(Uninstaller(Path.Combine(Constants.Constants.GamesPath, game.name)));
		}

		public void Uninstall(string gameName)
		{
			StartCoroutine(Uninstaller(Path.Combine(Constants.Constants.GamesPath, gameName)));
		}

		void AddGames()
		{
			foreach (GameObject go in games)
				Destroy(go);
			games.Clear();
			gameScripts.Clear();
			gameTweeners.Clear();

			for (int i = 0; i < gameDatas.Count; i++)
			{
				GameObject instantiated = Instantiate(gamePrefab, transform);
				games.Add(instantiated);
				Game insGame = instantiated.GetComponent<Game>();
				insGame.gameHandler = this;
				insGame.LoadAssets(gameDatas[i]);
				gameScripts.Add(insGame);
				UITweener insTweener = instantiated.AddComponent<UITweener>();
				insTweener.duration = cardAnimationDuration;
				insTweener.delay = cardAnimationBasedelay + (i / 10f);
				gameTweeners.Add(insTweener);
			}
			cardHandler.AddToGames();
		}

		public void LoadGames()
		{
			foreach (string path in Directory.EnumerateFiles(Constants.Constants.GamesPath))
				File.Delete(path);

			gameDatas = new List<GameData>();
			foreach (string path in Directory.EnumerateDirectories(Constants.Constants.GamesPath))
			{
				string gamedataPath = Path.Combine(path, gamedataFileName);
				if (File.Exists(gamedataPath))
				{
					string encrypted = File.ReadAllText(gamedataPath);
					string unencrypted = Encryption.EncryptionHelper.DecryptString(encrypted);
					gameDatas.Add(JsonUtility.FromJson<GameData>(unencrypted));
				}
				else
				{
					if (path != Constants.Constants.DownloadPath)
						Directory.Delete(path, true);
				}
			}
			AddGames();
		}

		public void StartGame(GameData gameData)
		{
			string path = Path.Combine(Constants.Constants.GamesPath, gameData.name, gameData.executable_path);
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = path,
				WorkingDirectory = Path.Combine(Constants.Constants.GamesPath, gameData.name)
			};
			gameRunningData = gameData;
			gameRunningStartTime = DateTime.UtcNow;
			gameRunningProcess = Process.Start(startInfo);
			gameRunningProcess.Exited += GameCloseHandler;
		}

		void GameCloseHandler(object sender, EventArgs e)
		{
			TimeSpan playTime = gameRunningStartTime - DateTime.UtcNow;
			if (playTime.Seconds < 0)
				return;
			string gamedataPath = Path.Combine(Constants.Constants.GamesPath, gameRunningData.name, gamedataFileName);
			if (File.Exists(gamedataPath))
			{
				string encrypted = File.ReadAllText(gamedataPath);
				string unencrypted = Encryption.EncryptionHelper.DecryptString(encrypted);
				GameData tmpData = JsonUtility.FromJson<GameData>(unencrypted);
				tmpData.play_time += Convert.ToUInt64(playTime.Seconds);
				string gamedataJson = JsonUtility.ToJson(tmpData);
				string gamedataEncrypted = Encryption.EncryptionHelper.EncryptString(gamedataJson);
				File.WriteAllText(gamedataPath, gamedataEncrypted);
			}

			LoadGames();
		}

		public void StopActiveGame()
		{
			if (gameRunningProcess != null)
			{
				gameRunningProcess.Kill();
				gameRunningProcess = null;
			}
		}

		void CalculateRectHeight()
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x,
				gridHeight + ((gridHeight + verticalSpacing) * Mathf.CeilToInt((transform.childCount - 1) / 3)));
		}

		void CalculateCellSize()
		{
			gridLayout.cellSize = new Vector2((rectTransform.rect.width - 20) / 5, gridHeight);
			gridLayout.spacing = new Vector2(gridLayout.spacing.x, verticalSpacing);
			CalculateRectHeight();
		}

		public void PlayAnimation()
		{
			foreach (UITweener tweener in gameTweeners)
				tweener.DoTween();
		}
	}
}