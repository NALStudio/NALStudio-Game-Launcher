using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HighlightOnMouseover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Graphic graphic;
	public Color normalColor;
	public Color highlightedColor;
	public float fadeDuration;

#if UNITY_EDITOR
	void Reset()
	{
		graphic = GetComponent<Graphic>();
		normalColor = new Color(1f, 1f, 1f);
		highlightedColor = new Color(0.75f, 0.75f, 0.75f);

		fadeDuration = 0.1f;
	}

	void Update()
	{
		if (!Application.isPlaying)
			graphic.canvasRenderer.SetColor(normalColor);
	}
#endif

	public void OnPointerEnter(PointerEventData eventData)
	{
		graphic.CrossFadeColor(highlightedColor, fadeDuration, true, true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		graphic.CrossFadeColor(normalColor, fadeDuration, true, true);
	}
}
