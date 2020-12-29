﻿using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System;

namespace NALStudio.GameLauncher.Cards
{
    public class Card : MonoBehaviour
    {
        public CardHandler.CardData cardData;
        public RawImage thumbnail;
        public TextMeshProUGUI title;
        public TextMeshProUGUI devPub;
        public TextMeshProUGUI price;

        [HideInInspector]
        public StorePage storePage;

        public void LoadAssets(CardHandler.CardData cData)
        {
            cardData = cData;
            StartCoroutine(SetContent());
        }

        public void OpenStorePage()
        {
            storePage.Open(cardData);
        }

        void LanguageChange()
		{
            if (cardData.price == 0)
                price.text = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
        }

		void Start()
		{
            Lean.Localization.LeanLocalization.OnLocalizationChanged += LanguageChange;
		}

		IEnumerator SetContent()
        {
            if (cardData.thumbnail.StartsWith("https://imgur.com/", StringComparison.OrdinalIgnoreCase) || cardData.thumbnail.StartsWith("https://i.imgur.com/", StringComparison.OrdinalIgnoreCase))
			{
                if (!cardData.thumbnail.EndsWith("l.png", StringComparison.OrdinalIgnoreCase))
                    cardData.thumbnail = cardData.thumbnail.Insert(cardData.thumbnail.Length - 4, "l");
			}
            UnityWebRequest wr = new UnityWebRequest(cardData.thumbnail);
            DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            wr.downloadHandler = texDl;
            yield return wr.SendWebRequest();
            if (wr.result == UnityWebRequest.Result.ConnectionError || wr.result == UnityWebRequest.Result.DataProcessingError || wr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(wr.error);
            }
            else
            {
                yield return new WaitWhile(() => wr.downloadProgress < 1f);
				#region Thumbnail
				cardData.thumbnailTexture = texDl.texture;
                thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = cardData.thumbnailTexture.width / (float)cardData.thumbnailTexture.height;
                thumbnail.texture = cardData.thumbnailTexture;
                #endregion
                yield return null;
                #region Title
				title.text = cardData.title;
                #endregion
                yield return null;
                #region Developer
				string developer = cardData.developer;
                if (developer != cardData.publisher)
                    developer += $" | {cardData.publisher}";
                devPub.text = developer;
                #endregion
                yield return null;
                #region Price
				string priceText = $"€{cardData.price}";
                if (cardData.price == 0)
                    priceText = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
                price.text = priceText;
				#endregion
			}
		}
    }
}