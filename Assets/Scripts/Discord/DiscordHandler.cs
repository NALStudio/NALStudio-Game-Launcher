using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NALStudio.GameLauncher
{
    public class DiscordHandler : MonoBehaviour
    {
        public static Discord.Discord DiscordClient { get; internal set; }
        public static Discord.ActivityManager ActivityManager { get; private set; }
        public static bool IsDefaultActivity { get; private set; }

        void Start()
        {
            SetClient();
            Networking.NetworkManager.InternetAvailabilityChange += OnConnectionChange;
        }

        public static void SetClient()
		{
            bool enabled = SettingsManager.Settings.EnableDiscordIntegration;

            Debug.Log($"Setting Discord Client... Client Enabled: {enabled}");
            switch (enabled)
			{
                case true:
                    try
                    {
                        DiscordClient = new Discord.Discord(815665697648541718L, (ulong)Discord.CreateFlags.NoRequireDiscord);
                    }
                    catch (Discord.ResultException e)
                    {
                        DiscordClient = null;
                        Debug.LogWarning($"Discord Result Exception. Most likely just that Discord wasn't found. Exception message: \"{e.Message}\"");
                    } // Throws error if Discord is not found.

                    ActivityManager = DiscordClient?.GetActivityManager();

                    ResetActivity(); // Sets the default Rich Presence
                    break;
                case false:
                    RemoveClient();
                    break;
			}
		}

        public static void RemoveClient()
		{
            try
            {
                DiscordClient?.Dispose();
            }
            catch (Discord.ResultException e)
            {
                Debug.LogError(e.Message);
            }

            DiscordClient = null;
            ActivityManager = null;
		}

        void OnConnectionChange(bool internetAccess)
        {
            switch (internetAccess)
            {
                case true:
                    SetClient();
                    break;
                case false:
                    RemoveClient();
                    break;
            }
        }

        void Update()
        {
            try
            {
                DiscordClient?.RunCallbacks();
            }
            catch (Discord.ResultException e)
			{
                DiscordClient = null;
                Debug.Log("Result Exception. Discord client was probably disconnected. Removing Discord Client... Exception Message: " + e.Message);
			}
        }

        public static void SetActivity(UniversalData data, long epochStartTime)
		{
            IsDefaultActivity = false;

            ActivityManager?.RegisterCommand($"nalstudiogamelauncher://rungameid/{data.UUID}");
            ActivityManager?.UpdateActivity(new Discord.Activity
			{
                State = "Playing",
                Details = data.DisplayName,
                Timestamps = new Discord.ActivityTimestamps { Start = epochStartTime },
                Assets = new Discord.ActivityAssets {
                    LargeImage = data.Discord != null ? data.Discord.ImageKey : "monitor",
                    LargeText = data.DisplayName,
                    SmallImage = "info",
                    SmallText = $"Playtime: {NALStudio.Math.Convert.MinutesToReadable(data.Playtime)}"
                },
                Instance = false
            }, (result) =>
            {
                if (result != Discord.Result.Ok)
                    Debug.LogError($"An error occured while updating activity. Error type: {result:F}");
            });
        }

        public static void SetActivity(Discord.Activity activity)
		{
            IsDefaultActivity = false;

            ActivityManager?.UpdateActivity(activity, result =>
            {
                if (result != Discord.Result.Ok)
                    Debug.LogError($"An error occured while updating activity. Error type: {result:F}");
            });
		}

        public static void ResetActivity()
		{
            IsDefaultActivity = true;

            ActivityManager?.RegisterCommand("nalstudiogamelauncher://");
            ActivityManager?.UpdateActivity(new Discord.Activity
            {
                State = "Idling",
                Timestamps = new Discord.ActivityTimestamps { Start = DateTimeOffset.Now.ToUnixTimeSeconds() },
                Assets = new Discord.ActivityAssets
                {
                    LargeImage = "padding",
                    LargeText = "NALStudio Game Launcher"
                },
                Instance = false
            }, (result) =>
            {
                if (result != Discord.Result.Ok)
                    Debug.LogError($"An error occured while updating activity. Error type: {result:F}");
            });
		}

		void OnApplicationQuit()
		{
            DiscordClient?.Dispose();
		}
	}
}