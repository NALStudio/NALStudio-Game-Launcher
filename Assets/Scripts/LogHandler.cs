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

		void init()
		{
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
			if (logFilePath == null)
				init();
			Application.logMessageReceivedThreaded += Log;
		}

		void Log(string logString, string stackTrace, LogType logType)
		{
			if (logFilePath == null)
				init();
			string typeString;
			switch (logType)
			{
				case LogType.Error:
					typeString = "ERROR";
					break;
				case LogType.Assert:
					typeString = "ASSERT";
					break;
				case LogType.Warning:
					typeString = "WARNING";
					break;
				case LogType.Log:
					typeString = "DEBUG";
					break;
				case LogType.Exception:
					typeString = "EXCEPTION";
					break;
				default:
					typeString = "null";
					break;
			}
			using (StreamWriter sw = new StreamWriter(logFilePath, true))
			{
				sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") +
					$"-{typeString}: {logString}\n    {stackTrace.Replace("\n", "\n    ")}");
			}
		}
	}
}
