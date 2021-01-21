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
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NALStudio.UI
{
    public class NALTooltip : MonoBehaviour
    {
        #region Variables
        public Canvas tooltipCanvas;
        public TextMeshProUGUI headerField;
        public TextMeshProUGUI contentField;
        public LayoutElement layoutElement;
        public RectTransform rectTransform;
        public int characterWrapLimit;
        #endregion

#if UNITY_EDITOR
        void Reset()
		{
            tooltipCanvas = GetComponentInParent<Canvas>();
            rectTransform = GetComponent<RectTransform>();
            layoutElement = GetComponent<LayoutElement>();
            characterWrapLimit = 80;
		}
#endif

        public void SetText(string content, string header = "")
        {
            headerField.gameObject.SetActive(!string.IsNullOrEmpty(header));
            headerField.text = header;

            contentField.text = content;

            bool headerOK = headerField.text.Split('\n').Any(s => s.Length > characterWrapLimit);
            bool contentOK = contentField.text.Split('\n').Any(s => s.Length > characterWrapLimit);
            layoutElement.enabled = headerOK || contentOK;
        }

        void Move()
		{
            Vector2 position = Input.mousePosition;
			if (position.x + rectTransform.sizeDelta.x > tooltipCanvas.pixelRect.width || position.y + rectTransform.sizeDelta.y > tooltipCanvas.pixelRect.height)
				rectTransform.pivot = new Vector2(1f, 1f);
			else
				rectTransform.pivot = new Vector2(0f, 1f);

			transform.position = position;
        }

		void OnEnable()
		{
			Move();
		}

		void Update()
		{
            Move();
        }
	}
}
