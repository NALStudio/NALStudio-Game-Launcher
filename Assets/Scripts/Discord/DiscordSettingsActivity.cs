using NALStudio.GameLauncher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordSettingsActivity : MonoBehaviour
{
	void OnEnable()
	{
        DiscordHandler.SetActivity(new Discord.Activity
        {
            State = "Idling",
            Timestamps = new Discord.ActivityTimestamps { Start = DateTimeOffset.Now.ToUnixTimeSeconds() },
            Assets = new Discord.ActivityAssets
            {
                LargeImage = "settings",
            },
            Instance = false
        });
    }
}
