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
		#region Bytes To MB
		public static double BytesToMB(ulong value) => value / (1024d * 1024d);

		public static double BytesToMB(long value) => value / (1024d * 1024d);

		public static double BytesToMB(double value) => value / (1024d * 1024d);

		public static float BytesToMB(int value) => value / (1024f * 1024f);

		public static float BytesToMB(float value) => value / (1024f * 1024f);
		#endregion

		#region Bits To Mb
		public static float BitsToMb(int value)
		{
            return value / (1024f * 1024f);
        }

		public static float BitsToMb(float value)
		{
			return value / (1024f * 1024f);
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
