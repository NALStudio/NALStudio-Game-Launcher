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

public class SetAsScreenWidth : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform rect;
    public float offset;
    [Header("Trigger On")]
    public bool awake;
    public bool start;
    public bool update;
    public bool enable;
    public bool disable;

    void DoIt()
	{
        if (rect.sizeDelta.x != Screen.width)
            rect.sizeDelta = new Vector2(Screen.width + offset, rect.sizeDelta.y);
    }

    void Awake()
	{
        if (awake)
            DoIt();
	}

	void Start()
	{
        if (start)
            DoIt();
	}

	void Update()
    {
        if (update)
            DoIt();
    }

	void OnEnable()
	{
        if (enable)
            DoIt();
	}

	void OnDisable()
	{
        if (disable)
            DoIt();
	}
}
