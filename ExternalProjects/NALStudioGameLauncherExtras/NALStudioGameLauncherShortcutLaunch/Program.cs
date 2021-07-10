/* .NET Core 3.1 for reason, .NET 5 onefile doesn't work */

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace NALStudioGameLauncherShortcutLaunch
{
	enum LaunchType
	{
		None,
		StartApplication,
		OpenStorePage
	}

	class Program
	{
		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);


		public static readonly string BaseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

		static void Main(string[] args)
		{
			IntPtr windowPointer = IntPtr.Zero;
			Process[] GameLauncherProsesses = Process.GetProcessesByName("NALStudio Game Launcher");

			if (GameLauncherProsesses.Length > 0)
			{
				Console.WriteLine("NALStudio Game Launcher instance found.");

				Process toFocus = GameLauncherProsesses[0];
				windowPointer = toFocus.MainWindowHandle;
			}
			else
			{
				Console.WriteLine("No instances of NALStudio Game Launcher found...");
				ProcessStartInfo sInfo = new ProcessStartInfo
				{
					WorkingDirectory = BaseDir,
					FileName = Path.Combine(BaseDir, "NALStudio Game Launcher.exe")
				};
				Console.WriteLine("Statring a new instance of NALStudio Game Launcher....");
				if (File.Exists(sInfo.FileName))
				{
					Process newProcess = Process.Start(sInfo);
					if (newProcess != null)
						windowPointer = newProcess.MainWindowHandle;
				}
				else
				{
					Console.WriteLine("No valid install of NALStudio Game Launcher found to launch the game.");
					Environment.SetEnvironmentVariable("NALStudioGameLauncherLaunchApplication", null, EnvironmentVariableTarget.User);
					Environment.SetEnvironmentVariable("NALStudioGameLauncherOpenStorePage", null, EnvironmentVariableTarget.User);
				}
			}

			LaunchType launchType = LaunchType.None;
			string gameUUID = null;
			string[] searchables = new string[] { "rungameid/", "storepage/" };
			if (args?.Length > 0)
			{
				string found = null;
				string matchedLaunchType = null;

				foreach (string toSearch in searchables)
				{
					found = Array.Find(args, (a) => a.Contains(toSearch));
					if (found != null)
					{
						matchedLaunchType = toSearch;
						break;
					}
				}
				if (found != null)
					gameUUID = found[(found.IndexOf(matchedLaunchType) + matchedLaunchType.Length)..];
				else
					gameUUID = null;

				launchType = matchedLaunchType switch
				{
					"rungameid/" => LaunchType.StartApplication,
					"storepage/" => LaunchType.OpenStorePage,
					_ => LaunchType.None,
				};
			}

			if (windowPointer != IntPtr.Zero)
			{
				Console.WriteLine("Focusable window found. Focusing...");
				SetForegroundWindow(windowPointer);
			}

			if (gameUUID != null)
			{
				switch (launchType)
				{
					case LaunchType.StartApplication:
						Console.WriteLine($"Launching \"{gameUUID}\"...");
						Environment.SetEnvironmentVariable("NALStudioGameLauncherLaunchApplication", gameUUID, EnvironmentVariableTarget.User);
						break;
					case LaunchType.OpenStorePage:
						Console.WriteLine($"Opening store page of app \"{gameUUID}\"...");
						Environment.SetEnvironmentVariable("NALStudioGameLauncherOpenStorePage", gameUUID, EnvironmentVariableTarget.User);
						break;
				}
			}
		}
	}
}
