using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMProVersionInfo : MonoBehaviour
{
    public TextMeshProUGUI text;

	void Start()
	{
		text.text = Application.version;
	}
}
