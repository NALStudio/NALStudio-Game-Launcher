/* .NET Core 3.1 for reason, .NET 5 onefile doesn't work */

using System;
using System.IO;
using Microsoft.Win32;

namespace NALStudioGameLauncherRegistry
{
	class Program
	{
		#pragma warning disable CA1416 // Validate platform compatibility (registry is windows only)
		static void Main(string[] args)
		{
			if (args == null || args.Length < 1 || !Path.IsPathRooted(args[0]))
				return;
			RegistryKey ClassesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
			RegistryKey baseReg = ClassesRoot.OpenSubKey("nalstudiogamelauncher", true);
			if (baseReg == null)
				baseReg = ClassesRoot.CreateSubKey("nalstudiogamelauncher", true);
			baseReg.SetValue("URL Protocol", "", RegistryValueKind.String);

			RegistryKey cmdReg = baseReg.OpenSubKey("shell\\open\\command", true);
			if (cmdReg == null)
				cmdReg = baseReg.CreateSubKey("shell\\open\\command", true);
			cmdReg.SetValue("", $"\"{args[0]}\" \"%1\"", RegistryValueKind.String);
		}
	}
}
