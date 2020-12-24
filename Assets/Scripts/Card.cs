using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

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
                cardData.thumbnailTexture = texDl.texture;
                thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = cardData.thumbnailTexture.width / (float)cardData.thumbnailTexture.height;
                thumbnail.texture = cardData.thumbnailTexture;

                title.text = cardData.title;

                string developer = cardData.developer;
                if (developer != cardData.publisher)
                    developer += $" | {cardData.publisher}";
                devPub.text = developer;

                string priceText = $"€{cardData.price}";
                if (cardData.price == 0)
                    priceText = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
                price.text = priceText;
            }
        }
    }
}