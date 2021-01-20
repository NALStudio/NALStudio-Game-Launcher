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
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.UI.Extensions;
using TMPro;
using NALStudio.GameLauncher.Games;
using Lean.Localization;
using NALStudio.Math;
using NALStudio.Coroutines;
using System;
using UnityEngine.Analytics;

namespace NALStudio.GameLauncher
{
    public class DownloadHandler : MonoBehaviour
    {
		public DownloadsMenu downloadsMenu;
		[Space(10f)]
		public UILineRenderer lineRenderer;
		public TextMeshProUGUI downloadSpeedText;
		public TextMeshProUGUI downloadProgressText;
		public TextMeshProUGUI queuedCountText;
		public TextMeshProUGUI uninstallingCountText;
		public TextMeshProUGUI extractingText;
		public RectTransform downloadSpeedRect;
		public RectTransform LineArea;
		public GameObject downloadingAssets;
		public GameObject extractingAssets;
		bool extracting = false;
		public Color progressBarExtracting;
		[Space(10f)]
		public int maxDataPoints;
		[Range(0, 1)]
		public float lineSmoothing;
		bool updateGraphs;
		ulong oldDownloadedBytes;
		public List<float> dataPoints = new List<float>();
		[Space(10f)]
		public Slider progressBar;
		[Space(10f)]
		public Image progressBarBackground;
		public Image progressBarFill;
		public Color errorProgressColor;
		Color progressNormalColor;
		Color progressFillNormalColor;
		Color extractingNormalColor;
		string extractingNormalText;
		bool downloadError;
		[Space(10f)]
		public GameHandler gameHandler;

        UnityWebRequest request;
		bool downloadInProgress = false;
		[HideInInspector]
		public QueueHandler Queue = new QueueHandler();
		[HideInInspector]
		public DownloadData currentlyDownloading;

		[System.Serializable]
		public class DownloadData
		{
			public string name;
			public string version;
			public string download;
			public string executable_path;
			public string customPath;
			public float Playtime
			{
				get
				{
					return PlayerPrefs.GetFloat($"playtime/{name}", 0f);
				}
			}

			public DownloadData Copy()
			{
				return new DownloadData()
				{
					name = name,
					version = version,
					download = download,
					customPath = customPath
				};
			}

			public override bool Equals(object obj)
			{
				return obj is DownloadData data && name == data.name;
			}

			public override int GetHashCode()
			{
				int hashCode = 1307175314;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(version);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(download);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(executable_path);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(customPath);
				hashCode = hashCode * -1521134295 + Playtime.GetHashCode();
				return hashCode;
			}
		}

		void Awake()
		{
			progressNormalColor = progressBarBackground.color;
			progressFillNormalColor = progressBarFill.color;
			extractingNormalColor = extractingText.color;
			extractingNormalText = extractingText.text;
			queuedCountText.text =
				$"{LeanLocalization.GetTranslationText("downloads-queued", "Queued")}: {Queue.Count()}";
		}

		public class QueueHandler
		{
            List<DownloadData> queued = new List<DownloadData>();

			public void Add(DownloadData downloadData)
			{
				if (!queued.Contains(downloadData))
					queued.Add(downloadData);
			}

            public void Add(Cards.CardHandler.CardData cardData)
			{
				Add(cardData.ToDownloadData());
			}

			public void Add(Cards.CardHandler.CardData cardData, string customPath)
			{
				DownloadData downloadData = cardData.ToDownloadData();
				downloadData.customPath = customPath;
				Add(downloadData);
			}

            public void Remove(Cards.CardHandler.CardData item)
			{
                queued.Remove(item.ToDownloadData());
			}

			public void Remove(DownloadData item)
			{
				queued.Remove(item);
			}

			public bool Contains(Cards.CardHandler.CardData item)
			{
				return queued.Contains(item.ToDownloadData());
			}

			public bool Contains(DownloadData item)
			{
				return queued.Contains(item);
			}

			public int Count()
			{
				return queued.Count;
			}

			public void Clear()
			{
				queued.Clear();
			}

			public DownloadData GetNextDownload()
			{
				if (queued.Count > 0)
				{
					DownloadData tmpData = queued[0];
					queued.RemoveAt(0);
					return tmpData;
				}
				else
				{
					return null;
				}
			}
		}

		void Update()
		{
			if (downloadInProgress)
			{
				if (!updateGraphs)
				{
					updateGraphs = true;
				}
			}
			else
			{
				updateGraphs = false;
				currentlyDownloading = Queue.GetNextDownload();
				if (currentlyDownloading != null)
				{
					downloadInProgress = true;
					StartCoroutine(Downloader(currentlyDownloading));
				}
			}
		}

		void FixedUpdate()
		{
			#region Uninstall Count
			if (gameHandler.uninstalling.Count > 0)
			{
				uninstallingCountText.gameObject.SetActive(true);
				uninstallingCountText.text =
					$"{LeanLocalization.GetTranslationText("downloads-uninstalling", "Uninstalling")}: {gameHandler.uninstalling.Count}";
			}
			else
			{
				uninstallingCountText.gameObject.SetActive(false);
			}
			#endregion

			if (updateGraphs && Application.targetFrameRate != 1 && request != null && !extracting)
			{
				downloadingAssets.SetActive(true);
				progressBar.gameObject.SetActive(true);

				if (downloadsMenu.opened)
				{
					#region Download Error
					if (downloadError)
					{
						downloadSpeedText.text = "[ERROR]";
						progressBarBackground.color = errorProgressColor;
						progressBarFill.color = new Color(0, 0, 0, 0);
					}
					else if (progressBarBackground.color != progressNormalColor)
					{
						progressBarBackground.color = progressNormalColor;
						progressBarFill.color = progressFillNormalColor;
					}
					#endregion

					#region Progress Bar
					progressBar.value = request.downloadProgress;
					#endregion

					#region Line Renderer
					#region Point Loading
					while (dataPoints.Count >= maxDataPoints)
						dataPoints.RemoveAt(0);
					dataPoints.Add((request.downloadedBytes - oldDownloadedBytes) / Time.fixedUnscaledDeltaTime);
					if (dataPoints.Count > 1)
					{
						if (dataPoints[dataPoints.Count - 1] == 0f && Application.internetReachability != NetworkReachability.NotReachable)
							dataPoints[dataPoints.Count - 1] = dataPoints[dataPoints.Count - 2];
						if (lineSmoothing > 0)
							dataPoints[dataPoints.Count - 1] = Mathf.Lerp(dataPoints[dataPoints.Count - 2], dataPoints[dataPoints.Count - 1], 1f - lineSmoothing);
					}
					#endregion
					#region Point Updating
					Vector2[] tmpPoints = new Vector2[maxDataPoints + 2];
					float remapVal = 0;
					if (dataPoints.Count > 0)
						remapVal = dataPoints.Max();
					for (int i = 0; i < tmpPoints.Length; i++)
					{
						if (i < dataPoints.Count)
							tmpPoints[i] = new Vector2((float)i / maxDataPoints, dataPoints[i] / remapVal);
						else
							tmpPoints[i] = tmpPoints[i - 1];
					}
					#region Vertical Line
					tmpPoints[maxDataPoints] = new Vector2(tmpPoints[maxDataPoints - 1].x, 1);
					tmpPoints[maxDataPoints + 1] = new Vector2(tmpPoints[maxDataPoints - 1].x, 0);
					#endregion
					lineRenderer.Points = tmpPoints;
					#endregion
					#endregion

					#region Download Speed
					downloadSpeedText.text = $"{Math.Convert.BitsToMb(Mathf.RoundToInt(dataPoints.Last() * 8)):0.0}{LeanLocalization.GetTranslationText("units-megabits_per_second-short", "Mb/s")}";
					downloadSpeedRect.anchoredPosition = new Vector2((lineRenderer.Points.Last().x * LineArea.rect.width) + lineRenderer.LineThickness + 5, downloadSpeedRect.anchoredPosition.y);
					#endregion

					#region Download Progress
					double downloadSize = request.downloadedBytes / (double)request.downloadProgress;
					if (double.IsNaN(downloadSize))
						downloadSize = 0;
					downloadProgressText.text =
						$"{Math.Convert.BytesToMB(request.downloadedBytes):0.0}{LeanLocalization.GetTranslationText("units-megabyte_short", "MB")} / " +
						$"{Math.Convert.BytesToMB(downloadSize):0.0}{LeanLocalization.GetTranslationText("units-megabyte_short")}";
					#endregion

					#region Queued Count
					queuedCountText.text =
						$"{LeanLocalization.GetTranslationText("downloads-queued", "Queued")}: {Queue.Count()}";
					#endregion

					oldDownloadedBytes = request.downloadedBytes;

				}
				else
				{
					dataPoints.Clear();
					oldDownloadedBytes = request.downloadedBytes;
					lineRenderer.Points = new Vector2[] { Vector2.zero };
				}
			}
			else
			{
				downloadingAssets.SetActive(false);
				if (!extracting)
					progressBar.gameObject.SetActive(false);
				dataPoints.Clear();
				oldDownloadedBytes = 0;
				lineRenderer.Points = new Vector2[] { Vector2.zero };
			}
		}

		public void Cancel(Cards.CardHandler.CardData toCancel)
		{
			Cancel(toCancel.ToDownloadData());
		}

		public void Cancel(DownloadData toCancel)
		{
			if (currentlyDownloading.Equals(toCancel))
			{
				StopCoroutine(Downloader(currentlyDownloading));
				request.Abort();
				byte tries = 0;
				while (Directory.Exists(Constants.Constants.DownloadPath))
				{
					tries++;
					try
					{
						Directory.Delete(Constants.Constants.DownloadPath, true);
					}
					catch (IOException e)
					{
						Debug.LogWarning(e.Message);
					}
					if (tries > 10)
					{
						Debug.LogError($"Could not delete download files at path {Constants.Constants.DownloadPath}.");
						break;
					}
				}
				request = null;
				downloadInProgress = false;
			}
			else if (Queue.Contains(toCancel))
			{
				Queue.Remove(toCancel);
			}
		}

		IEnumerator DownloadError(string error)
		{
			Debug.LogError(error);
			downloadError = true;
			yield return new WaitForSeconds(5f);
			downloadError = false;
			request = null;
			updateGraphs = false;
			yield return new WaitForSeconds(Time.fixedDeltaTime * 2);
			downloadInProgress = false;
		}

		IEnumerator DownloadError(Exception error)
		{
			yield return DownloadError($"{error.Message}\n{error.StackTrace}");
		}

		IEnumerator Downloader(DownloadData downloadData)
		{
			string downloadDir = Constants.Constants.DownloadPath;
            if (Directory.Exists(downloadDir))
                Directory.Delete(downloadDir, true);
			DirectoryInfo downloadTmp = Directory.CreateDirectory(downloadDir);
			downloadTmp.Attributes |= FileAttributes.Hidden;
            string downloadPath = Path.Combine(downloadDir, "data.nbf");
			request = new UnityWebRequest(downloadData.download)
			{
				downloadHandler = new DownloadHandlerFile(downloadPath)
			};
			yield return request.SendWebRequest();
			if (request != null)
			{
				if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError || request.result == UnityWebRequest.Result.ProtocolError)
				{
					StartCoroutine(DownloadError(request.error));
				}
				else
				{
					yield return new WaitWhile(() => request?.downloadProgress < 1f);
					if (request != null)
					{
						request = null;
						if (downloadInProgress)
						{
							extracting = true;
							downloadingAssets.SetActive(false);
							extractingAssets.SetActive(true);
							progressBarBackground.color = progressBarExtracting;
							progressBarFill.color = new Color(0, 0, 0, 0);
							bool extractionCompleted = false;
							StartCoroutine(Extractor(downloadPath, downloadData, () => extractionCompleted = true));
							yield return new WaitWhile(() => !extractionCompleted);
							gameHandler.LoadGames();
							yield return new WaitForSeconds(Time.fixedDeltaTime * 2);
							extractingAssets.SetActive(false);
							extractingText.color = extractingNormalColor;
							extractingText.text = extractingNormalText;
							progressBarBackground.color = progressNormalColor;
							progressBarFill.color = progressFillNormalColor;
							progressBar.gameObject.SetActive(false);
							extracting = false;
							downloadInProgress = false;
						}
					}
				}
			}
		}

		void OnApplicationQuit()
		{
			Queue.Clear();
			StopAllCoroutines();

			request?.Abort();

			int tries = 0;
			while (Directory.Exists(Constants.Constants.DownloadPath))
			{
				tries++;
				try
				{
					Directory.Delete(Constants.Constants.DownloadPath, true);
				}
				catch (IOException e)
				{
					Debug.LogWarning(e.Message);
				}
				if (tries > 10)
				{
					Debug.LogError($"Could not delete download files on exit at path {Constants.Constants.DownloadPath}.");
					break;
				}
			}
		}

		IEnumerator Extractor(string zipPath, DownloadData downloadData, Action onComplete = null)
		{
			bool extractError = false;

			string extractPath = Path.Combine(Constants.Constants.DownloadPath, "extraction");
			if (Directory.Exists(extractPath))
				Directory.Delete(extractPath, true);

			yield return null;

			ZipFile.ExtractToDirectory(zipPath, extractPath);

			yield return null;

			bool uninstalled = false;
			bool updated = false;
			StartCoroutine(gameHandler.UpdateUninstall(downloadData, (u) =>
			{
				uninstalled = true;
				updated = u;
			}));

			string gamePath = Path.Combine(Constants.Constants.GamesPath, downloadData.name);
			if (downloadData.customPath != null)
			{
				gamePath = Path.Combine(downloadData.customPath, downloadData.name);
				// Extra prosessointi kakkaa
				if (!SettingsManager.Settings.customGamePaths.Keys.Contains(downloadData.name))
					SettingsManager.Settings.customGamePaths.Add(downloadData.name, gamePath);
				else
					Debug.LogWarning($"Custom path \"{gamePath}\" for game \"{downloadData.name}\" exists already!");

				SettingsManager.Save();
			}

			yield return new WaitWhile(() => !uninstalled);

			try
			{
				if (Directory.GetDirectories(extractPath).Length == 1 && Directory.GetFiles(extractPath).Length < 1)
					Directory.Move(Directory.GetDirectories(extractPath)[0], gamePath);
				else
					Directory.Move(extractPath, gamePath);
			}
			catch (Exception e)
			{
				StartCoroutine(DownloadError(e));
				extractError = true;
				extractingText.color = errorProgressColor;
				extractingText.text = "[ERROR]";
				progressBarBackground.color = errorProgressColor;
				progressBarFill.color = new Color(0, 0, 0, 0);
			}

			yield return null;
			if (Directory.Exists(Constants.Constants.DownloadPath))
				Directory.Delete(Constants.Constants.DownloadPath, true);
			yield return null;

			string gamedataPath = Path.Combine(gamePath, GameHandler.gamedataFilePath);
			GameHandler.GameData gamedata;
			if (File.Exists(gamedataPath))
			{
				string encrypted = File.ReadAllText(gamedataPath);
				string unencrypted = Encryption.EncryptionHelper.DecryptString(encrypted);
				gamedata = JsonUtility.FromJson<GameHandler.GameData>(unencrypted);
				gamedata.version = downloadData.version;
				gamedata.executable_path = downloadData.executable_path;
				gamedata.last_interest = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds();

				yield return null;
			}
			else
			{
				gamedata = new GameHandler.GameData
				{
					name = downloadData.name,
					version = downloadData.version,
					executable_path = downloadData.executable_path,
					last_interest = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds()
				};
			}
			string gamedataJson = JsonUtility.ToJson(gamedata, true);
			string gamedataEncrypted = Encryption.EncryptionHelper.EncryptString(gamedataJson);

			try
			{
				if (!Directory.Exists(GameHandler.launcherDataFilePath))
					Directory.CreateDirectory(Path.Combine(gamePath, GameHandler.launcherDataFilePath));
				File.WriteAllText(Path.Combine(gamePath, GameHandler.gamedataFilePath), gamedataEncrypted);
			}
			catch (Exception e)
			{
				StartCoroutine(DownloadError(e));
				extractError = true;
				extractingText.color = errorProgressColor;
				extractingText.text = "[ERROR]";
				progressBarBackground.color = errorProgressColor;
				progressBarFill.color = new Color(0, 0, 0, 0);
			}

			yield return null;

			if (extractError)
				yield return new WaitForSeconds(5);
			onComplete?.Invoke();
			if (!extractError)
			{
				if (!updated)
				{
					Debug.Log($"Installed game: {downloadData.name}");
					AnalyticsEvent.Custom("game_installed", new Dictionary<string, object>
				{
					{ "name", downloadData.name},
					{ "version", downloadData.version },
					{ "playtime", downloadData.Playtime }
				});
					AnalyticsEvent.Custom($"{downloadData.name}_installed", new Dictionary<string, object>
				{
					{ "version", downloadData.version },
					{ "playtime", downloadData.Playtime }
				});
				}
				else
				{
					Debug.Log($"Updated game: {downloadData.name}");
					AnalyticsEvent.Custom("game_update", new Dictionary<string, object>
				{
					{ "name", downloadData.name },
					{ "version", downloadData.version },
					{ "playtime", downloadData.Playtime }
				});
					AnalyticsEvent.Custom($"{downloadData.name}_update", new Dictionary<string, object>
				{
					{ "version", downloadData.version },
					{ "playtime", downloadData.Playtime }
				});
				}
			}
		}
	}
}