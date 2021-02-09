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
		static readonly string gamesPath = Path.GetFullPath("Games").Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		public static string GamesPath
		{
			get
			{
				if (!Directory.Exists(gamesPath))
					Directory.CreateDirectory(gamesPath);
				return gamesPath;
			}
		}

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
	}
}
