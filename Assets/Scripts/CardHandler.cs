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
using UnityEngine.UI;

[ExecuteAlways]
public class CardHandler : MonoBehaviour
{
    #region Variables

    public float gridHeight;
	public float verticalSpacing;

    GridLayoutGroup gridLayout;
    RectTransform rectTranform;

	#endregion

    void Start()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTranform = GetComponent<RectTransform>();
		CalculateCellSize();
    }

#if UNITY_EDITOR
	void Reset()
	{
		gridHeight = 200f;
		verticalSpacing = 10f;
		Start();
	}

	void Update()
	{
		if (!Application.isPlaying)
			CalculateCellSize();
	}
#endif

	void CalculateCellSize()
	{
        gridLayout.cellSize = new Vector2((rectTranform.rect.width - 20) / 3, gridHeight);
		gridLayout.spacing = new Vector2(gridLayout.spacing.x, verticalSpacing);
		rectTranform.sizeDelta = new Vector2(rectTranform.sizeDelta.x,
			gridHeight + verticalSpacing + ((gridHeight + verticalSpacing) * Mathf.CeilToInt((rectTranform.childCount - 1) / 3)));
	}

	void OnRectTransformDimensionsChange()
	{
		if (gridLayout != null)
			CalculateCellSize();
	}
}
