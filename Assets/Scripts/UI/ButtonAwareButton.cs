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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAwareButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    #region Variables
    public Graphic graphic;

	public Color normalColor;
    public Color highlightedColor;
    public Color pressedColor;
    public float fadeDuration;

	public bool LEnabled;
	public UnityEvent onLeftClick;
	public bool MEnabled;
	public UnityEvent onMiddleClick;
	public bool REnabled;
	public UnityEvent onRightClick;

	PointerEventData.InputButton[] enabledButtons;
	readonly Dictionary<PointerEventData.InputButton, bool> pressedButtons = new Dictionary<PointerEventData.InputButton, bool> 
	{
		{ PointerEventData.InputButton.Left, false },
		{ PointerEventData.InputButton.Middle, false },
		{ PointerEventData.InputButton.Right, false }
	};

	bool inside;
	bool selected;
	#endregion

	public void IsSelected()
	{
		selected = pressedButtons.Values.Any((bool v) => v) && inside;
	}

#if UNITY_EDITOR
	void Reset()
	{
		graphic = GetComponent<Image>();
		normalColor = new Color(1f, 1f, 1f);
		highlightedColor = new Color(0.75f, 0.75f, 0.75f);
		pressedColor = new Color(0.5f, 0.5f, 0.5f);
		fadeDuration = 0.1f;
	}
#endif

	void Awake()
	{
		List<PointerEventData.InputButton> enabled = new List<PointerEventData.InputButton>();
		if (LEnabled)
			enabled.Add(PointerEventData.InputButton.Left);
		if (MEnabled)
			enabled.Add(PointerEventData.InputButton.Middle);
		if (REnabled)
			enabled.Add(PointerEventData.InputButton.Right);
		enabledButtons = enabled.ToArray();
	}

	void OnEnable()
	{
		graphic.canvasRenderer.SetColor(normalColor);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		inside = true;
		IsSelected();
		if (!selected)
			graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		inside = true;
		IsSelected();
		if (!selected)
			graphic.CrossFadeColor(normalColor, fadeDuration, true, true);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		pressedButtons[eventData.button] = true;
		IsSelected();
		if (inside && enabledButtons.Contains(eventData.button))
			graphic.CrossFadeColor(pressedColor, fadeDuration, true, true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		pressedButtons[eventData.button] = false;
		IsSelected();
		if (inside)
			graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
		else
			graphic.CrossFadeColor(normalColor, fadeDuration, true, true);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
			case PointerEventData.InputButton.Left:
				if (LEnabled)
					onLeftClick?.Invoke();
				break;
			case PointerEventData.InputButton.Middle:
				if (MEnabled)
					onMiddleClick?.Invoke();
				break;
			case PointerEventData.InputButton.Right:
				if (REnabled)
					onRightClick?.Invoke();
				break;
		}
	}
}
