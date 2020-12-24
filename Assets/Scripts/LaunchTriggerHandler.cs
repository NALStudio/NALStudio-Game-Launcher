using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LaunchTriggerHandler : MonoBehaviour
{
	void Awake()
	{
		if (!Directory.Exists("LaunchTrigger"))
			Directory.CreateDirectory("LaunchTrigger");
		if (!File.Exists(Path.Combine("LaunchTrigger", "Trigger.exe")))
		{
			File.Copy(Path.Combine(
				Application.streamingAssetsPath, "NALStudioGameLauncherLaunchTrigger.exe"),
				Path.Combine("LaunchTrigger", "Trigger.exe"));
		}
	}
}
