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
using System.IO;
using UnityEngine;

namespace NALStudio.Logging
{
	public class LogHandler : MonoBehaviour
	{
		string logDir;
		string logFilePath;

		IEnumerator DelayedInit()
		{
			yield return new WaitUntil(() => SettingsManager.Loaded);

			Init();
		}

		void Init()
		{
			if (SettingsManager.Settings.DisableLogging)
				return;

			logDir = Path.Combine(Application.persistentDataPath, "logs");
			if (!Directory.Exists(logDir))
				Directory.CreateDirectory(logDir);
			logFilePath = Path.Combine(logDir, "log_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".log");
			string[] logFiles = Directory.GetFiles(logDir);
			for (int i = 0; i < logFiles.Length - 4; i++)
				File.Delete(logFiles[i]);

			string initString = $@"Created log file: {logFilePath}
I==========[ SYSTEM INFO ]==========I
- Window:       {Screen.width}x{Screen.height}@{Screen.currentResolution.refreshRate} [{Screen.dpi}dpi]
- Graphics API: {SystemInfo.graphicsDeviceVersion}
- GPU:          {SystemInfo.graphicsDeviceName}
- VRAM:         {SystemInfo.graphicsMemorySize}MB. Max texture size: {SystemInfo.maxTextureSize}px. Shader level: {SystemInfo.graphicsShaderLevel}
- CPU:          {SystemInfo.processorType} [{SystemInfo.processorCount} cores]
- RAM:          {SystemInfo.systemMemorySize}MB
- OS:           {SystemInfo.operatingSystem} [{SystemInfo.deviceType}]
I==========[ SYSTEM INFO ]==========I";
			using (StreamWriter sw = new StreamWriter(logFilePath))
			{
				sw.WriteLine(initString);
			}
		}

		void Awake()
		{
			StartCoroutine(DelayedInit());
			Application.logMessageReceivedThreaded += Log;
		}

		void Log(string logString, string stackTrace, LogType logType)
		{
			if (SettingsManager.Settings.DisableLogging)
				return;

			if (logFilePath == null)
			{
				StopAllCoroutines();
				Init();
			}

			string typeString = logType switch
			{
				LogType.Error => "ERROR",
				LogType.Assert => "ASSERT",
				LogType.Warning => "WARNING",
				LogType.Log => "DEBUG",
				LogType.Exception => "EXCEPTION",
				_ => "null",
			};

			using (StreamWriter sw = new StreamWriter(logFilePath, true))
			{
				sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") +
					$"-{typeString}: {logString}\n    {stackTrace.Replace("\n", "\n    ")}");
			}
		}
	}
}
