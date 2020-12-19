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

namespace NALStudio.GameLauncher
{
    public class DownloadHandler : MonoBehaviour
    {
		public DownloadsMenu downloadsMenu;
		[Space(10f)]
		public UILineRenderer lineRenderer;
		public TextMeshProUGUI downloadSpeedText;
		public RectTransform downloadSpeedRect;
		public RectTransform LineArea;
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
		public Color errorProgressFillColor;
		Color progressNormalColor;
		Color progressFillNormalColor;
		bool downloadError;

        UnityWebRequest request;
		[HideInInspector]
		public QueueHandler Queue = new QueueHandler();

		void Awake()
		{
			progressNormalColor = progressBarBackground.color;
			progressFillNormalColor = progressBarFill.color;
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
			if (request != null)
			{
				if (!updateGraphs)
				{
					updateGraphs = true;
				}
			}
			else
			{
				updateGraphs = false;
				Cards.CardHandler.CardData cardData = Queue.GetNextDownload();
				if (cardData != null)
				{
					StartCoroutine(Downloader(cardData));
				}
			}
		}

		void FixedUpdate()
		{
			if (updateGraphs && request != null)
			{
				downloadSpeedText.gameObject.SetActive(true);
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
					dataPoints.Add((request.downloadedBytes - oldDownloadedBytes) / Time.fixedDeltaTime);
					if (dataPoints.Count > 1)
					{
						if (dataPoints[dataPoints.Count - 1] == 0f)
						{
							dataPoints[dataPoints.Count - 1] = dataPoints[dataPoints.Count - 2];
							if (dataPoints.TrueForAll(i => i == 0f))
								dataPoints[dataPoints.Count - 1] = 0f;
						}
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

					#region DownloadSpeed
					downloadSpeedText.text = $"{Math.Convert.BytesToMB(Mathf.RoundToInt(dataPoints.Last() * 8)):0.0}Mb/s";
					downloadSpeedRect.anchoredPosition = new Vector2((lineRenderer.Points.Last().x * LineArea.rect.width) + lineRenderer.LineThickness + 5, downloadSpeedRect.anchoredPosition.y);
					#endregion

					oldDownloadedBytes = request.downloadedBytes;

					if (downloadError)
						downloadSpeedText.text = "[ERROR]";
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
				progressBar.gameObject.SetActive(false);
				downloadSpeedText.gameObject.SetActive(false);
				dataPoints.Clear();
				oldDownloadedBytes = 0;
				lineRenderer.Points = new Vector2[] { Vector2.zero };
			}
		}

		void DownloadError(string error)
		{
			Debug.LogError(error);
			progressBarBackground.color = errorProgressColor;
			progressBarFill.color = errorProgressFillColor;
			downloadError = true;
		}

		IEnumerator Downloader(Cards.CardHandler.CardData cardData)
		{
            string downloadDir = Path.Combine(Constants.Constants.GamesPath, "download");
            if (Directory.Exists(downloadDir))
                Directory.Delete(downloadDir, true);
            string downloadPath = Path.Combine(downloadDir, "data.nbf");
			request = new UnityWebRequest(cardData.download)
			{
				downloadHandler = new DownloadHandlerFile(downloadPath)
			};
			yield return request.SendWebRequest();
			if (request.isNetworkError || request.isHttpError)
			{
				DownloadError(request.error);
			}
			else
			{
				yield return new WaitWhile(() => request.downloadProgress < 1.0);
				StartCoroutine(Extractor(downloadDir, downloadPath, cardData));
			}
		}

		IEnumerator Extractor(string downloadDir, string zipPath, Cards.CardHandler.CardData cardData)
		{
			string extractPath = Path.Combine(downloadDir, "extraction");
			ZipFile.ExtractToDirectory(zipPath, Path.Combine(downloadDir, "extraction"));
			string gamePath = Path.Combine(Constants.Constants.GamesPath, cardData.title);
			if (Directory.Exists(gamePath))
				Directory.Delete(gamePath, true);
			if (Directory.GetDirectories(extractPath).Length == 1 && Directory.GetFiles(extractPath).Length < 1)
				Directory.Move(Directory.GetDirectories(extractPath)[0], gamePath);
			else
				Directory.Move(extractPath, gamePath);
			Directory.Delete(downloadDir, true);
			request = null;
			yield return null;
		}
	}
}