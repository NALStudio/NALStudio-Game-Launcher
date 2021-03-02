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
#if UNITY_EDITOR
using UnityEditor;
#endif

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
	[TextArea]
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
	[HideInInspector]
	public GameObject runtimeStartFixer;

	int index = 0;

	public void Generate()
	{
		while (bannerParent.transform.childCount > 0)
			DestroyImmediate(bannerParent.transform.GetChild(0).gameObject);
		while (buttonParent.transform.childCount > 0)
			DestroyImmediate(buttonParent.transform.GetChild(0).gameObject);

		banners.Clear();
		buttons.Clear();

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
			buttons.Add(butComp);
			#endregion
		}
		runtimeStartFixer = Instantiate(startFixer, bannerParent);
		runtimeStartFixer.transform.SetAsLastSibling();
	}

	void OnEnable()
	{
		runtimeStartFixer.transform.SetAsLastSibling();
		foreach (NewsButton b in buttons)
			b.background.canvasRenderer.SetColor(buttonColor);
	}

	void Update()
	{
		NewsButton button = buttons[index];
		if (button.progress.value < button.progress.maxValue)
		{
			button.Select();
			button.progress.value += Time.deltaTime;
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
			button.Unselect();
			index++;
			if (index >= buttons.Count)
				index = 0;
		}
	}

	public void SetPage(NewsButton newsButton)
	{
		if (newsButton == buttons[index])
			return;
		if (buttons.IndexOf(newsButton) == banners.IndexOf(lastBanner))
			lastBanner.gameObject.SetActive(false);
		lastBanner = banners[index];
		NewsButton button = buttons[index];
		button.Unselect();
		index = buttons.IndexOf(newsButton);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(NewsGroup))]
class NewsGroupEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GUILayout.Space(25f);
		NewsGroup t = (NewsGroup)target;
		if (GUILayout.Button("Generate!"))
			t.Generate();
	}
}
#endif