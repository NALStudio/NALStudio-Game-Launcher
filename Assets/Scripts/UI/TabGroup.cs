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
using UnityEngine.Events;

namespace NALStudio.UI
{
	public class TabGroup : MonoBehaviour
    {
        #region Variables

        public List<TabButton> tabButtons;
		public List<GameObject> tabs;
		public List<UnityEvent> onSelect;
		public List<UnityEvent> onDeselect;

		public int defaultIndex = 0;

        [HideInInspector]
        public TabButton SelectedButton;

		TabButton mouseInside;

		public delegate void TabAction();
		public event TabAction OnTabSwitch;

		#endregion

		void Awake()
		{
			foreach (TabButton button in tabButtons)
			{
				button.gameObject.SetActive(true);
				button.Subscribe(this);
			}
			SwitchTab(tabButtons[defaultIndex]);
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
			if (tabButton == SelectedButton)
				return;
			SelectedButton = tabButton;
			for (int i = 0; i < tabButtons.Count; i++)
			{
				if (tabs[i] != null)
					tabs[i].SetActive(tabButtons[i] == tabButton);
				if (tabButtons[i] == tabButton)
					onSelect[i]?.Invoke();
				else
					onDeselect[i]?.Invoke();
			}

			OnTabSwitch?.Invoke();
		}
	}
}
