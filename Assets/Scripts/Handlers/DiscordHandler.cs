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

        void Start()
        {
            SetClient(SettingsManager.Settings.enableDiscordIntegration);
        }

        public static void SetClient(bool enabled)
		{
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

                    ResetRichPresence(); // Sets the default Rich Presence
                    break;
                case false:
                    DiscordClient?.Dispose();
					DiscordClient = null;
                    ActivityManager = null;
                    break;
			}
		}

        void Update()
        {
            DiscordClient?.RunCallbacks();
        }

        public static void SetRichPresence(UniversalData data, long epochStartTime)
		{
            ActivityManager?.RegisterCommand($"nalstudiogamelauncher://rungameid/{data.UUID}");
            ActivityManager?.UpdateActivity(new Discord.Activity
            {
                Details = $"{data.DisplayName}",
                State = "Playing",
                Timestamps = new Discord.ActivityTimestamps { Start = epochStartTime },
                Assets = new Discord.ActivityAssets { LargeImage = data.Discord != null ? data.Discord.ImageKey : "monitor", SmallImage = "info", SmallText = $"Playtime: {NALStudio.Math.Convert.MinutesToReadable(data.Playtime)}" }
            }, (result) =>
            {
                if (result != Discord.Result.Ok)
					Debug.LogError($"Rich presence set failed with result: \"{result:F}\".");
			});
		}

        public static void ResetRichPresence()
		{
            ActivityManager?.RegisterCommand();
            ActivityManager?.UpdateActivity( new Discord.Activity
			{
                State = "Idling",
                Timestamps = new Discord.ActivityTimestamps { Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                Assets = new Discord.ActivityAssets { LargeImage = "padding" }
			}, (result) =>
            {
                if (result != Discord.Result.Ok)
                    Debug.LogError($"Rich presence set failed with result: \"{result:F}\".");
            });
		}

		void OnApplicationQuit()
		{
            DiscordClient?.Dispose();
		}
	}
}