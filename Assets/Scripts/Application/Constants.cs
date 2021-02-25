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
using System.IO;

namespace NALStudio.GameLauncher.Constants
{
	public static class Constants
	{
		public static readonly string BaseFolder = Path.GetFullPath(".");
		static readonly string gamesPath = Path.Combine(BaseFolder, "Games").Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		public static string GamesPath
		{
			get
			{
				if (!Directory.Exists(gamesPath))
					Directory.CreateDirectory(gamesPath);
				return gamesPath;
			}
		}
		public static readonly string LaunchPath = Path.Combine(BaseFolder, "launch.exe");

		/*
		static readonly string screenshotsPath = Path.GetFullPath("Screenshots").Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		public static string ScreenshotsPath
		{
			get
			{
				if (!Directory.Exists(screenshotsPath))
					Directory.CreateDirectory(screenshotsPath);
				return screenshotsPath;
			}
		}
		*/

		public static readonly string DownloadPath = Path.Combine(GamesPath, "download");

		public const string launcherDataFilePath = "launcher-data";
		public const string gamedataFilePath = "launcher-data/data.nal";
		public const string gameLaunchFilePath = "launcher-data/launch.exe";
	}
}
