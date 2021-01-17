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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NALStudio.UI
{
	[ExecuteAlways]
	public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        #region Variables
        public Graphic graphic;

        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public float fadeDuration;

        TabGroup tabGroup;
		#endregion

#if UNITY_EDITOR
		void Reset()
		{
			normalColor = new Color(1f, 1f, 1f);
			highlightedColor = new Color(0.75f, 0.75f, 0.75f);
			pressedColor = new Color(0.5f, 0.5f, 0.5f);

			fadeDuration = 0.1f;

			tabGroup = GetComponent<TabGroup>();
		}

		void Update()
		{
			if (!Application.isPlaying)
				graphic.canvasRenderer.SetColor(normalColor);
		}
#endif

		public void Subscribe(TabGroup _tabGroup)
		{
            graphic.canvasRenderer.SetColor(normalColor);
            tabGroup = _tabGroup;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
            graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
        }

		public void OnPointerExit(PointerEventData eventData)
		{
            graphic.CrossFadeColor(normalColor, fadeDuration, true, true);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				graphic.CrossFadeColor(pressedColor, fadeDuration, true, true);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				tabGroup.SwitchTab(this);
		}
	}
}
