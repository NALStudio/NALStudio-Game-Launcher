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
        [Space(10)]
        public bool isOn;
        [Space(10)]
        public GameObject toggleObject;
        public bool tweenOnDeselect;
        UITweener tweener;
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
            isOn = false;
        }

        void Update()
        {
            if (!Application.isPlaying && graphic != null)
                graphic.canvasRenderer.SetColor(normalColor);
        }
#endif

        void Start()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                toggleObject.SetActive(isOn);
                if (tweenOnDeselect)
                    tweener = toggleObject.GetComponent<UITweener>();
#if UNITY_EDITOR
            }
#endif
            if (isOn)
                graphic.canvasRenderer.SetColor(selectedColor);
            else
                graphic.canvasRenderer.SetColor(normalColor);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isOn)
                graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isOn)
                graphic.CrossFadeColor(normalColor, fadeDuration, true, true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            graphic.CrossFadeColor(pressedColor, fadeDuration, true, true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isOn = !isOn;
            if (tweenOnDeselect && !isOn)
                tweener.DoTween(true, true);
            else
                toggleObject.SetActive(isOn);
            if (isOn)
                graphic.CrossFadeColor(selectedColor, fadeDuration, true, true);
            else
                graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
        }
    }
}