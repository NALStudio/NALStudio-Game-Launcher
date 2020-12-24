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

using NALStudio.Coroutines;
using NALStudio.GameLauncher.Games;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ArgumentParser : MonoBehaviour
{
	public GameHandler gameHandler;

	void Parse(string arg)
	{
		string[] argSplit = arg.Split('=');
		if (argSplit.Length == 2)
		{
			switch (argSplit[0])
			{
				case "--launch":
					StartCoroutine(LaunchGame(argSplit[1]));
					break;
			}
		}
	}

	void Start()
	{
		foreach (string arg in Environment.GetCommandLineArgs())
		{
			Parse(arg);
		}
	}

	IEnumerator LaunchGame(string GameName)
	{
		yield return new WaitForFrames(100);
		GameHandler.GameData correctData = null;
		foreach (GameHandler.GameData gameData in gameHandler.gameDatas)
		{
			if (gameData.name == GameName)
			{
				correctData = gameData;
				break;
			}
		}
		if (correctData != null)
			gameHandler.StartGame(correctData);
		else
			Debug.LogError($"Game name not found in gameDatas! GameDatas count: {gameHandler.gameDatas.Count}, GameName: {GameName}");
	}
}
