using System.Collections;
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

        public void LoadAssets(CardHandler.CardData cData)
        {
            cardData = cData;
            StartCoroutine(SetContent());
        }

        public void OpenStorePage()
        {

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
            if (wr.isNetworkError || wr.isHttpError)
            {
                Debug.LogError(wr.error);
            }
            else
            {
                Texture2D t = texDl.texture;
                thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = t.width / t.height;
                thumbnail.texture = t;

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