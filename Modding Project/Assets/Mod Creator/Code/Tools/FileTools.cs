using System;
using System.IO;
using UnityEngine;

namespace Code.Tools
{
	public static class FileTools
	{
		public static bool CreateDirectorySafe(string path, out DirectoryInfo directory)
		{
			try
			{
				directory = Directory.CreateDirectory(path);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			directory = null;
			return false;
		}
		
		public static bool ReadAllTextSafe(string path, out string text)
		{
			try
			{
				text = File.ReadAllText(path);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			text = "";
			return false;
		}
		
		public static bool ReadAllLinesSafe(string path, out string[] lines)
		{
			try
			{
				lines = File.ReadAllLines(path);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			lines = Array.Empty<string>();
			return false;
		}
		
		public static bool ReadAllBytesSafe(string path, out byte[] bytes)
		{
			try
			{
				bytes = File.ReadAllBytes(path);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			bytes = Array.Empty<byte>();
			return false;
		}
		
		public static bool WriteAllTextSafe(string path, string text)
		{
			try
			{
				File.WriteAllText(path, text);
				
				if (File.Exists(path))
					return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			return false;
		}

		public static bool AppendAllTextSafe(string path, string text)
		{
			try
			{
				File.AppendAllText(path, text);
				
				if (File.Exists(path))
					return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			return false;
		}
		
		public static bool MoveSafe(string from, string to)
		{
			try
			{
				File.Move(from, to);
				
				if (!File.Exists(from) && File.Exists(to))
					return true;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			return false;
		}
	}
}