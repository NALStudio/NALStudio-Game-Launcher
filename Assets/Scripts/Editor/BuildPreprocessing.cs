using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using System.IO;
using UnityEditor.Build.Reporting;

public class BuildPreprocessing : IPreprocessBuildWithReport
{
	public int callbackOrder { get { return 0; } }
	public void OnPreprocessBuild(BuildReport target)
	{
		Debug.Log("Started Build Preprocessing...");
		Debug.Log("Removing launch.exe...");
		if (File.Exists("launch.exe"))
		{
			try
			{
				File.Delete("launch.exe");
				Debug.Log("launch.exe deletion succesful.");
			}
			catch
			{
				Debug.LogWarning("launch.exe deletion failed!");
			}
		}
		else
		{
			Debug.Log("No launch.exe file found.");
		}
		Debug.Log("Removing Games directory...");
		if (Directory.Exists("Games"))
		{
			try
			{
				Directory.Delete("Games", true);
				Debug.Log("Games directory deletion succesful.");
			}
			catch
			{
				Debug.LogWarning("Games directory deletion failed!");
			}
		}
		else
		{
			Debug.Log("No Games directory found.");
		}
	}
}
