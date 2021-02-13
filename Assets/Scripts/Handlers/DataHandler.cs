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

[Serializable]
public class UniversalData
{
	#region Static
	static List<UniversalData> datas = new List<UniversalData>();
	public static IReadOnlyCollection<UniversalData> Items
	{
		get
		{
			return datas;
		}
	}
	#endregion

	#region Data
	public class LocalData
	{
		public string version;
		public string executable_path;
	}

	public class RemoteData
	{
		public string version;
		public string executable_path;
	}

	#region Basic Info
	public string Name { get; private set; }
	public string UUID { get; private set; }
	#endregion
	#region Store Info
	public string Developer { get; private set; }
	public string Publisher { get; private set; }
	public string ReleaseDate { get; private set; }
	public bool EarlyAccess { get; private set; }
	public long Price { get; private set; }
	public long Order { get; private set; }
	public string DownloadUrl { get; private set; }
	#endregion
	#region Displaying Info
	public string ThumbnailUrl { get; private set; }
	public Texture2D ThumbnailTexture { get; private set; }
	#endregion

	#region Game Info
	public float Playtime
	{
		get
		{
			return PlayerPrefs.GetFloat($"playtime/{UUID}");
		}
		set
		{
			PlayerPrefs.SetFloat($"playtime/{UUID}", value);
		}
	}
	#endregion

	public LocalData Local { get; private set; }
	public RemoteData Remote { get; private set; }
	
	#endregion
}
