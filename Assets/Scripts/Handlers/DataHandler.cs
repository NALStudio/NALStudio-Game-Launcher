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

using NALStudio.GameLauncher.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class DataHandler : MonoBehaviour
{
	static List<UniversalData> datas = new List<UniversalData>();
	public static class UniversalDatas
	{
		public static bool Loaded { get; set; }

		public static IReadOnlyCollection<UniversalData> Get()
		{
			return datas;
		}

		public static UniversalData Get(string uuid)
		{
			return datas.Find(d => d.UUID == uuid);
		}

		public static void Clear()
		{
			datas.Clear();
		}

		public static void Add(UniversalData toAdd)
		{
			while (datas.Any(d => d.UUID == toAdd.UUID))
				datas.RemoveAt(datas.FindIndex(d => d.UUID == toAdd.UUID));
			datas.Add(toAdd);
		}
	}

	public static DataHandler Executor { get; private set; }

	void Awake()
	{
		if (Executor == null)
			Executor = this;
		else
			Debug.LogError("Only one DataHandler is allowed per scene!");
	}
#if UNITY_EDITOR
	void Reset()
	{
		UnityEditor.MonoScript monoScript = UnityEditor.MonoScript.FromMonoBehaviour(this);
		UnityEditor.MonoImporter.SetExecutionOrder(monoScript, -34);
	}
#endif
}

[Serializable]
public class UniversalData
{
	#region Data
	public class LocalData
	{
		public string Version { get; set; }
		public string ExecutablePath { get; set; }
		public string LocalsPath { get; private set; }
		public string UUID { get; private set; }
		public long LastInterest { get; set; }

		public UniversalData Parent { get; private set; }

		[Serializable]
		class JsonLocalData
		{
			public string uuid;
			public string version;
			public string executable_path;
			public long last_interest;
		}

		public void Save()
		{
			string dir = Path.Combine(LocalsPath, Constants.launcherDataFilePath);
			if (!Directory.Exists(dir))
			{
				try
				{
					Directory.CreateDirectory(dir);
				}
				catch (Exception e)
				{
					Debug.LogError($"Could not create gamedata directory for \"{UUID}\" at path \"{dir}\". Errors will follow. Exception Message: {e.Message}");
				}
			}
			try
			{
				string json = ToJson(this);
				File.WriteAllText(Path.Combine(LocalsPath, Constants.gamedataFilePath), json);
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not save gamedata file for \"{UUID}\". Errors will follow. Exception Message: {e.Message}");
			}
		}

		public LocalData(string uuid, string version, string exePath, string localsPath)
		{
			UUID = uuid;
			Parent = DataHandler.UniversalDatas.Get(UUID);

			Version = version;
			ExecutablePath = exePath;
			LocalsPath = localsPath;
			LastInterest = DateTimeOffset.Now.ToUnixTimeSeconds();

			Save();
		}

		private LocalData() { }

		public static LocalData FromData(UniversalData data)
		{
			LocalData loaded = null;

			string searchDir;
			string gdPath;
			if (!SettingsManager.Settings.CustomGamePaths.ContainsKey(data.UUID))
			{
				searchDir = Path.Combine(Constants.GamesPath, data.Name);
				gdPath = Path.Combine(searchDir, Constants.gamedataFilePath);
				if (File.Exists(gdPath))
				{
					string json = File.ReadAllText(gdPath);
					loaded = FromJson(json);
					if (loaded == null || loaded.UUID != data.UUID)
					{
						loaded = null;
						searchDir = Path.Combine(Constants.GamesPath, data.UUID);
						gdPath = Path.Combine(searchDir, Constants.gamedataFilePath);
						if (File.Exists(gdPath))
						{
							json = File.ReadAllText(gdPath);
							loaded = FromJson(json);
							if (loaded != null && loaded.UUID != data.UUID)
							{
								Debug.LogError($"Invalid UUID of \"{loaded.UUID}\" in game: \"{data.UUID}\" ({data.Name}) UUID path.");
								loaded = null;
							}
						}
					}
				}
			}
			else
			{
				searchDir = SettingsManager.Settings.CustomGamePaths[data.UUID];
				gdPath = Path.Combine(searchDir, Constants.gamedataFilePath);
				if (File.Exists(gdPath))
				{
					string json = File.ReadAllText(gdPath);
					loaded = FromJson(json);
					//Null check will be the one below.
				}
				else
				{
					SettingsManager.Settings.CustomGamePaths.Remove(data.UUID);
					SettingsManager.Save();
				}
			}
			if (loaded != null)
			{
				loaded.LocalsPath = searchDir;
				loaded.Parent = data;
			}
			return loaded;
		}

		public static LocalData FromJson(string json)
		{
			try
			{
				JsonLocalData j = JsonUtility.FromJson<JsonLocalData>(json);
				return new LocalData()
				{
					ExecutablePath = j.executable_path,
					Version = j.version,
					UUID = j.uuid,
					LastInterest = j.last_interest
				};
			}
			catch (ArgumentException e)
			{
				Debug.LogError(e.Message);
				return null;
			}
		}

		static string ToJson(LocalData localData)
		{
			JsonLocalData j = new JsonLocalData
			{
				uuid = localData.UUID,
				version = localData.Version,
				executable_path = localData.ExecutablePath,
				last_interest = localData.LastInterest
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

	public class DiscordData
	{
		public string ImageKey { get; private set; }

		public DiscordData(string imageKey)
		{
			ImageKey = imageKey;
		}
	}

	#region Basic Info
	public string Name { get; private set; }
	public string DisplayName { get; private set; }
	public string UUID { get; private set; }
	#endregion
	#region Store Info
	public bool Hidden { get; private set; }
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

	public LocalData Local { get; set; }

	public RemoteData Remote { get; private set; }

	public DiscordData Discord { get; private set; }
	#endregion

	#region Instancing
	[Serializable]
	class JsonUniversalData
	{
		public string name;
		public string display_name;
		public string uuid;
		public bool hidden;
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
		public string discord_image_key;
	}

	public UniversalData(string json)
	{
		JsonUniversalData d = JsonUtility.FromJson<JsonUniversalData>(json);
		#region Remote
		Remote = new RemoteData(d.version, d.executable_path);
		#endregion
		#region Discord
		Discord = null;
		if (!string.IsNullOrEmpty(d.discord_image_key))
			Discord = new DiscordData(d.discord_image_key);
		#endregion
		#region Data
		Name = d.name;
		DisplayName = d.display_name;
		UUID = d.uuid;
		Hidden = d.hidden;
		Developer = d.developer;
		Publisher = d.publisher;
		Price = d.price;
		ReleaseDate = d.release_date;
		EarlyAccess = d.early_access;
		#region Thumbnail
		if (d.thumbnail_url.StartsWith("https://imgur.com/", StringComparison.OrdinalIgnoreCase))
			d.thumbnail_url = "https://i.imgur.com/" + d.thumbnail_url.Substring(18);
		if (d.thumbnail_url.StartsWith("https://i.imgur.com/", StringComparison.OrdinalIgnoreCase)
			&& !d.thumbnail_url.EndsWith("l.png", StringComparison.OrdinalIgnoreCase))
		{
			d.thumbnail_url = d.thumbnail_url.Insert(d.thumbnail_url.Length - 4, "l");
		}
		ThumbnailUrl = d.thumbnail_url;
		ThumbnailTexture = null;
		DataHandler.Executor.StartCoroutine(LoadThumbnail());
		#endregion
		DownloadUrl = d.download_url;
		Order = d.order;
		#endregion
		#region Local
		Local = LocalData.FromData(this);
		#endregion
	}
	#endregion

	IEnumerator LoadThumbnail()
	{
		UnityWebRequest wr = new UnityWebRequest(ThumbnailUrl);
		DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
		wr.downloadHandler = texDl;
		yield return wr.SendWebRequest();
		if (wr.result == UnityWebRequest.Result.ConnectionError || wr.result == UnityWebRequest.Result.DataProcessingError || wr.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.LogError(wr.error);
		}
		else
		{
			yield return new WaitWhile(() => wr.downloadProgress < 1f);
			ThumbnailTexture = texDl.texture;
		}
	}
}