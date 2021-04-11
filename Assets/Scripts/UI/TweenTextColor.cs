using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TweenTextColor : MonoBehaviour
{
	public TextMeshProUGUI text;
	public Color From;
	public Color To;
	public float time;

#if UNITY_EDITOR
	void Reset()
	{
		text = GetComponent<TextMeshProUGUI>();
		From = Color.black;
		To = Color.white;
		time = 0.1f;
	}
#endif

	public void DoTween()
	{
		DoTween(false);
	}

	public void DoTween(bool reverse)
	{
		StopTween();

		Color f = From;
		Color t = To;

		if (reverse)
		{
			f = To;
			t = From;
		}

		LeanTween.value(gameObject, UpdateColor, f, t, time);
	}

	public void StopTween()
	{
		LeanTween.cancel(gameObject);
	}

	void UpdateColor(Color color)
	{
		text.color = color;
	}
}
