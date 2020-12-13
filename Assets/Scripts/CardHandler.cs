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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using NALStudio.JSON;
using NALStudio.UI;

namespace NALStudio.GameLauncher.Cards
{
	public class CardHandler : MonoBehaviour
	{
		#region Variables

		public float gridHeight;
		public float verticalSpacing;
		[Space(10)]
		public GameObject cardPrefab;
		public float cardAnimationBasedelay;
		public float cardAnimationDuration = 0.5f;

		GridLayoutGroup gridLayout;
		RectTransform rectTranform;
		int childCount;

		public List<UITweener> cardTweeners;

		#endregion

		[System.Serializable]
		public class CardData
		{
			public string developer;
			public string download;
			public int price;
			public string publisher;
			public string thumbnail;
			public string title;
		}

		void Start()
		{
			gridLayout = GetComponent<GridLayoutGroup>();
			rectTranform = GetComponent<RectTransform>();
			CalculateCellSize();

			childCount = transform.childCount;
			StartCoroutine(loadCards());
		}

		IEnumerator loadCards()
		{
			UnityWebRequest wr = UnityWebRequest.Get("https://nalstudio-game-launcher-api-default-rtdb.europe-west1.firebasedatabase.app/.json");
			yield return wr.SendWebRequest();
			if (wr.isNetworkError || wr.isHttpError)
			{
				Debug.LogError(wr.error);
			}
			else
			{
				string jsonString = wr.downloadHandler.text;
				CardData[] cardDatas = JsonHelper.FromJsonArray<CardData>(jsonString);
				for (int i = 0; i < cardDatas.Length; i++)
				{
					GameObject instantiated = Instantiate(cardPrefab, transform);
					Card insCard = instantiated.GetComponent<Card>();
					insCard.LoadAssets(cardDatas[i]);
					UITweener insTweener = instantiated.GetComponent<UITweener>();
					insTweener.duration = cardAnimationDuration;
					insTweener.delay = cardAnimationBasedelay + (i  / 10f);
					cardTweeners.Add(insTweener);
				}
				CalculateCellSize();
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
			rectTranform = GetComponent<RectTransform>();
			CalculateCellSize();
		}
#endif

		void CalculateRectHeight()
		{
			rectTranform.sizeDelta = new Vector2(rectTranform.sizeDelta.x,
				gridHeight + ((gridHeight + verticalSpacing) * Mathf.CeilToInt((rectTranform.childCount - 1) / 3)));
		}

		void CalculateCellSize()
		{
			gridLayout.cellSize = new Vector2((rectTranform.rect.width - 20) / 3, gridHeight);
			gridLayout.spacing = new Vector2(gridLayout.spacing.x, verticalSpacing);
			if (childCount != transform.childCount)
				CalculateRectHeight();
		}

		void OnRectTransformDimensionsChange()
		{
			if (gridLayout != null)
				CalculateCellSize();
		}
	}
}