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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NALStudio.UI
{
    [ExecuteAlways]
    public class ToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        #region Variables
        public Graphic graphic;
        [Space(10)]
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public float fadeDuration;
        bool _isOn;
        public bool IsOn
		{
			get
			{
                return _isOn;
			}
			set
			{
                _isOn = value;
                if (_isOn)
                {
                    graphic.CrossFadeColor(selectedColor, fadeDuration, true, true);
                    onSelected?.Invoke();
                }
                else
                {
                    graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
                    onDeselected?.Invoke();
                }
            }
		}
        [Space(10)]
        public UnityEvent onSelected;
        public UnityEvent onDeselected;
        #endregion

#if UNITY_EDITOR
        void Reset()
        {
            graphic = GetComponent<Image>();
            normalColor = new Color(1f, 1f, 1f);
            highlightedColor = new Color(0.75f, 0.75f, 0.75f);
            pressedColor = new Color(0.5f, 0.5f, 0.5f);
            selectedColor = new Color(0.75f, 0.75f, 0.75f);
            fadeDuration = 0.1f;
            IsOn = false;
        }
#endif

        void Start()
        {
            if (IsOn)
                graphic.canvasRenderer.SetColor(selectedColor);
            else
                graphic.canvasRenderer.SetColor(normalColor);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsOn)
                graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsOn)
                graphic.CrossFadeColor(normalColor, fadeDuration, true, true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            graphic.CrossFadeColor(pressedColor, fadeDuration, true, true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IsOn = !IsOn;
        }
    }
}