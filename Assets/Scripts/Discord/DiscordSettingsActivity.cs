using NALStudio.GameLauncher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordSettingsActivity : MonoBehaviour
{
    bool wasDefault;

	void OnEnable()
	{
        wasDefault = DiscordHandler.IsDefaultActivity;

        if (wasDefault)
        {
            DiscordHandler.SetActivity(new Discord.Activity
            {
                State = "Configuring Settings",
                Timestamps = new Discord.ActivityTimestamps { Start = DateTimeOffset.Now.ToUnixTimeSeconds() },
                Assets = new Discord.ActivityAssets
                {
                    LargeImage = "settings",
                },
                Instance = false
            });
        }
    }

	void OnDisable()
	{
		if (wasDefault)
		{
            DiscordHandler.ResetActivity();
        }
	}
}
