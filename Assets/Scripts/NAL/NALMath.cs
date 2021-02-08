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
	}
}
