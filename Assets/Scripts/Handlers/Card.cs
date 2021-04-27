using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System;
using NALStudio.Extensions;
using UnityEngine.EventSystems;

namespace NALStudio.GameLauncher.Cards
{
    public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UniversalData data;
        public RawImage thumbnail;
        public Color textLightColor;
        public TextMeshProUGUI title;
        public TextMeshProUGUI devPub;
        public TextMeshProUGUI price;
        [Space(10f)]
        public GameObject gradientObject;
        public float gradientAnimationTime;
        public Color gradientDefaultColor;
        CornerGradient cornerGradient;
        Graphic cornerGradientGraphic;
        Color topLeftColor;
        Color topRightColor;
        Color bottomLeftColor;
        Color bottomRightColor;

        [HideInInspector]
        public StorePage storePage;

        public void LoadAssets(UniversalData _data)
        {
            data = _data;
            SetContent();
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

            cornerGradient = gradientObject.GetComponent<CornerGradient>();
            cornerGradientGraphic = gradientObject.GetComponent<Graphic>();
        }

        void Update()
		{
			if (thumbnail.texture != data.ThumbnailTexture)
                SetThumbnail();
		}

		void SetThumbnail()
		{
            thumbnail.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = data.ThumbnailTexture.width / (float)data.ThumbnailTexture.height;
            thumbnail.texture = data.ThumbnailTexture;

            #region Gradient
            int ttw = data.ThumbnailTexture.width;
            int tth = data.ThumbnailTexture.height;
            // Gradient is for whatever reason flipped and because of that we invert y
            topLeftColor = data.ThumbnailTexture.GetPixel(0, tth - 1);
            topRightColor = data.ThumbnailTexture.GetPixel(ttw - 1, tth - 1);
            bottomLeftColor = data.ThumbnailTexture.GetPixel(0, 0);
            bottomRightColor = data.ThumbnailTexture.GetPixel(ttw - 1, 0);
            // gradient.m_topLeftColor = data.ThumbnailTexture.GetPixels(0, 0, ttw / 2, tth / 2).Mode();
            // gradient.m_topRightColor = data.ThumbnailTexture.GetPixels(ttw / 2, 0, ttw / 2, tth / 2).Mode();
            // gradient.m_bottomLeftColor = data.ThumbnailTexture.GetPixels(0, tth / 2, ttw / 2, tth / 2).Mode();
            // gradient.m_bottomRightColor = data.ThumbnailTexture.GetPixels(ttw / 2, tth / 2, ttw / 2, tth / 2).Mode();
            #endregion
        }

        void SetContent()
        {
            #region Title
			title.text = data.DisplayName;
            #endregion
            #region Developer
			devPub.text = data.Developer == data.Publisher ? data.Developer : $"{data.Developer} | {data.Publisher}";
            #endregion
            #region Price
			string priceText = $"{data.Price}€";
            if (data.Price == 0)
                priceText = Lean.Localization.LeanLocalization.GetTranslationText("pricing-free", "Free");
            price.text = priceText;
            #endregion
            SetThumbnail();
        }

        void AnimationUpdate(float t, float _)
		{
			cornerGradientGraphic.SetVerticesDirty();
            cornerGradient.m_topLeftColor = Color.Lerp(gradientDefaultColor, topLeftColor, t);
            cornerGradient.m_topRightColor = Color.Lerp(gradientDefaultColor, topRightColor, t);
            cornerGradient.m_bottomLeftColor = Color.Lerp(gradientDefaultColor, bottomLeftColor, t);
            cornerGradient.m_bottomRightColor = Color.Lerp(gradientDefaultColor, bottomRightColor, t);
        }

		public void OnPointerEnter(PointerEventData eventData)
		{
            LeanTween.cancel(gradientObject);
            LeanTween.value(gradientObject, AnimationUpdate, 0f, 1f, gradientAnimationTime);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
            LeanTween.cancel(gradientObject);
            LeanTween.value(gradientObject, AnimationUpdate, 1f, 0f, gradientAnimationTime);
		}
	}
}