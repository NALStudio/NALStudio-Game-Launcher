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
using UnityEngine.EventSystems;

namespace NALStudio.UI
{
	public class NALTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region Variables
		public bool interactable = true;
		public string header;
		[TextArea]
		public string content;
		public float delay = 0.5f;
		#endregion

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (interactable)
				StartCoroutine(Show());
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			StopAllCoroutines();
			NALTooltipSystem.Hide();
		}

		IEnumerator Show()
		{
			yield return new WaitForSeconds(delay);
			NALTooltipSystem.Show(content, header);
		}
	}
}
