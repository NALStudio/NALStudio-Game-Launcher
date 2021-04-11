using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClearFilter : MonoBehaviour
{
	public TMP_InputField inputField;
	public Button button;

	void Start()
	{
		inputField.onValueChanged.AddListener(ValueChange);
		button.onClick.AddListener(Clear);
	}

	void ValueChange(string value)
	{
		button.interactable = !string.IsNullOrEmpty(value);
	}

	void Clear()
	{
		inputField.text = string.Empty;
	}
}
