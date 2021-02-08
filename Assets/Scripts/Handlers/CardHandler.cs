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
using NALStudio.Encryption;
using NALStudio.GameLauncher.Games;
using System.Linq;

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

            public DownloadHandler.DownloadData ToDownloadData()
            {
                return new DownloadHandler.DownloadData()
                {
                    name = title,
                    version = version,
                    download = download,
                    executable_path = executable_path,
                    customPath = null
                };
            }
        }

        void Start()
        {
            gridLayout = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();

            sortingMode = (SortingMode)PlayerPrefs.GetInt("sorting/cards", 0);
            sortDropdown.value = (int)sortingMode;

            StartCoroutine(LoadCards());
        }

        public void SortCards(int index)
        {
            if ((SortingMode)index != sortingMode)
            {
                PlayerPrefs.SetInt("sorting/cards", index);
                sortingMode = (SortingMode)index;
                StartCoroutine(LoadCards());
            }
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

        void AddCards(IReadOnlyCollection<string> jsons)
        {
            foreach (GameObject go in cards)
                Destroy(go);
            cards.Clear();
            cardScripts.Clear();
            cardTweeners.Clear();

            List<CardData> cardDatas = new List<CardData>();
            foreach (string json in jsons)
            {
                cardDatas.Add(JsonUtility.FromJson<CardData>(json));
            }

            switch (sortingMode)
            {
                case SortingMode.alphabetical:
                    cardDatas = cardDatas.OrderBy(c => c.title).ToList();
                    break;
            }

            for (int i = 0; i < cardDatas.Count; i++)
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

        void ReloadCards()
        {
            SettingsManager.ReloadRemote();
            StartCoroutine(LoadCards());
            Debug.Log("Application has been running for a day. Reloading cards...");
        }

        IEnumerator LoadCards()
        {
            yield return new WaitUntil(() => SettingsManager.RemoteLoaded);

            string[] cardKeys = ConfigManager.appConfig.GetKeys();
            List<string> jsons = new List<string>();
            foreach (string cardKey in cardKeys)
            {
                string fetchedJson = ConfigManager.appConfig.GetJson(cardKey, null);
                if (fetchedJson != null)
                    jsons.Add(fetchedJson);
                else
                    Debug.LogError($"Fetched json for game: \"{cardKey}\" could not be loaded!");
            }

            Invoke(nameof(ReloadCards), 86400f);

            AddCards(jsons);
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