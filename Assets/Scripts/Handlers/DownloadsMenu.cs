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
using NALStudio.UI;
using UnityEngine.UI;
using TMPro;
using NALStudio.GameLauncher;
using Lean.Localization;

public class DownloadsMenu : MonoBehaviour
{
	public DownloadHandler downloadHandler;
	public UITweener tweener;
	public UITweener inputIntercepterTweener;
	public CanvasGroup canvasGroup;
	public ToggleButton toggler;

	public bool opened;
	public bool trueOpened;

	void Start()
	{
		tweener.OnComplete += () => opened = trueOpened;
		inputIntercepterTweener.OnComplete += () => opened = trueOpened;
	}

	public void Open()
	{
		if (trueOpened)
			return;
		opened = true;
		trueOpened = true;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		tweener.DoTween();
		inputIntercepterTweener.StopTween();
		if (inputIntercepterTweener.gameObject.activeSelf)
			inputIntercepterTweener.gameObject.SetActive(false);
		inputIntercepterTweener.gameObject.SetActive(true);
	}

	public void Close()
	{
		if (!trueOpened)
			return;
		trueOpened = false;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		tweener.DoTween(true);
		inputIntercepterTweener.DoTween(true, true);
	}
}
