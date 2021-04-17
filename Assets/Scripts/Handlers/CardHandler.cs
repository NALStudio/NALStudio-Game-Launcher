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
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using NALStudio.JSON;
using NALStudio.UI;
using NALStudio.GameLauncher.Games;
using System.Linq;
using NALStudio.Extensions;
using System.Text.RegularExpressions;

namespace NALStudio.GameLauncher.Cards
{
    public class CardHandler : MonoBehaviour
    {
        public enum SortingMode { relevance, alphabetical }
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
        public SortingMode sortingMode;
        public TMPro.TMP_Dropdown sortDropdown;
        [HideInInspector]
        public List<GameObject> cards = new List<GameObject>();
        [HideInInspector]
        public List<Card> cardScripts = new List<Card>();
        [HideInInspector]
        public List<UITweener> cardTweeners = new List<UITweener>();

        GridLayoutGroup gridLayout;
        RectTransform rectTransform;

        string filter = null;

        #endregion

        void Start()
        {
            gridLayout = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();

            sortingMode = (SortingMode)PlayerPrefs.GetInt("sorting/cards", 0);
            sortDropdown.value = (int)sortingMode;

            // 3600 seconds in an hour.
            InvokeRepeating(nameof(ReloadCards), 3600f, 3600f);

            StartCoroutine(LoadCards());
        }

		void OnApplicationQuit()
		{
            CancelInvoke(nameof(ReloadCards));
		}

		public void SortCards(int index)
        {
            if ((SortingMode)index != sortingMode)
            {
                PlayerPrefs.SetInt("sorting/cards", index);
                sortingMode = (SortingMode)index;
                AddCards();
            }
        }

        public void SetFilter(string f)
		{
            if (f == filter)
                return;
            filter = f;
            AddCards();
		}

        void AddCards()
        {
            foreach (GameObject go in cards)
                Destroy(go);
            cards.Clear();
            cardScripts.Clear();
            cardTweeners.Clear();

            UniversalData[] sortedDatas = DataHandler.UniversalDatas.Get().Where((d) => !d.Hidden).ToArray();
            switch (sortingMode)
            {
                case SortingMode.alphabetical:
                    sortedDatas = sortedDatas.OrderBy(d => d.DisplayName).ToArray();
                    break;
                case SortingMode.relevance:
                    sortedDatas = sortedDatas.OrderBy(d => d.Order).ToArray();
                    break;
            }

            if (!string.IsNullOrEmpty(filter))
			{
                List<UniversalData> filtered = new List<UniversalData>();
                string normalizedFilter = Regex.Replace(filter, @"\s", string.Empty).ToLowerInvariant();

                // Distance from data function
                int distanceFromData(UniversalData d)
				{
					string normalizedDisplayName = Regex.Replace(d.DisplayName, @"\s", string.Empty).ToLowerInvariant();
                    string shortName;
                    if (normalizedDisplayName.Length > normalizedFilter.Length)
                        shortName = normalizedDisplayName.Substring(0, normalizedFilter.Length);
                    else
                        shortName = normalizedDisplayName;
                    return shortName.DamerauLevenshteinDistance(normalizedFilter, normalizedFilter.Length / 2);
                }

				for (int i = 0; i < sortedDatas.Length; i++)
				{
                    UniversalData toCheck = sortedDatas[i];
                    if (distanceFromData(toCheck) != -1)
						filtered.Add(toCheck);
				}

                // Checking everything twice is not exactly optimal, but I don't know any other way.
                sortedDatas = filtered.OrderBy(d => distanceFromData(d)).ToArray();
            }

            for (int i = 0; i < sortedDatas.Length; i++)
            {
                GameObject instantiated = Instantiate(cardPrefab, transform);
                cards.Add(instantiated);
                Card insCard = instantiated.GetComponent<Card>();
                insCard.storePage = storePage;
                insCard.LoadAssets(sortedDatas[i]);
                cardScripts.Add(insCard);
                UITweener insTweener = instantiated.GetComponent<UITweener>();
                insTweener.duration = cardAnimationDuration;
                insTweener.delay = cardAnimationBaseDelay + (i * cardAnimationAddDelay);
                cardTweeners.Add(insTweener);
            }
            CalculateCellSize();
        }

        void ReloadCards()
        {
            SettingsManager.ReloadRemote();
            StartCoroutine(LoadCards());
            Debug.Log("Application has been running for an hour. Reloading cards...");
        }

        IEnumerator LoadCards()
        {
            DataHandler.UniversalDatas.Loaded = false;
            DataHandler.UniversalDatas.Clear();
            yield return new WaitUntil(() => SettingsManager.RemoteLoaded);

            string[] cardKeys = ConfigManager.appConfig.GetKeys();
            if (cardKeys.Length < 1)
			{
                Debug.LogError("CardKeys is empty... Closing Application.");
                Application.Quit();
			}
            foreach (string cardKey in cardKeys)
            {
                string fetchedJson = ConfigManager.appConfig.GetJson(cardKey, null);
                if (fetchedJson != null)
                    DataHandler.UniversalDatas.Add(new UniversalData(fetchedJson));
                else
                    Debug.LogError($"Fetched json for game: \"{cardKey}\" could not be loaded!");
            }

			DataHandler.UniversalDatas.Loaded = true;

            AddCards();
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

        void OnRectTransformDimensionsChange()
        {
            if (gridLayout == null)
                return;
            CalculateCellSize();
        }
    }
}