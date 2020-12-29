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
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace NALStudio.GameLauncher.Games
{
	public class GameHandler : MonoBehaviour
	{
		public enum SortingMode { recent, alphabetical }

		public float gridHeight;
		public float verticalSpacing;
		[Space(10f)]
		GameData gameRunningData;
		DateTime gameRunningStartTime;
		System.Diagnostics.Process gameRunningProcess;
		[Space(10f)]
		public GameObject gamePrefab;
		public Cards.CardHandler cardHandler;
		public StorePage storePage;
		public float cardAnimationBasedelay;
		public float cardAnimationDuration = 0.5f;
		[Space(10f)]
		public SortingMode sortingMode;
		public TMPro.TMP_Dropdown sortDropdown;

		public const string launcherDataFilePath = "launcher-data";
		public const string gamedataFilePath = "launcher-data/data.nal";
		public const string gameLaunchFilePath = "launcher-data/launch.exe";
		[HideInInspector]
		public List<GameData> gameDatas = new List<GameData>();
		[HideInInspector]
		public List<Game> gameScripts = new List<Game>();	
		[HideInInspector]
		public List<string> uninstalling = new List<string>();

		List<GameObject> games = new List<GameObject>();
		List<UITweener> gameTweeners = new List<UITweener>();

		RectTransform rectTransform;
		GridLayoutGroup gridLayout;

		Dictionary<string, float> playtimesToSave = new Dictionary<string, float>();

		public class GameData
		{
			public string name;
			public string version;
			public string executable_path;
			public long last_interest;
		}

		void Start()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();

			sortingMode = (SortingMode)PlayerPrefs.GetInt("sorting/games", 0);
			sortDropdown.value = (int)sortingMode;

			LoadGames();

			StartCoroutine(CheckForStartRequest());
		}

		public void SortGames(int index)
		{
			if ((SortingMode)index != sortingMode)
			{
				PlayerPrefs.SetInt("sorting/games", index);
				sortingMode = (SortingMode)index;
				LoadGames();
			}
		}

#if UNITY_EDITOR
		void Reset()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();
			CalculateCellSize();
		}
#endif

		IEnumerator Uninstaller(string path, Action onComplete = null)
		{
			int tries = 0;
			string dPath = Path.Combine(path, gamedataFilePath);
			while (File.Exists(dPath))
			{
				try
				{
					tries++;
					File.Delete(dPath);
					if (tries > 10)
						break;
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Could not delete file {dPath}!\n{e.Message}");
				}
			}
			yield return null;
			while (Directory.Exists(path))
			{
				if (Directory.Exists(path))
				{
					Directory.Delete(path, true);
					yield return null;
					LoadGames();
				}
			}
			onComplete?.Invoke();
		}

		public IEnumerator Uninstall(GameData game, Action onComplete = null)
		{
			if (!uninstalling.Contains(game.name))
			{
				uninstalling.Add(game.name);
				bool uninstalled = false;
				StartCoroutine(Uninstaller(Path.Combine(Constants.Constants.GamesPath, game.name), () => uninstalled = true));
				yield return new WaitWhile(() => !uninstalled);
				uninstalling.Remove(game.name);
			}
			else
			{
				Debug.LogError($"{game.name} is already in the uninstalling queue!");
			}
			onComplete?.Invoke();
			AnalyticsEvent.Custom("game_uninstalled", new Dictionary<string, object>
			{
				{ "name", game.name },
				{ "version", game.version },
				{ "playtime", PlayerPrefs.GetFloat($"playtime/{game.name}", 0f) }
			});
		}

		public IEnumerator UpdateUninstall(Cards.CardHandler.CardData card, Action onComplete = null)
		{
			if (!uninstalling.Contains(card.title))
			{
				uninstalling.Add(card.title);
				bool uninstalled = false;
				StartCoroutine(Uninstaller(Path.Combine(Constants.Constants.GamesPath, card.title), () => uninstalled = true));
				yield return new WaitWhile(() => !uninstalled);
				uninstalling.Remove(card.title);
			}
			else
			{
				Debug.LogError($"{card.title} is already in the uninstalling queue!");
			}
			onComplete?.Invoke();
			AnalyticsEvent.Custom("game_update", new Dictionary<string, object>
			{
				{ "name", card.title },
				{ "version", card.version },
				{ "playtime", PlayerPrefs.GetFloat($"playtime/{card.title}", 0f) }
			});
		}

		void AddGames()
		{
			foreach (GameObject go in games)
				Destroy(go);
			games.Clear();
			gameScripts.Clear();
			gameTweeners.Clear();

			switch (sortingMode)
			{
				case SortingMode.recent:
					gameDatas = gameDatas.OrderBy(g => g.last_interest).ToList();
					gameDatas.Reverse();
					break;
			}

			for (int i = 0; i < gameDatas.Count; i++)
			{
				GameObject instantiated = Instantiate(gamePrefab, transform);
				games.Add(instantiated);
				Game insGame = instantiated.GetComponent<Game>();
				insGame.gameHandler = this;
				insGame.storePage = storePage;
				StartCoroutine(insGame.LoadAssets(gameDatas[i]));
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
				string gamedataPath = Path.Combine(path, gamedataFilePath);
				if (File.Exists(gamedataPath))
				{
					string encrypted = File.ReadAllText(gamedataPath);
					string unencrypted = Encryption.EncryptionHelper.DecryptString(encrypted);
					gameDatas.Add(JsonUtility.FromJson<GameData>(unencrypted));
				}
				else if (path != Constants.Constants.DownloadPath)
				{
					Directory.Delete(path, true);
				}
			}
			AddGames();
		}

		public void StartGame(GameData gameData)
		{
			if (gameRunningProcess != null)
				return;

			string path = Path.Combine(Constants.Constants.GamesPath, gameData.name, gameData.executable_path);

			#region Set Start Time
			string gdPath = Path.Combine(Constants.Constants.GamesPath, gameData.name, gamedataFilePath);
			string encrypted = File.ReadAllText(gdPath);
			string unencrypted = Encryption.EncryptionHelper.DecryptString(encrypted);
			GameData tmp = JsonUtility.FromJson<GameData>(unencrypted);
			tmp.last_interest = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds();
			unencrypted = JsonUtility.ToJson(tmp, true);
			encrypted = Encryption.EncryptionHelper.EncryptString(unencrypted);
			File.WriteAllText(gdPath, encrypted);
			#endregion

			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
			{
				FileName = path,
				WorkingDirectory = Path.Combine(Constants.Constants.GamesPath, gameData.name)
			};
			gameRunningProcess = new System.Diagnostics.Process
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};
			gameRunningProcess.Exited += GameCloseHandler;
			gameRunningData = gameData;
			gameRunningStartTime = DateTime.UtcNow;
			gameRunningProcess.Start();

			if (sortingMode == SortingMode.recent)
				LoadGames();
		}

		void Update()
		{
			foreach (KeyValuePair<string, float> kv in playtimesToSave)
			{
				float toAdd = PlayerPrefs.GetFloat($"playtime/{kv.Key}", 0f);
				float added = toAdd + kv.Value;
				PlayerPrefs.SetFloat($"playtime/{kv.Key}", added);
			}
			playtimesToSave.Clear();
		}

		void GameCloseHandler(object sender, EventArgs e)
		{
			try
			{
				TimeSpan playTime = DateTime.UtcNow - gameRunningStartTime;
				if (playTime.TotalMinutes > 0)
					playtimesToSave.Add(gameRunningData.name, (float)playTime.TotalMinutes);
				gameRunningData = null;
				gameRunningProcess = null;
			}
			catch (Exception ex)
			{
				gameRunningData = null;
				gameRunningProcess = null;
				Debug.LogError(ex.Message);
			}
		}

		public System.Diagnostics.Process GetActiveProcess() { return gameRunningProcess; }
		public GameData GetActiveData() { return gameRunningData; }

		public void StopActiveGame()
		{
			gameRunningProcess?.Kill();
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

		void OnRectTransformDimensionsChange()
		{
			if (gridLayout == null)
				return;
			CalculateCellSize();
		}

		public void PlayAnimation()
		{
			foreach (UITweener tweener in gameTweeners)
				tweener.DoTween();
		}

		IEnumerator CheckForStartRequest()
		{
			while (true)
			{
				yield return new WaitForSeconds(5f);
				if (gameRunningProcess == null)
				{
					if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
					{
						string launchApp = Environment.GetEnvironmentVariable("NALStudioGameLauncherLaunchApplication", EnvironmentVariableTarget.User);
						if (launchApp != null)
						{
							GameData correctData = null;
							foreach (GameData gd in gameDatas)
							{
								if (gd.name == launchApp)
								{
									correctData = gd;
								}
							}
							if (correctData != null)
								StartGame(correctData);
							else
								Debug.LogError($"Game name not found in gameDatas! GameDatas count: {gameDatas.Count}, GameName: {launchApp}");

							Environment.SetEnvironmentVariable("NALStudioGameLauncherLaunchApplication", null, EnvironmentVariableTarget.User);
						}
					}
					else
					{
						throw new PlatformNotSupportedException("Shortcuts currently support only Windows NT or newer!");
					}
				}
			}
		}
	}
}