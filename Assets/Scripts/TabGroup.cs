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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NALStudio.UI
{
	public class TabGroup : MonoBehaviour
    {
        #region Variables

        public List<TabButton> tabButtons;
		public List<GameObject> tabs;

		public int defaultIndex = 0;

        [HideInInspector]
        public TabButton SelectedButton;

		TabButton mouseInside;

		public delegate void TabAction();
		public event TabAction OnTabSwitch;

		#endregion

		void Awake()
		{
			SelectedButton = tabButtons[defaultIndex];
			foreach (TabButton button in tabButtons)
				button.Subscribe(this);
			for (int i = 0; i < tabs.Count; i++)
				tabs[i].SetActive(i == defaultIndex);
#if UNITY_EDITOR
			if (tabButtons.Count != tabs.Count)
				throw new ArgumentException("TabButton count is not equal to Tab count.");
			else if (defaultIndex >= tabButtons.Count || defaultIndex < 0)
				throw new IndexOutOfRangeException("DefaultIndex is invalid. " +
					$"Allowed range 0 - {tabButtons.Count - 1}, DefaultIndex: {defaultIndex}");
#endif
		}

		public void SwitchTab(TabButton tabButton)
		{
			SelectedButton = tabButton;
			for (int i = 0; i < tabButtons.Count; i++)
				tabs[i].SetActive(tabButtons[i] == tabButton);
			OnTabSwitch?.Invoke();
		}
	}
}
