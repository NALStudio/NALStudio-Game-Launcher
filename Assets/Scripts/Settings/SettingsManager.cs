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

using NALStudio.GameLauncher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.RemoteConfig;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public enum ConfigManagerEnvironment
	{
        Auto,
        Production,
        Testing
    }

    public ConfigManagerEnvironment Environment;
    static ConfigManagerEnvironment environmentOverride;

    [Serializable]
    class SettingsException : Exception
    {
        public SettingsException() : base() { }

        public SettingsException(string message) : base(message) { }

        public SettingsException(string message, Exception innerException) : base(message, innerException) { }
    }

    static string path = string.Empty;

    public static SettingsHolder Settings = null;
    public static bool Loaded { get; private set; }
    static SettingsManager active = null;

    public static bool RemoteLoaded { get; private set; }
    public struct userAttributes { }
    public struct appAttributes { }

    void Awake()
    {
        if (active == null)
            active = this;
        else
            throw new SettingsException("Only one Settings Manger allowed per scene!");

        path = Path.Combine(Application.persistentDataPath, "settings.nal");
        Load();
        Loaded = true;

        ConfigManager.FetchCompleted += (resp) =>
        {
            RemoteLoaded = true;
            switch (resp.requestOrigin)
            {
                case ConfigOrigin.Default:
                    Debug.LogError("No remote data loaded! (Default values will be used.)");
                    break;
                case ConfigOrigin.Cached:
                    Debug.LogWarning("Using cached remote data.");
                    break;
            }
        };

        environmentOverride = Environment;
        ReloadRemote();
    }

    public static void ReloadRemote()
    {
#if UNITY_EDITOR
        if (environmentOverride == ConfigManagerEnvironment.Production || (environmentOverride == ConfigManagerEnvironment.Auto && !Debug.isDebugBuild))
#else
        if (!Debug.isDebugBuild)
#endif
            ConfigManager.SetEnvironmentID("08fedb44-fa53-49fc-88dd-81bbbcc55193");
        else
            ConfigManager.SetEnvironmentID("03dfe905-cd6b-47f5-8a2d-ca5087b6e065");

        RemoteLoaded = false;
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

#if UNITY_EDITOR
    void Reset()
    {
        UnityEditor.MonoScript monoScript = UnityEditor.MonoScript.FromMonoBehaviour(this);
        UnityEditor.MonoImporter.SetExecutionOrder(monoScript, -69);
    }
#endif

    [Serializable]
    public class SettingsHolder
    {
        public Dictionary<string, string> CustomGamePaths { get; set; } = new Dictionary<string, string>();
        public bool DisableLogging { get; set; } = false;
        public bool AllowInstallsDuringGameplay { get; set; } = true;
        public bool LimitFPS { get; set; } = true;
        public bool LowPerfMode { get; set; } = false;
        public bool EnableDiscordIntegration { get; set; } = true;
        public bool AllowShortcuts { get; set; } = true;
    }

    public static void Save()
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(Settings, Formatting.Indented));
    }

    public static void Load()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No settings file found. Creating a new one...");
            Settings = new SettingsHolder();
            Save();
        }
        else
        {
            try
            {
                Settings = JsonConvert.DeserializeObject<SettingsHolder>(File.ReadAllText(path));
            }
            catch
            {
                Settings = new SettingsHolder();
                Save();
            }
        }
    }

    public static void ResetSettings()
    {
        Settings = new SettingsHolder();
        Save();
    }

    void OnApplicationQuit()
    {
        Save();
    }
}
