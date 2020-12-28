using Lean.Localization;
using System.Collections;
using TMPro;
using UnityEngine;

public class RightOfText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public RectTransform toMove;
    public float offset;

    void Start()
    {
		LeanLocalization.OnLocalizationChanged += DelayedCalculate;
        DelayedCalculate();
    }

	void OnDestroy()
	{
        LeanLocalization.OnLocalizationChanged -= DelayedCalculate;
	}

	void DelayedCalculate()
	{
        StartCoroutine(Delay());
	}

    IEnumerator Delay()
	{
        yield return null;
        toMove.anchoredPosition = new Vector2(text.textBounds.size.x + offset, toMove.anchoredPosition.y);
    }
}
