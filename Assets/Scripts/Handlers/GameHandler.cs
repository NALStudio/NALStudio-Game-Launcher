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

using NALStudio.IO;
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

		public bool gameRunning { get; private set; }
		UniversalData gameRunningData;
		DateTime gameRunningStartTime;
		public System.Diagnostics.Process gameRunningProcess { get; private set; }
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
		public UniversalData[] gameDatas;
		bool gameDatasLoaded = false;
		[HideInInspector]
		public List<Game> gameScripts = new List<Game>();
		[HideInInspector]
		public List<string> uninstalling = new List<string>();

		List<GameObject> games = new List<GameObject>();
		List<UITweener> gameTweeners = new List<UITweener>();

		RectTransform rectTransform;
		GridLayoutGroup gridLayout;

		Dictionary<string, double> playtimesToSave = new Dictionary<string, double>();

		void Start()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();

			sortingMode = (SortingMode)PlayerPrefs.GetInt("sorting/games", 0);
			sortDropdown.value = (int)sortingMode;

			StartCoroutine(LoadGames());
		}

		public void SortGames(int index)
		{
			if ((SortingMode)index != sortingMode)
			{
				PlayerPrefs.SetInt("sorting/games", index);
				sortingMode = (SortingMode)index;
				StartCoroutine(LoadGames());
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
					{
						Debug.LogError($"Could not delete file {dPath}! Tries: {tries}");
						break;
					}
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Could not delete file {dPath}!\n{e.Message}");
				}
			}
			yield return null;
			tries = 0;
			while (Directory.Exists(path))
			{
				try
				{
					tries++;
					Directory.Delete(path, true);
					if (tries > 10)
					{
						Debug.LogError($"Could not delete directory {path}! Tries: {tries}");
					}
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Could not delete directory {path}!\n{e.Message}");
				}
			}
			yield return null;
			onComplete?.Invoke();
		}

		public IEnumerator Uninstall(UniversalData data, Action onComplete = null)
		{
			if (!uninstalling.Contains(data.UUID))
			{
				uninstalling.Add(data.UUID);
				string uninstallPath = data.Local.LocalsPath;
				if (SettingsManager.Settings.customGamePaths.ContainsKey(data.UUID))
				{
					uninstallPath = SettingsManager.Settings.customGamePaths[data.UUID];
					SettingsManager.Settings.customGamePaths.Remove(data.UUID);
					SettingsManager.Save();
				}

				string uninstallVersion = data.Local.Version;
				AnalyticsEvent.Custom("game_uninstalled", new Dictionary<string, object>
				{
					{ "name", data.Name },
					{ "version", uninstallVersion },
					{ "playtime", data.Playtime }
				});
				AnalyticsEvent.Custom($"{data.Name}_uninstalled", new Dictionary<string, object>
				{
					{ "version", uninstallVersion },
					{ "playtime", data.Playtime }
				});

				bool uninstalled = false;
				StartCoroutine(Uninstaller(uninstallPath, () => uninstalled = true));
				yield return new WaitUntil(() => uninstalled);
				data.Local = null;
				//Will clear invalid game folders as well...
				StartCoroutine(LoadGames());
				uninstalling.Remove(data.UUID);
			}
			else
			{
				Debug.LogError($"\"{data.UUID}\" ({data.Name}) is already in the uninstalling queue!");
			}
			onComplete?.Invoke();
			Debug.Log($"Uninstalled game: {data.Name}");
		}

		public IEnumerator UpdateUninstall(UniversalData data, Action<bool> onComplete = null)
		{
			string uninstallPath = data.Local?.LocalsPath;
			if (SettingsManager.Settings.customGamePaths.ContainsKey(data.UUID))
			{
				uninstallPath = SettingsManager.Settings.customGamePaths[data.UUID];
				SettingsManager.Settings.customGamePaths.Remove(data.UUID);
				SettingsManager.Save();
			}

			if (!string.IsNullOrEmpty(uninstallPath) && Directory.Exists(uninstallPath))
			{
				bool looped = false;
				while (true)
				{
					if (!uninstalling.Contains(data.UUID))
					{
						uninstalling.Add(data.UUID);
						bool uninstalled = false;
						StartCoroutine(Uninstaller(uninstallPath, () => uninstalled = true));
						yield return new WaitUntil(() => uninstalled);
						data.Local = null;
						//Will clear invalid game folders as well...
						StartCoroutine(LoadGames());
						uninstalling.Remove(data.UUID);
					}
					else
					{
						Debug.LogError($"\"{data.UUID}\" ({data.Name}) is already in the uninstalling queue!");
					}

					string oldUninstallPath = uninstallPath;
					uninstallPath = Path.Combine(Constants.Constants.GamesPath, data.Name);
					if (!looped)
					{
						if (!Directory.Exists(uninstallPath) || looped)
							break;
						looped = true;
					}
					else
					{
						Debug.Log($"Uninstalled base game of \"{data.UUID}\" ({data.Name}). New custom path location: \"{oldUninstallPath}\"");
					}
				}
				onComplete?.Invoke(true);
			}
			else
			{
				onComplete?.Invoke(false);
			}
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
					gameDatas = gameDatas.OrderByDescending(g => g.Local.LastInterest).ToArray();
					break;
				case SortingMode.alphabetical:
					gameDatas = gameDatas.OrderBy(g => g.DisplayName).ToArray();
					break;
			}

			for (int i = 0; i < gameDatas.Length; i++)
			{
				GameObject instantiated = Instantiate(gamePrefab, transform);
				games.Add(instantiated);
				Game insGame = instantiated.GetComponent<Game>();
				insGame.gameHandler = this;
				insGame.storePage = storePage;
				StartCoroutine(insGame.LoadAssets(gameDatas[i]));
				gameScripts.Add(insGame);
				UITweener insTweener = instantiated.GetComponent<UITweener>();
				insTweener.duration = cardAnimationDuration;
				insTweener.delay = cardAnimationBasedelay + (i / 10f);
				gameTweeners.Add(insTweener);
			}
		}

		public IEnumerator LoadGames()
		{
			yield return new WaitUntil(() => DataHandler.UniversalDatas.Loaded);

			foreach (string path in Directory.EnumerateFiles(Constants.Constants.GamesPath))
				File.Delete(path);

			gameDatas = DataHandler.UniversalDatas.Get().Where(d => d.Local != null).ToArray();
			gameDatasLoaded = true;

			foreach (string path in Directory.EnumerateDirectories(Constants.Constants.GamesPath))
			{
				if (!gameDatas.Any(g => NALPath.Match(g.Local.LocalsPath, path)))
				{
					Directory.Delete(path, true);
				}
			}
			foreach (string path in SettingsManager.Settings.customGamePaths.Values.ToArray())
			{
				if (!gameDatas.Any(g => g.Local.LocalsPath == path))
				{
					foreach (string key in SettingsManager.Settings.customGamePaths.Keys.Where(k => SettingsManager.Settings.customGamePaths[k] == path).ToArray())
						SettingsManager.Settings.customGamePaths.Remove(key);
				}
			}
			AddGames();
		}

		public void StartGame(UniversalData data)
		{
			if (gameRunningProcess != null)
				return;

			#region Set Start Time
			data.Local.LastInterest = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds();
			data.Local.Save();
			#endregion

			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
			{
				FileName = Path.Combine(data.Local.LocalsPath, data.Local.ExecutablePath),
				WorkingDirectory = data.Local.LocalsPath
			};
			gameRunningProcess = new System.Diagnostics.Process
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};
			gameRunningProcess.Exited += GameCloseHandler;
			gameRunningData = data;
			gameRunningStartTime = DateTime.UtcNow;
			gameRunningProcess.Start();
			gameRunning = true;

			if (sortingMode == SortingMode.recent)
				StartCoroutine(LoadGames());
		}

		void Update()
		{
			foreach (KeyValuePair<string, double> kv in playtimesToSave)
			{
				DataHandler.UniversalDatas.Get(kv.Key).Playtime += Convert.ToSingle(kv.Value);
			}
			playtimesToSave.Clear();

			CheckForStartRequest();
		}

		void GameCloseHandler(object sender, EventArgs e)
		{
			try
			{
				TimeSpan playTime = DateTime.UtcNow - gameRunningStartTime;
				if (playTime.TotalMinutes > 0)
					playtimesToSave.Add(gameRunningData.UUID, playTime.TotalMinutes);
			}
			catch (Exception ex) { Debug.LogError(ex.Message); }
			gameRunningData = null;
			gameRunningProcess = null;
			gameRunning = false;
		}

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

		void CheckForStartRequest()
		{
			if (gameRunningProcess == null)
			{
				string launchUUID = Environment.GetEnvironmentVariable("NALStudioGameLauncherLaunchApplication", EnvironmentVariableTarget.User);
				if (launchUUID != null && gameDatasLoaded)
				{
					UniversalData correctData = Array.Find(gameDatas, d => d.UUID == launchUUID);
					if (correctData != null)
						StartGame(correctData);
					else
						Debug.LogError($"Game name not found in gameDatas! GameDatas count: {gameDatas.Length}, Game UUID: {launchUUID}");

					Environment.SetEnvironmentVariable("NALStudioGameLauncherLaunchApplication", null, EnvironmentVariableTarget.User);
				}
			}
		}
	}
}