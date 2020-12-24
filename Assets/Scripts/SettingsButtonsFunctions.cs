using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsButtonsFunctions : MonoBehaviour
{
	public void ResetWindow() { Screen.SetResolution(1280, 720, FullScreenMode.Windowed); }
	public void OpenLogPath() { Application.OpenURL(Path.Combine(Application.persistentDataPath, "logs")); }
	public void OpenBugReport() { Application.OpenURL("https://github.com/NALStudio/NALStudio-Game-Launcher/issues"); }
}
