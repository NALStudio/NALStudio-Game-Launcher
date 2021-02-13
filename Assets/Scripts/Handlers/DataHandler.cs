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

using NALStudio.Encryption;
using NALStudio.GameLauncher.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	const string gamedataFilePath = "launcher-data/data.nal";
	#endregion

	#region Data
	public class LocalData
	{
		public string Version { get; private set; }
		public string ExecutablePath { get; private set; }

		public UniversalData Parent { get; private set; }

		[Serializable]
		class JsonLocalData
		{
			public string uuid;
			public string version;
			public string executable_path;
		}

		private LocalData() { }

		public static LocalData FromJson(string json)
		{
			JsonLocalData j = JsonUtility.FromJson<JsonLocalData>(json);
			return new LocalData
			{
				ExecutablePath = j.executable_path,
				Version = j.version,
				Parent = Items.FirstOrDefault(d => d.UUID == j.uuid)
			};
		}

		public static string ToJson(LocalData localData)
		{
			JsonLocalData j = new JsonLocalData
			{
				uuid = localData.Parent.UUID,
				version = localData.Version,
				executable_path = localData.ExecutablePath
			};
			return JsonUtility.ToJson(j);
		}
	}

	public class RemoteData
	{
		public string Version { get; private set; }
		public string ExecutablePath { get; private set; }

		public RemoteData(string version, string exePath)
		{
			Version = version;
			ExecutablePath = exePath;
		}
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

	#region Instancing
	[Serializable]
	class JsonUniversalData
	{
		public string name;
		public string uuid;
		public string developer;
		public string publisher;
		public long price;
		public string release_date;
		public bool early_access;
		public string thumbnail_url;
		public string download_url;
		public long order;

		public string version;
		public string executable_path;
	}

	public UniversalData(string json)
	{
		JsonUniversalData d = JsonUtility.FromJson<JsonUniversalData>(json);
		#region Remote
		Remote = new RemoteData(d.version, d.executable_path);
		#endregion
		#region Data
		Name = d.name;
		UUID = d.uuid;
		Developer = d.developer;
		Publisher = d != null ? d.publisher : d.developer;
		Price = d.price;
		ReleaseDate = d.release_date;
		EarlyAccess = d.early_access;
		ThumbnailUrl = d.thumbnail_url;
		DownloadUrl = d.download_url;
		Order = d.order;
		#endregion
		#region Local
		string gdPath = Path.Combine(Constants.GamesPath, Name, gamedataFilePath);
		if (File.Exists(gdPath))
		{
			string encrypted = File.ReadAllText(gdPath);
			string unencrypted = EncryptionHelper.DecryptString(encrypted);
			Local = LocalData.FromJson(unencrypted);
		}
		#endregion
		datas.Add(this);
	}
	#endregion
}
