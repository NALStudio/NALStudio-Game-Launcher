using System;
using System.Collections;
using System.IO;

namespace NALStudio.IO
{
	public static class Directory
	{
		/// <summary>
		/// Clears a directory and then deletes it.
		/// </summary>
		/// <param name="path">The path to the directory to be deleted.</param>
		public static IEnumerator RemoveCoroutine(string path, Action onComplete = null)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			foreach (FileInfo fi in di.EnumerateFiles("*", SearchOption.AllDirectories))
			{
				fi.Delete();
				yield return null;
			}
			di.Delete(true);
			onComplete?.Invoke();
		}
	}
}