using System.Collections;
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
        public UniversalData data;
        public RawImage thumbnail;
        public TextMeshProUGUI title;
        public TextMeshProUGUI devPub;
        public TextMeshProUGUI price;

        [HideInInspector]
        public StorePage storePage;

        public void LoadAssets(UniversalData _data)
        {
            data = _data;
            StartCoroutine(SetContent());
        }

        public void OpenStorePage()
        {
            storePage.Open(data);
        }

        void LanguageChange()
		{
            if (data.Price == 0)
                price.text = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
        }

		void Start()
		{
            Lean.Localization.LeanLocalization.OnLocalizationChanged += LanguageChange;
		}

		IEnumerator SetContent()
        {
            yield return new WaitWhile(() => data.ThumbnailTexture == null);
			#region Thumbnail
            thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = data.ThumbnailTexture.width / (float)data.ThumbnailTexture.height;
            thumbnail.texture = data.ThumbnailTexture;
            #endregion
            yield return null;
            #region Title
			title.text = data.DisplayName;
            #endregion
            yield return null;
            #region Developer
			devPub.text = data.Developer == data.Publisher ? data.Developer : $"{data.Developer} | {data.Publisher}";
            #endregion
            yield return null;
            #region Price
			string priceText = $"€{data.Price}";
            if (data.Price == 0)
                priceText = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
            price.text = priceText;
			#endregion
		}
    }
}