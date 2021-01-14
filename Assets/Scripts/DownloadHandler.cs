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
		bool downloadError;
		[Space(10f)]
		public GameHandler gameHandler;

        UnityWebRequest request;
		bool downloadInProgress = false;
		[HideInInspector]
		public QueueHandler Queue = new QueueHandler();
		[HideInInspector]
		public Cards.CardHandler.CardData currentlyDownloading;

		void Awake()
		{
			progressNormalColor = progressBarBackground.color;
			progressFillNormalColor = progressBarFill.color;
			queuedCountText.text =
				$"{LeanLocalization.GetTranslationText("downloads-queued", "Queued")}: {Queue.Count()}";
		}

		public class QueueHandler
		{
            List<Cards.CardHandler.CardData> queued = new List<Cards.CardHandler.CardData>();
            public void Add(Cards.CardHandler.CardData cardData)
			{
                if (!queued.Contains(cardData))
                    queued.Add(cardData);
			}

            public void Remove(Cards.CardHandler.CardData item)
			{
                queued.Remove(item);
			}

			public bool Contains(Cards.CardHandler.CardData item)
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

			public Cards.CardHandler.CardData GetNextDownload()
			{
				if (queued.Count > 0)
				{
					Cards.CardHandler.CardData tmpData = queued[0];
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
					#region Error Assets
					if (downloadError)
					{
						downloadSpeedText.text = "[ERROR]";
						progressBarBackground.color = errorProgressColor;
						progressBarFill.color = new Color(0, 0, 0, 0);
					}
					else if(progressBarBackground.color != progressNormalColor)
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
			if (currentlyDownloading == toCancel)
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

		IEnumerator Downloader(Cards.CardHandler.CardData cardData)
		{
			string downloadDir = Constants.Constants.DownloadPath;
            if (Directory.Exists(downloadDir))
                Directory.Delete(downloadDir, true);
			DirectoryInfo downloadTmp = Directory.CreateDirectory(downloadDir);
			downloadTmp.Attributes |= FileAttributes.Hidden;
            string downloadPath = Path.Combine(downloadDir, "data.nbf");
			request = new UnityWebRequest(cardData.download)
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
							StartCoroutine(Extractor(downloadDir, downloadPath, cardData, () => extractionCompleted = true));
							yield return new WaitWhile(() => !extractionCompleted);
							gameHandler.LoadGames();
							yield return new WaitForSeconds(Time.fixedDeltaTime * 2);
							extractingAssets.SetActive(false);
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

		IEnumerator Extractor(string downloadDir, string zipPath, Cards.CardHandler.CardData cardData, Action onComplete = null)
		{
			string extractPath = Path.Combine(downloadDir, "extraction");
			if (Directory.Exists(extractPath))
				Directory.Delete(extractPath, true);
			yield return null;
			ZipFile.ExtractToDirectory(zipPath, extractPath);
			yield return null;
			string gamePath = Path.Combine(Constants.Constants.GamesPath, cardData.title);
			bool uninstalled = false;
			bool updated = false;
			StartCoroutine(gameHandler.UpdateUninstall(cardData, (u) =>
			{
				uninstalled = true;
				updated = u;
			}));
			yield return new WaitWhile(() => !uninstalled);
			if (Directory.GetDirectories(extractPath).Length == 1 && Directory.GetFiles(extractPath).Length < 1)
				Directory.Move(Directory.GetDirectories(extractPath)[0], gamePath);
			else
				Directory.Move(extractPath, gamePath);
			yield return null;
			if (Directory.Exists(downloadDir))
				Directory.Delete(downloadDir, true);
			yield return null;

			string gamedataPath = Path.Combine(gamePath, GameHandler.gamedataFilePath);
			GameHandler.GameData gamedata;
			if (File.Exists(gamedataPath))
			{
				string encrypted = File.ReadAllText(gamedataPath);
				string unencrypted = Encryption.EncryptionHelper.DecryptString(encrypted);
				gamedata = JsonUtility.FromJson<GameHandler.GameData>(unencrypted);
				gamedata.version = cardData.version;
				gamedata.executable_path = cardData.executable_path;
				gamedata.last_interest = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds();
				yield return null;
			}
			else
			{
				gamedata = new GameHandler.GameData
				{
					name = cardData.title,
					version = cardData.version,
					executable_path = cardData.executable_path,
					last_interest = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds()
				};
			}
			string gamedataJson = JsonUtility.ToJson(gamedata, true);
			string gamedataEncrypted = Encryption.EncryptionHelper.EncryptString(gamedataJson);
			if (!Directory.Exists(GameHandler.launcherDataFilePath))
				Directory.CreateDirectory(Path.Combine(gamePath, GameHandler.launcherDataFilePath));
			File.WriteAllText(Path.Combine(gamePath, GameHandler.gamedataFilePath), gamedataEncrypted);
			yield return null;
			onComplete?.Invoke();
			if (!updated)
			{
				Debug.Log($"Installed game: {cardData.title}");
				AnalyticsEvent.Custom("game_installed", new Dictionary<string, object>
				{
					{ "name", cardData.title},
					{ "version", cardData.version },
					{ "playtime", PlayerPrefs.GetFloat($"playtime/{cardData.title}", 0) }
				});
				AnalyticsEvent.Custom($"{cardData.title}_installed", new Dictionary<string, object>
				{
					{ "version", cardData.version },
					{ "playtime", PlayerPrefs.GetFloat($"playtime/{cardData.title}", 0f) }
				});
			}
		}
	}
}