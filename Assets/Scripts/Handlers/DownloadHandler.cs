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
using NALStudio.IO;

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
        public TextMeshProUGUI timeRemainingText;
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
        public Color errorColor;
        Color progressNormalColor;
        Color progressFillNormalColor;
        Color lineNormalColor;
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
        public UniversalData currentlyDownloading;

        void Awake()
        {
            progressNormalColor = progressBarBackground.color;
            progressFillNormalColor = progressBarFill.color;
            lineNormalColor = lineRenderer.color;
            extractingNormalColor = extractingText.color;
            extractingNormalText = extractingText.text;
            queuedCountText.text =
                $"{LeanLocalization.GetTranslationText("downloads-queued", "Queued")}: {Queue.Count}";
        }

        public class QueueHandler
        {
            List<string> queued = new List<string>();
            Dictionary<string, string> customPaths = new Dictionary<string, string>();

            public int Count
			{
				get
				{
                    return queued.Count;
				}
			}

            public void Add(UniversalData data)
            {
                if (!queued.Contains(data.UUID))
                    queued.Add(data.UUID);
            }

            public void Add(UniversalData data, string customPath)
            {
                if (!string.IsNullOrEmpty(customPath))
					customPaths[data.UUID] = customPath;

				Add(data);
            }

            public void Remove(UniversalData item)
            {
                if (queued.Contains(item.UUID))
                    queued.Remove(item.UUID);
            }

            public bool Contains(UniversalData item)
            {
                return queued.Contains(item.UUID);
            }

            public string CustomPath(UniversalData item)
			{
                if (customPaths.TryGetValue(item.UUID, out string value))
                    return value;
                else
                    return null;
			}

            public void Clear()
            {
                queued.Clear();
            }

            public UniversalData GetNextDownload()
            {
                if (queued.Count > 0)
                {
                    string tmpUUID = queued[0];
                    queued.RemoveAt(0);
                    UniversalData tmpData = DataHandler.UniversalDatas.Get(tmpUUID);
                    if (tmpData == null)
						Debug.LogWarning($"UUID \"{tmpUUID}\" not found in UniversalDatas! Removing from download queue...");
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
            #region Queued Count
            queuedCountText.text =
                $"{LeanLocalization.GetTranslationText("downloads-queued", "Queued")}: {Queue.Count}";
            #endregion

            if (updateGraphs && Application.targetFrameRate != 1 && request != null && !extracting)
            {
                downloadingAssets.SetActive(true);
                progressBar.gameObject.SetActive(true);

                if (downloadsMenu.opened)
                {

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
                    downloadSpeedText.text = $"{Math.Convert.BitsToMb(dataPoints.Last() * 8):0.0}{LeanLocalization.GetTranslationText("units-megabits_per_second-short", "Mb/s")}";
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

                    #region Download Time Remaining
                    double remainingBytes = downloadSize - request.downloadedBytes;
                    double timeToDownload = remainingBytes / dataPoints.Last();
                    TimeSpan toFormat = !double.IsNaN(timeToDownload) ? TimeSpan.FromSeconds(timeToDownload) : TimeSpan.Zero;
                    #region Time Formatting
                    string formatted;
                    if (toFormat.TotalDays >= 1d)
                        formatted = toFormat.ToString(@"d\d\ hh\h\ mm\m\ ss\s\ ");
                    else if (toFormat.TotalHours >= 1d)
                        formatted = toFormat.ToString(@"h\h\ mm\m\ ss\s\ ");
                    else if (toFormat.TotalMinutes >= 1d)
                        formatted = toFormat.ToString(@"m\m\ ss\s\ ");
                    else
                        formatted = toFormat.ToString(@"s\s\ ");
                    #endregion
                    timeRemainingText.text = LeanLocalization.GetTranslationText("downloads-remaining", "Time Remaining:") + $" {formatted}";
                    #endregion

                    #region Download Error
                    if (downloadError)
                    {
                        downloadSpeedText.text = "[ERROR]";
                        timeRemainingText.text = LeanLocalization.GetTranslationText("downloads-remaining", "Time Remaining:") + " [ERROR]";
                        progressBarBackground.color = errorColor;
                        lineRenderer.color = errorColor;
                        progressBarFill.color = new Color(0, 0, 0, 0);
                    }
                    else if (progressBarBackground.color != progressNormalColor)
                    {
                        progressBarBackground.color = progressNormalColor;
                        progressBarFill.color = progressFillNormalColor;
                        lineRenderer.color = lineNormalColor;
                    }
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

        public void Cancel(UniversalData toCancel)
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
            yield return new WaitForSeconds(10f);
            downloadError = false;
            request = null;
            updateGraphs = false;
            yield return new WaitForSeconds(Time.fixedDeltaTime * 2);
            downloadInProgress = false;
        }

        IEnumerator DownloadError(Exception error)
        {
            yield return StartCoroutine(DownloadError($"{error.Message}\n{error.StackTrace}"));
        }

        IEnumerator Downloader(UniversalData data)
        {
            string downloadDir = Constants.Constants.DownloadPath;
            if (Directory.Exists(downloadDir))
                Directory.Delete(downloadDir); // Threaded delete access gets denies.
            DirectoryInfo downloadTmp = Directory.CreateDirectory(downloadDir);
            downloadTmp.Attributes |= FileAttributes.Hidden;
            string downloadPath = Path.Combine(downloadDir, "data.nbf");
            request = new UnityWebRequest(data.DownloadUrl)
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
                            yield return StartCoroutine(Extractor(downloadPath, data));
							StartCoroutine(gameHandler.LoadGames());
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

        IEnumerator Extractor(string zipPath, UniversalData data)
        {
            if (!SettingsManager.Settings.allowInstallsDuringGameplay)
                yield return new WaitUntil(() => !gameHandler.gameRunning || ApplicationHandler.HasFocus || SettingsManager.Settings.allowInstallsDuringGameplay);

            bool extractError = false;

            string extractPath = Path.Combine(Constants.Constants.DownloadPath, "extraction");
            if (Directory.Exists(extractPath))
                yield return StartCoroutine(NALDirectory.Delete(extractPath, true));

            yield return StartCoroutine(NALZipFile.ExtractToDirectoryThreaded(zipPath, extractPath));

            bool updated = false;
            yield return StartCoroutine(gameHandler.UpdateUninstall(data, (u) => updated = u));

            string searchDir;
            if (Queue.CustomPath(data) == null)
            {
                searchDir = Path.Combine(Constants.Constants.GamesPath, data.Name);
				if (File.Exists(Path.Combine(searchDir, Constants.Constants.gamedataFilePath)))
				{
                    string json = File.ReadAllText(Path.Combine(searchDir, Constants.Constants.gamedataFilePath));
                    var tmp = UniversalData.LocalData.FromJson(json);
                    if (tmp != null && tmp.UUID != data.UUID)
                        searchDir = Path.Combine(Constants.Constants.GamesPath, data.UUID);
				}
			}
            else
            {
                searchDir = Queue.CustomPath(data);
			}

            string gamePath = Path.Combine(Constants.Constants.GamesPath, searchDir);
            string customPath = Queue.CustomPath(data);
            if (customPath != null)
            {
                gamePath = customPath;
				// Extra prosessointi kakkaa
				if (!SettingsManager.Settings.customGamePaths.Keys.Contains(data.UUID))
				{
					SettingsManager.Settings.customGamePaths.Add(data.UUID, gamePath);
				}
				else
				{
					Debug.LogWarning($"Custom path \"{gamePath}\" for game \"{data.UUID}\" ({data.Name}) exists already! Overriding old setting...");
                    SettingsManager.Settings.customGamePaths[data.UUID] = gamePath;
                }

				SettingsManager.Save();
            }

            try
            {
                string[] subDirs = Directory.GetDirectories(extractPath);
                if (subDirs.Length == 1 && Directory.GetFiles(extractPath).Length < 1)
                    Directory.Move(subDirs[0], gamePath);
                else
                    Directory.Move(extractPath, gamePath);
            }
            catch (Exception e)
            {
                StartCoroutine(DownloadError(e));
                extractError = true;
                extractingText.color = errorColor;
                extractingText.text = "[ERROR]";
                progressBarBackground.color = errorColor;
                progressBarFill.color = new Color(0, 0, 0, 0);
            }

            if (Directory.Exists(Constants.Constants.DownloadPath))
                yield return StartCoroutine(NALDirectory.Delete(Constants.Constants.DownloadPath, true));

			if (data.Local != null)
			{
				data.Local.Version = data.Remote.Version;
				data.Local.ExecutablePath = data.Remote.ExecutablePath;
				data.Local.LastInterest = DateTimeOffset.Now.ToUnixTimeSeconds();
                data.Local.Save();
			}
			else
			{
				data.Local = new UniversalData.LocalData(data.UUID,
					data.Remote.Version,
					data.Remote.ExecutablePath,
					gamePath);
                // Will automatically save and create a directory if needed.
			}
            string tmpPath = Path.Combine(gamePath, Constants.Constants.launcherDataFilePath);
            if (!Directory.Exists(tmpPath))
            {
                StartCoroutine(DownloadError($"No launcher data directory found. Excepted to find directory at path: \"{tmpPath}\""));
                extractError = true;
                extractingText.color = errorColor;
                extractingText.text = "[ERROR]";
                progressBarBackground.color = errorColor;
                progressBarFill.color = new Color(0, 0, 0, 0);
            }

            if (extractError)
                yield return new WaitForSeconds(5f);

            if (!extractError)
            {
                if (!updated)
                {
                    Debug.Log($"Installed game: \"{data.UUID}\" ({data.Name})");
                    AnalyticsEvent.Custom("game_installed", new Dictionary<string, object>
                    {
                        { "name", data.Name},
                        { "uuid", data.UUID },
                        { "version", data.Remote.Version },
                        { "playtime", data.Playtime }
                    });
                    AnalyticsEvent.Custom($"{data.Name}_{data.UUID}_installed", new Dictionary<string, object>
                    {
                        { "version", data.Remote.Version },
                        { "playtime", data.Playtime }
                    });
                    }
                else
                {
                    Debug.Log($"Updated game: \"{data.UUID}\" ({data.Name})");
                    AnalyticsEvent.Custom("game_updated", new Dictionary<string, object>
                    {
                        { "name", data.Name },
                        { "uuid", data.UUID },
                        { "version", data.Remote.Version },
                        { "playtime", data.Playtime }
                    });
                    AnalyticsEvent.Custom($"{data.Name}_{data.UUID}_updated", new Dictionary<string, object>
                    {
                        { "version", data.Remote.Version },
                        { "playtime", data.Playtime }
                    });
                }
            }
        }
    }
}