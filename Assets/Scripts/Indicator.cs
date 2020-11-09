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

using NALStudio.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public TabGroup tabGroup;
    public RectTransform rectTransform;
    [Range(1, 10)]
    public float moveSpeed;
    bool animationRunning;
    float fromY;
    float toY;
    float t;
    float defaultX;

	void Reset()
	{
        moveSpeed = 1.0f;
	}

	void Awake()
	{
        rectTransform = gameObject.GetComponent<RectTransform>();
        defaultX = rectTransform.anchoredPosition.x;
	}

	void Start()
	{
        rectTransform.anchoredPosition = new Vector2(
            defaultX, tabGroup.SelectedButton.GetComponent<RectTransform>().anchoredPosition.y);
	}

	void OnEnable()
    {
        tabGroup.OnTabSwitch += MoveTab;
    }

	void OnDisable()
	{
        tabGroup.OnTabSwitch -= MoveTab;
	}

	public void MoveTab()
	{
        fromY = rectTransform.anchoredPosition.y;
        toY = tabGroup.SelectedButton.GetComponent<RectTransform>().anchoredPosition.y;
        t = 0.0f;
        animationRunning = true;
	}

	void Update()
	{
		if (animationRunning)
		{
            animationRunning = t < 1;

            float moveToY = Mathf.SmoothStep(fromY, toY, t);

            rectTransform.anchoredPosition = new Vector2(defaultX, moveToY);

            t += moveSpeed * Time.unscaledDeltaTime;
		}
	}
}
