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

using System.Security.Cryptography;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NALStudio.Encryption
{
    public static class EncryptionHelper
    {
        public static string DecryptString(string encrypred)
		{
			try
			{
                byte[] bytes = Convert.FromBase64String(encrypred);
                return Encoding.UTF32.GetString(bytes);
			}
            catch (FormatException fe)
			{
                Debug.LogError(fe.Message);
                return "";
			}
		}

        public static string EncryptString(string toEncrypt)
		{
            byte[] bytes = Encoding.UTF32.GetBytes(toEncrypt);
            return Convert.ToBase64String(bytes);
		}

        public static string ToMD5Hex(string input)
		{
            byte[] bytes;
            using (MD5 md5 = MD5.Create())
                bytes = md5.ComputeHash(Encoding.UTF32.GetBytes(input));
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                result.Append(b.ToString("x2"));
            return result.ToString();
		}
    }
}
