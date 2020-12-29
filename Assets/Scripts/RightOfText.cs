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
		LeanLocalization.OnLocalizationChanged += DelayedUpdatePosition;
        DelayedUpdatePosition();
    }

	void OnDestroy()
	{
        LeanLocalization.OnLocalizationChanged -= DelayedUpdatePosition;
	}

    public void UpdatePosition()
	{
        toMove.anchoredPosition = new Vector2(text.textBounds.size.x + offset, toMove.anchoredPosition.y);
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
