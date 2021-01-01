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

public class NewsGroup : MonoBehaviour
{
	[Header("Data")]
	public List<Sprite> bannerLogos;
	public List<Sprite> bannerBackgrounds;
	public List<string> gameNames;
	public List<Sprite> buttonLogos;
	public Color buttonColor;
	public Color ButtonHighlightedColor;
	public List<Color> buttonColors;
	public List<string> titles;
	public List<string> texts;
	public List<Color> textColors;
	[Header("Settings")]
	public Transform bannerParent;
	public GameObject banner;
	public Transform buttonParent;
	public GameObject button;
	public float bannerSwitchDelay;
	[Header("Parameters")]
	public GameObject startFixer;

	[HideInInspector]
	public List<NewsBanner> banners;
	[HideInInspector]
	public NewsBanner lastBanner;
	[HideInInspector]
	public List<NewsButton> buttons;

	int index = 0;

	void Start()
	{
		NewsBanner bannerScript = banner.GetComponent<NewsBanner>();
		NewsButton buttonScript = button.GetComponent<NewsButton>();

		buttonScript.normalColor = buttonColor;
		buttonScript.highlightedColor = ButtonHighlightedColor;
		buttonScript.progress.maxValue = bannerSwitchDelay;
		buttonScript.newsGroup = this;
		for (int i = 0; i < bannerLogos.Count; i++)
		{
			#region Banner Handling
			bannerScript.logo.sprite = bannerLogos[i];
			if (bannerScript.logo.sprite == null)
				bannerScript.logo.color = new Color(0, 0, 0, 0);
			else
				bannerScript.logo.color = Color.white;
			bannerScript.background.sprite = bannerBackgrounds[i];
			if (bannerScript.background.sprite == null)
			{
				bannerScript.background.color = new Color(0, 0, 0, 0);
			}
			else
			{
				bannerScript.background.color = Color.white;
				bannerScript.ratioFitter.aspectRatio = bannerBackgrounds[i].bounds.size.x / bannerBackgrounds[i].bounds.size.y;
			}
			bannerScript.title.text = titles[i];
			bannerScript.title.color = textColors[i];
			bannerScript.text.text = texts[i];
			bannerScript.text.color = textColors[i];
			GameObject ban = Instantiate(banner, bannerParent);
			ban.SetActive(false);
			banners.Add(ban.GetComponent<NewsBanner>());
			#endregion
			#region Button Handling
			buttonScript.logo.sprite = buttonLogos[i];
			if (buttonScript.logo.sprite == null)
				buttonScript.logo.color = new Color(0, 0, 0, 0);
			else
				buttonScript.logo.color = Color.white;
			buttonScript.selectedColor = buttonColors[i];
			buttonScript.gameName.text = gameNames[i];
			GameObject but = Instantiate(button, buttonParent);
			but.SetActive(true);
			NewsButton butComp = but.GetComponent<NewsButton>();
			butComp.background.canvasRenderer.SetColor(buttonColor);
			buttons.Add(butComp);
			#endregion
		}
		Destroy(banner);
		Destroy(button);
		startFixer.transform.SetAsLastSibling();
	}

	void OnEnable()
	{
		startFixer.transform.SetAsLastSibling();
	}

	void Update()
	{
		NewsButton button = buttons[index];
		if (button.progress.value < button.progress.maxValue)
		{
			button.progress.value += Time.deltaTime;
			button.Highlight();
			for (int i = 0; i < banners.Count; i++)
			{
				if (i == index)
				{
					banners[i].gameObject.SetActive(true);
					banners[i].transform.SetAsLastSibling();
				}
				else if (banners[i] == lastBanner)
				{
					banners[i].gameObject.SetActive(true);
				}
				else
				{
					banners[i].gameObject.SetActive(false);
				}
			}

		}
		else if (!button.pointerInside)
		{
			lastBanner = banners[index];
			button.progress.value = 0f;
			button.UnHighlight();
			index++;
			if (index >= buttons.Count)
				index = 0;
		}
	}

	public void SetPage(NewsButton newsButton)
	{
		if (buttons.IndexOf(newsButton) == banners.IndexOf(lastBanner))
			lastBanner.gameObject.SetActive(false);
		lastBanner = banners[index];
		NewsButton button = buttons[index];
		index = buttons.IndexOf(newsButton);
		button.progress.value = 0f;
		button.UnHighlight();
	}
}
