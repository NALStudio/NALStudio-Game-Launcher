using UnityEngine;
using NALStudio.UI;

namespace Lean.Localization
{
	/// <summary>This component will update a NALTooltip's content with localized text, or use a fallback if none is found.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NALTooltipTrigger))]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized NALTooltipContent")]
	public class LeanLocalizedNALTooltipContent : LeanLocalizedBehaviour
	{
		[Tooltip("If PhraseName couldn't be found, this text will be used"), TextArea]
		public string FallbackText;

		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the NALTooltipTrigger component attached to this GameObject
			var tooltip = GetComponent<NALTooltipTrigger>();

			// Use translation?
			if (translation != null && translation.Data is string)
			{
				tooltip.content = LeanTranslation.FormatText((string)translation.Data, tooltip.content, this, gameObject);
			}
			// Use fallback?
			else
			{
				tooltip.content = LeanTranslation.FormatText(FallbackText, tooltip.content, this, gameObject);
			}
		}

		protected virtual void Awake()
		{
			// Should we set FallbackText?
			if (string.IsNullOrEmpty(FallbackText) == true)
			{
				// Get the TextMeshProUGUI component attached to this GameObject
				var tooltip = GetComponent<NALTooltipTrigger>();

				// Copy current text to fallback
				FallbackText = tooltip.content;
			}
		}
	}
}