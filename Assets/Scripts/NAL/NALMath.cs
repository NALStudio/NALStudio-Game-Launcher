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

using Lean.Localization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NALStudio.Math
{
    public static class Convert
    {
		#region Bytes To MiB
		public static double BytesToMiB(ulong value) => value / (1024d * 1024d);
		#endregion

		#region Bytes (auto)
		// Uses JEDEC standard
		public static string BytesAutoFormatter(double value)
		{
			static string format(double _v, string translationName)
			{
				string translation = translationName != null ? LeanLocalization.GetTranslationText(translationName) : "";
				return $"{_v:0.0}{translation}{LeanLocalization.GetTranslationText("units-byte", "B")}";
			}

			if (value < 1024d)
				return format(value, null);

			double kilo = value / 1024d;
			if (kilo < 1024d)
				return format(kilo, "units-kilo");

			double mega = kilo / 1024d;
			if (mega < 1024d)
				return format(mega, "units-mega");

			double giga = mega / 1024d;
			return format(giga, "units-giga");

		}

		public static string BytePairAutoFormatter(double v1, double v2)
		{
			static string format(double _v1, double _v2, string translationName)
			{
				string unitTanslation = translationName != null ? LeanLocalization.GetTranslationText(translationName) : "";
				string byteTranslation = LeanLocalization.GetTranslationText("units-byte", "B");
				return $"{_v1:0.0}{unitTanslation}{byteTranslation} / {_v2:0.0}{unitTanslation}{byteTranslation}";
			}

			double max = System.Math.Max(v1, v2);
			if (max < 1024d)
				return format(v1, v2, null);

			double kilo = max / 1024d;
			if (kilo < 1024d)
			{
				double div = 1024d;
				return format(v1 / div, v2 / div, "units-kilo");
			}

			double mega = kilo / 1024d;
			if (mega < 1024d)
			{
				double div = 1024d * 1024d;
				return format(v1 / div, v2 / div, "units-mega");
			}
			else
			{
				double div = 1024d * 1024d * 1024d;
				return format(v1 / div, v2 / div, "units-giga");
			}
		}
		#endregion

		#region Time
		public static string MinutesToReadable(double minutes)
		{
			double time = minutes;
			string playtimeFormat = LeanLocalization.GetTranslationText("units-minutes", "Minutes");
			if (time >= 60)
			{
				time /= 60f;
				playtimeFormat = LeanLocalization.GetTranslationText("units-hours", "Hours");
			}
			return $"{time:0.0} {playtimeFormat}";
		}

		public static string MinutesToReadable(float minutes)
		{
			return MinutesToReadable((double)minutes);
		}
		#endregion
	}
}
