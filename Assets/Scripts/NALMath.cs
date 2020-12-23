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
		public static double BytesToMB(ulong value)
        {
            return (double)value / 1024 / 1024;
        }

        public static float BytesToMB(int value)
		{
            return (float)value / 1024 / 1024;
		}

        public static float BytesToMB(float value)
		{
            return value / 1024 / 1024;
		}

		public static double BytesToMB(double value)
		{
			return value / 1024 / 1024;
		}
		#endregion

		#region Bits To Mb
		public static float BitsToMb(int value)
		{
            return (float)value / 1024 / 1024;
        }
		#endregion
	}
}
