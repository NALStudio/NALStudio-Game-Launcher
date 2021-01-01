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
	[HideInInspector]
	public List<NewsBanner> banners;
	[HideInInspector]
	public List<NewsButton> buttons;

	int index = 0;

	void Start()
	{
		NewsBanner bannerScript = banner.GetComponent<NewsBanner>();
		NewsButton buttonScript = button.GetComponent<NewsButton>();
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
				bannerScript.ratioFitter.aspectRatio = bannerScript.background.sprite.texture.width / (float)bannerScript.background.sprite.texture.height;
			}
			bannerScript.title.text = titles[i];
			bannerScript.title.color = textColors[i];
			bannerScript.text.text = texts[i];
			bannerScript.text.color = textColors[i];
			GameObject ban = Instantiate(banner, bannerParent);
			banners.Add(ban.GetComponent<NewsBanner>());
			#endregion
			#region Button Handling
			buttonScript.logo.sprite = buttonLogos[i];
			if (buttonScript.logo.sprite == null)
				buttonScript.logo.color = new Color(0, 0, 0, 0);
			else
				buttonScript.logo.color = Color.white;
			buttonScript.gameName.text = gameNames[i];
			buttonScript.progress.maxValue = bannerSwitchDelay;
			GameObject but = Instantiate(button, buttonParent);
			buttons.Add(but.GetComponent<NewsButton>());
			#endregion
		}
		Destroy(banner);
		Destroy(button);
	}

	void Update()
	{
		NewsButton button = buttons[index];
		if (button.progress.value < button.progress.maxValue)
		{
			button.progress.value += Time.deltaTime;
			for (int i = 0; i < banners.Count; i++)
				banners[i].gameObject.SetActive(i == index);

		}
		else
		{
			button.progress.value = 0f;
			index++;
			if (index >= buttons.Count)
				index = 0;
		}
	}
}
