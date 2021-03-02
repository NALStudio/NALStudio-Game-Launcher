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
using System.IO.Compression;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace NALStudio.IO
{
	public static class NALPath
	{
		public static string NormalizePath(string path)
		{
			return Path.GetFullPath(new Uri(path).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.ToUpperInvariant();
		}

		public static bool Match(string path1, string path2)
		{
			return NormalizePath(path1) == NormalizePath(path2);
		}
	}

	public static class NALDirectory
	{
		struct DirSizeCalculator : IJob
		{
			public NativeArray<char> pathArray;
			public NativeArray<long> result;

			public void Execute()
			{
				long size = 0L;
				string path = new string(pathArray.ToArray());
				foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
				{
					try
					{
						size += fi.Length;
					}
					catch (Exception e)
					{
						Debug.LogWarning(e.Message);
					}
				}

				result[0] = size;
			}
		}

		public static IEnumerator GetSize(string path, Action<long> onComplete)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				Debug.LogError($"Directory \"{path}\" does not exist!");
				onComplete.Invoke(0L);
				yield break;
			}
			NativeArray<long> result = new NativeArray<long>(1, Allocator.Persistent);
			NativeArray<char> pathArray = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
			DirSizeCalculator job = new DirSizeCalculator
			{
				pathArray = pathArray,
				result = result
			};
			JobHandle handle = job.Schedule();
			yield return new WaitUntil(() => handle.IsCompleted);
			handle.Complete();
			onComplete?.Invoke(result[0]);
			yield return null;
			result.Dispose();
			pathArray.Dispose();
		}

		struct DirDelete : IJob
		{
			public bool recursive;
			public NativeArray<char> pathArray;

			public void Execute()
			{
				string path = new string(pathArray.ToArray());
				Directory.Delete(path, recursive);
			}
		}

		public static IEnumerator Delete(string path, bool recursive)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				Debug.LogError($"Directory \"{path}\" does not exist!");
				yield break;
			}
			NativeArray<char> pathArray = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
			DirDelete job = new DirDelete
			{
				pathArray = pathArray,
				recursive = recursive
			};
			JobHandle handle = job.Schedule();
			yield return new WaitUntil(() => handle.IsCompleted);
			handle.Complete();
			yield return null;
			pathArray.Dispose();
		}
	}

	public static class NALZipFile
	{
		struct Ex2Dir : IJob
		{
			public NativeArray<char> sourceChars;
			public NativeArray<char> destinationChars;

			public void Execute()
			{
				string source = new string(sourceChars.ToArray());
				string destination = new string(destinationChars.ToArray());
				ZipFile.ExtractToDirectory(source, destination);
			}
		}

		public static IEnumerator ExtractToDirectoryThreaded(string sourceArchiveFileName, string destinationDirectoryName)
		{
			NativeArray<char> sourceChars = new NativeArray<char>(sourceArchiveFileName.ToCharArray(), Allocator.Persistent);
			NativeArray<char> destinationChars = new NativeArray<char>(destinationDirectoryName.ToCharArray(), Allocator.Persistent);
			Ex2Dir job = new Ex2Dir
			{
				sourceChars = sourceChars,
				destinationChars = destinationChars
			};
			JobHandle handle = job.Schedule();
			yield return new WaitUntil(() => handle.IsCompleted);
			handle.Complete();
			yield return null;
			sourceChars.Dispose();
			destinationChars.Dispose();
		}
	}
}
