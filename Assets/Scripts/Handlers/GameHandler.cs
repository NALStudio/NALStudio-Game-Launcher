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

		IEnumerator Uninstaller(string path)
		{
			try
			{
				File.Delete(Path.Combine(path, gamedataFilePath));
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to remove gamedata file in advance. Exception message: {e.Message}");
			}
			yield return StartCoroutine(NALDirectory.Delete(path, true));
		}

		public void Uninstall(UniversalData data)
		{
			StartCoroutine(UninstallCoroutine(data));
			if (data.MsixBundle)
				System.Diagnostics.Process.Start("ms-settings:appsfeatures");
		}

		IEnumerator UninstallCoroutine(UniversalData data, Action onComplete = null)
		{
			if (!uninstalling.Contains(data.UUID))
			{
				uninstalling.Add(data.UUID);
				string uninstallPath = data.Local.LocalsPath;
				if (SettingsManager.Settings.CustomGamePaths.ContainsKey(data.UUID))
				{
					uninstallPath = SettingsManager.Settings.CustomGamePaths[data.UUID];
					SettingsManager.Settings.CustomGamePaths.Remove(data.UUID);
					SettingsManager.Save();
				}

				string uninstallVersion = data.Local.Version;
				double tmpPlaytime = data.Playtime;
				AnalyticsEvent.Custom("game_uninstalled", new Dictionary<string, object>
				{
					{ "name", data.Name },
					{ "version", uninstallVersion },
					{ "playtime", tmpPlaytime }
				});
				AnalyticsEvent.Custom($"{data.UUID}_uninstalled", new Dictionary<string, object>
				{
					{ "version", uninstallVersion },
					{ "playtime", tmpPlaytime }
				});

				yield return StartCoroutine(Uninstaller(uninstallPath));
				data.Local = null;
				//Will clear invalid game folders as well...
				StartCoroutine(LoadGames());
				uninstalling.Remove(data.UUID);
			}
			else
			{
				Debug.LogError($"\"{data.UUID}\" ({data.Name}) is already in the uninstalling queue!");
				yield return new WaitWhile(() => uninstalling.Contains(data.UUID));
			}
			onComplete?.Invoke();
			Debug.Log($"Uninstalled game: {data.Name}");
		}

		public IEnumerator UpdateUninstall(UniversalData data, Action<bool> onComplete = null)
		{
			string uninstallPath = data.Local?.LocalsPath;
			if (SettingsManager.Settings.CustomGamePaths.ContainsKey(data.UUID))
			{
				uninstallPath = SettingsManager.Settings.CustomGamePaths[data.UUID];
				SettingsManager.Settings.CustomGamePaths.Remove(data.UUID);
				SettingsManager.Save();
			}

			if (!string.IsNullOrEmpty(uninstallPath) && Directory.Exists(uninstallPath))
			{
				if (!uninstalling.Contains(data.UUID))
				{
					uninstalling.Add(data.UUID);
					yield return StartCoroutine(Uninstaller(uninstallPath));
					data.Local = null;
					//Will clear invalid game folders as well...
					yield return StartCoroutine(LoadGames());
					uninstalling.Remove(data.UUID);
				}
				else
				{
					Debug.LogError($"\"{data.UUID}\" ({data.Name}) is already in the uninstalling queue!");
					yield return new WaitWhile(() => uninstalling.Contains(data.UUID));
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
				insGame.LoadAssets(gameDatas[i]);
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

			foreach (string path in Directory.EnumerateDirectories(Constants.Constants.GamesPath))
			{
				if (NALPath.Match(path, Constants.Constants.DownloadPath))
					continue; // Skip deletion if download path.

				if (!gameDatas.Any(g => NALPath.Match(g.Local.LocalsPath, path))) // If no one owns directory. Delete.
				{
					yield return StartCoroutine(NALDirectory.Delete(path, true));
				}
				else if (Directory.GetFiles(path).Length < 1)
				{
					string[] subDirs = Directory.GetDirectories(path);
					if (subDirs.Length == 1 && subDirs[0].Contains(Constants.Constants.launcherDataFilePath))
					{
						yield return StartCoroutine(NALDirectory.Delete(path, true));
						foreach (UniversalData d in DataHandler.UniversalDatas.Get().Where(d => d.Local != null && NALPath.Match(d.Local.LocalsPath, path)))
							d.Local = null;
						gameDatas = DataHandler.UniversalDatas.Get().Where(d => d.Local != null).ToArray();
					}
				}
			}
			gameDatasLoaded = true;

			foreach (string path in SettingsManager.Settings.CustomGamePaths.Values.ToArray())
			{
				if (!gameDatas.Any(g => g.Local.LocalsPath == path))
				{
					foreach (string key in SettingsManager.Settings.CustomGamePaths.Keys.Where(k => SettingsManager.Settings.CustomGamePaths[k] == path).ToArray())
						SettingsManager.Settings.CustomGamePaths.Remove(key);
				}
			}

			AddGames();
		}

		public void StartGame(UniversalData data)
		{
			if (gameRunningProcess != null)
				return;

			System.Diagnostics.ProcessStartInfo startInfo;
			if (!data.MsixBundle)
			{
				startInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = Path.Combine(data.Local.LocalsPath, data.Local.ExecutablePath),
					WorkingDirectory = data.Local.LocalsPath
				};
			}
			else
			{
				startInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = data.Local.ExecutablePath
				};
			}

			#region Set Start Time
			data.Local.LastInterest = DateTimeOffset.Now.ToUnixTimeSeconds();
			data.Local.Save();
			#endregion

			DiscordHandler.SetActivity(data, data.Local.LastInterest);

			gameRunningProcess = new System.Diagnostics.Process
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};
			gameRunningProcess.Exited += GameCloseHandler;
			gameRunningData = data;
			gameRunningStartTime = DateTime.UtcNow;
			try
			{
				gameRunningProcess.Start();
				gameRunning = true;
			}
			catch (Exception ex)
			{
				gameRunningProcess = null;
				Debug.LogError($"Game could not be started. Exception: {ex}");
			}


			if (sortingMode == SortingMode.recent)
				StartCoroutine(LoadGames());
		}

		void Update()
		{
			if (playtimesToSave.Count > 0)
			{
				KeyValuePair<string, double> toAdd = playtimesToSave.First();
				playtimesToSave.Remove(toAdd.Key);
				DataHandler.UniversalDatas.Get(toAdd.Key).Playtime += toAdd.Value;
			}

			CheckForStartRequest();
		}

		void GameCloseHandler(object sender, EventArgs e)
		{
			try
			{
				TimeSpan playTime = DateTime.UtcNow - gameRunningStartTime;
				playtimesToSave.Add(gameRunningData.UUID, playTime.TotalMinutes);
			}
			catch (Exception ex) { Debug.LogError(ex.Message); }
			gameRunningData = null;
			gameRunningProcess = null;
			gameRunning = false;

			try
			{
				DiscordHandler.ResetActivity();
			}
			catch (Exception ex) { Debug.LogError(ex.Message); }
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