using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventTextColor : MonoBehaviour
{
    #region Variables
    public TextMeshProUGUI text;
    public Color normalColor;
    public Color highlightedColor;
    #endregion

#if UNITY_EDITOR
    void Reset()
    {
        text = GetComponent<TextMeshProUGUI>();
        normalColor = new Color(1f, 1f, 1f);
        highlightedColor = new Color(0.75f, 0.75f, 0.75f);
    }
#endif

    public void SetNormal()
    {
        text.color = normalColor;
    }

    public void SetHighlighted()
    {
        text.color = highlightedColor;
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
