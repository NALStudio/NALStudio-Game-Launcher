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
using Unity.Collections;
using Unity.Jobs;
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

		struct UninstallJob : IJob
		{
			public NativeArray<char> pathChars;
			public NativeArray<bool> result;

			public void Execute()
			{
				result[0] = false;
				string path = new string(pathChars.ToArray());
				string dPath = Path.Combine(path, gamedataFilePath);
				File.Delete(dPath);
				Directory.Delete(path, true);
				result[0] = true;
			}
		}

		IEnumerator Uninstaller(string path)
		{
			NativeArray<bool> result = new NativeArray<bool>(1, Allocator.Persistent);
			NativeArray<char> pathChars = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
			UninstallJob job = new UninstallJob
			{
				pathChars = pathChars,
				result = result
			};
			JobHandle handle = job.Schedule();
			yield return new WaitUntil(() => handle.IsCompleted);
			handle.Complete();
			bool success = result[0];
			if (!success)
				Debug.LogError($"Directory deletion at path \"{path}\" was unsuccesful!");
			result.Dispose();
			pathChars.Dispose();
		}

		public void Uninstall(UniversalData data)
		{
			StartCoroutine(UninstallCoroutine(data));
		}

		IEnumerator UninstallCoroutine(UniversalData data, Action onComplete = null)
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

				yield return StartCoroutine(Uninstaller(uninstallPath));
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
						yield return StartCoroutine(Uninstaller(uninstallPath));
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