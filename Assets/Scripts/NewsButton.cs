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
using UnityEngine.UI;

public class NewsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    #region Variables
    [Header("Properties")]
    public Image logo;
    public Graphic background;
    public TextMeshProUGUI gameName;
    public Slider progress;
    [Header("Settings")]
    public float AnimationSpeed;
    [HideInInspector]
    public Color normalColor;
	[HideInInspector]
	public Color highlightedColor;
	[HideInInspector]
    public Color selectedColor;
	[HideInInspector]
	public bool pointerInside;
	[HideInInspector]
	public NewsGroup newsGroup;
	#endregion

	public void Highlight()
	{
        if (background.color != selectedColor)
            background.CrossFadeColor(selectedColor, AnimationSpeed, true, true);
	}

	public void UnHighlight()
	{
        if (background.color != normalColor)
            background.CrossFadeColor(normalColor, AnimationSpeed, true, true);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (pointerInside)
		{
			newsGroup.SetPage(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerInside = true;
		if (background.canvasRenderer.GetColor() != selectedColor)
			background.CrossFadeColor(highlightedColor, AnimationSpeed, true, true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerInside = false;
		if (background.canvasRenderer.GetColor() != normalColor)
			background.CrossFadeColor(normalColor, AnimationSpeed, true, true);
	}
}
