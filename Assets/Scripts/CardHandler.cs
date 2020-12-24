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

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using NALStudio.JSON;
using NALStudio.UI;
using NALStudio.Encryption;
using NALStudio.GameLauncher.Games;

namespace NALStudio.GameLauncher.Cards
{
	public class CardHandler : MonoBehaviour
	{
		#region Variables

		public float gridHeight;
		public float verticalSpacing;
		[Space(10)]
		public GameObject cardPrefab;
		public float cardAnimationBaseDelay;
		public float cardAnimationAddDelay = 0.05f;
		public float cardAnimationDuration = 0.5f;
		[Space(10)]
		public StorePage storePage;
		public GameHandler gameHandler;
		[Space(10)]
		[HideInInspector]
		public List<GameObject> cards = new List<GameObject>();
		[HideInInspector]
		public List<Card> cardScripts = new List<Card>();
		[HideInInspector]
		public List<UITweener> cardTweeners = new List<UITweener>();

		GridLayoutGroup gridLayout;
		RectTransform rectTransform;

		#endregion

		[System.Serializable]
		public class CardData
		{
			public string title;
			public string developer;
			public string publisher;
			public int price;
			public string version;
			public string release_date;
			public bool early_access;
			public string thumbnail;
			public string download;
			public string executable_path;

			public Texture2D thumbnailTexture;
		}

		void Start()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();

			StartCoroutine(LoadCards());
		}

		public void AddToGames()
		{
			foreach (Card c in cardScripts)
			{
				foreach(Game g in gameHandler.gameScripts)
				{
					if (c.cardData.title == g.gameData.name)
					{
						g.cardData = c.cardData;
						break;
					}
				}
			}
		}

		void AddCards(string json)
		{
			foreach (GameObject go in cards)
				Destroy(go);
			cards.Clear();
			cardScripts.Clear();
			cardTweeners.Clear();

			CardData[] cardDatas = JsonHelper.FromJsonArray<CardData>(json);
			for (int i = 0; i < cardDatas.Length; i++)
			{
				GameObject instantiated = Instantiate(cardPrefab, transform);
				cards.Add(instantiated);
				Card insCard = instantiated.GetComponent<Card>();
				insCard.storePage = storePage;
				insCard.LoadAssets(cardDatas[i]);
				cardScripts.Add(insCard);
				UITweener insTweener = instantiated.GetComponent<UITweener>();
				insTweener.duration = cardAnimationDuration;
				insTweener.delay = cardAnimationBaseDelay + (i * cardAnimationAddDelay);
				cardTweeners.Add(insTweener);
			}
			AddToGames();
			CalculateCellSize();
		}

		IEnumerator LoadCards()
		{
			string dataDirPath = Path.Combine(Cache.Cache.Path, "data");
			string fileName = EncryptionHelper.ToMD5Hex(DateTime.UtcNow.ToString("ddMMyyyy"));
			string filePath = Path.Combine(dataDirPath, $"{fileName}.nal");
			if (File.Exists(filePath))
			{
				string encryptedText = File.ReadAllText(filePath);
				AddCards(EncryptionHelper.DecryptString(encryptedText));
			}
			else
			{
				Debug.Log("Loading new JSON file...");
				UnityWebRequest wr = UnityWebRequest.Get("https://nalstudio-game-launcher-api-default-rtdb.europe-west1.firebasedatabase.app/.json");
				yield return wr.SendWebRequest();
				if (wr.result == UnityWebRequest.Result.ConnectionError || wr.result == UnityWebRequest.Result.DataProcessingError || wr.result == UnityWebRequest.Result.ProtocolError)
				{
					Debug.LogError(wr.error);
				}
				else
				{
					yield return new WaitWhile(() => wr.downloadProgress < 1f);
					string jsonString = wr.downloadHandler.text;
					if (Directory.Exists(dataDirPath))
						Directory.Delete(dataDirPath, true);
					Directory.CreateDirectory(dataDirPath);
					File.WriteAllText(filePath, EncryptionHelper.EncryptString(jsonString));
					AddCards(jsonString);
				}
			}
		}

		public void PlayAnimation()
		{
			foreach (UITweener tweener in cardTweeners)
				tweener.DoTween();
		}

#if UNITY_EDITOR
		void Reset()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTransform = GetComponent<RectTransform>();
			CalculateCellSize();
		}
#endif

		void CalculateRectHeight()
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x,
				gridHeight + ((gridHeight + verticalSpacing) * Mathf.CeilToInt((transform.childCount - 1) / 3)));
		}

		void CalculateCellSize()
		{
			gridLayout.cellSize = new Vector2((rectTransform.rect.width - 20) / 3, gridHeight);
			gridLayout.spacing = new Vector2(gridLayout.spacing.x, verticalSpacing);
			CalculateRectHeight();
		}
	}
}