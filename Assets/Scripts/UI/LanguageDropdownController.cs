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
using Lean.Localization;
using TMPro;

public class LanguageDropdownController : MonoBehaviour
{
    #region Variables
    public TMP_Dropdown dropdown;
    public LeanPhrase phrase;
    List<string> languages = new List<string>();
	#endregion

    [System.Serializable]
    class testi
	{
        public string thingy = Random.Range(0, 1000).ToString();
        public int parameter = Random.Range(0, 11);
	}

    void Start()
    {
        foreach (KeyValuePair<string, LeanLanguage> tmpLang in LeanLocalization.CurrentLanguages)
            languages.Add(tmpLang.Key);
        List<string> strEntries = new List<string>();
        foreach (LeanPhrase.Entry entry in phrase.Entries)
            strEntries.Add(entry.Text);
        dropdown.AddOptions(strEntries);
        dropdown.onValueChanged.AddListener(LanguageChange);
        dropdown.value = languages.IndexOf(LeanLocalization.CurrentLanguage);
    }

    void LanguageChange(int i)
	{
        LeanLocalization.CurrentLanguage = languages[i];
	}
}
