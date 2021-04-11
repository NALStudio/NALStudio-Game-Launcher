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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseoverTextStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	#region Variables
	public bool interactable;
    public TextMeshProUGUI text;
    public FontStyles normalStyle;
    public FontStyles highlightedStyle;
	#endregion

#if UNITY_EDITOR
	void Reset()
	{
		interactable = true;
        text = GetComponent<TextMeshProUGUI>();
        normalStyle = FontStyles.Normal;
        highlightedStyle = FontStyles.Underline;
	}
#endif

	void Start()
    {
        text.fontStyle = normalStyle;
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (interactable)
			text.fontStyle = highlightedStyle;
		else
			text.fontStyle = normalStyle;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		text.fontStyle = normalStyle;
	}

	void OnDisable()
	{
		text.fontStyle = normalStyle;
	}
}
