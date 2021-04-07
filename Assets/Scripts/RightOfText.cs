using Lean.Localization;
using System.Collections;
using TMPro;
using UnityEngine;

public class RightOfText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public RectTransform toMove;
    public float offset;
    public bool inheritXPos;

    void Start()
    {
		LeanLocalization.OnLocalizationChanged += DelayedUpdatePosition;
        DelayedUpdatePosition();
    }

	void OnDestroy()
	{
        LeanLocalization.OnLocalizationChanged -= DelayedUpdatePosition;
	}

    public void UpdatePosition()
	{
        float x = text.textBounds.size.x + offset;
        if (inheritXPos)
            x += text.rectTransform.anchoredPosition.x;
        toMove.anchoredPosition = new Vector2(x, toMove.anchoredPosition.y);
    }

	public void DelayedUpdatePosition()
	{
        if (gameObject.activeInHierarchy)
            StartCoroutine(Delay());
	}

    IEnumerator Delay()
	{
        yield return null;
        UpdatePosition();
    }
}
