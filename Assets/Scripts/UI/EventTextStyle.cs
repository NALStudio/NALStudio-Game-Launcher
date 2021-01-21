using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventTextStyle : MonoBehaviour
{
    #region Variables
    public TextMeshProUGUI text;
    public FontStyles normalStyle;
    public FontStyles highlightedStyle;
    #endregion

#if UNITY_EDITOR
    void Reset()
    {
        text = GetComponent<TextMeshProUGUI>();
        normalStyle = FontStyles.Normal;
        highlightedStyle = FontStyles.Underline;
    }
#endif

    public void SetNormal()
	{
        text.fontStyle = normalStyle;
	}

    public void SetHighlighted()
	{
        text.fontStyle = highlightedStyle;
	}

    public void Set(bool highlighted)
	{
        switch (highlighted)
		{
            case false:
                SetNormal();
                break;
            case true:
                SetHighlighted();
                break;
		}
	}
}
