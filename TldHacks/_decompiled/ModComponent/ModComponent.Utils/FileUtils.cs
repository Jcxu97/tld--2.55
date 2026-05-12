using System;
using System.Collections.Generic;
using System.IO;

namespace ModComponent.Utils;

internal static class FileUtils
{
	private static Dictionary<string, byte[]> headerTypes = new Dictionary<string, byte[]>
	{
		{
			"zip-1",
			new byte[4] { 80, 75, 3, 4 }
		},
		{
			"zip-2",
			new byte[4] { 80, 75, 5, 6 }
		},
		{
			"zip-3",
			new byte[4] { 80, 75, 7, 8 }
		},
		{
			"gzip-1",
			new byte[2] { 31, 139 }
		},
		{
			"tar-1",
			new byte[2] { 31, 157 }
		},
		{
			"lzh-1",
			new byte[2] { 31, 160 }
		},
		{
			"bzip-1",
			new byte[3] { 66, 90, 104 }
		},
		{
			"lzip-1",
			new byte[4] { 76, 90, 73, 80 }
		},
		{
			"rar-1",
			new byte[5] { 82, 97, 114, 33, 26 }
		},
		{
			"7z-1",
			new byte[5] { 55, 122, 188, 175, 39 }
		}
	};

	private static byte[] GetFirstBytes(FileStream fs, int length)
	{
		fs.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[length];
		fs.Read(array, 0, length);
		return array;
	}

	internal static string? DetectZipFileType(FileStream fs)
	{
		byte[] firstBytes = GetFirstBytes(fs, 5);
		Logger.LogDebug($"MC File Bytes: {firstBytes[0]}|{firstBytes[1]}|{firstBytes[2]}|{firstBytes[3]}|{firstBytes[4]} {fs.Name}");
		foreach (KeyValuePair<string, byte[]> headerType in headerTypes)
		{
			if (HeaderBytesMatch(headerType, firstBytes))
			{
				return headerType.Key.ToLowerInvariant().Split('-')[0];
			}
		}
		return null;
	}

	private static bool HeaderBytesMatch(KeyValuePair<string, byte[]> headerType, byte[] dataBytes)
	{
		if (dataBytes.Length < headerType.Value.Length)
		{
			return false;
		}
		for (int i = 0; i < headerType.Value.Length; i++)
		{
			if (headerType.Value[i] != dataBytes[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static string GetRelativePath(string file, string directory)
	{
		if (file.StartsWith(directory))
		{
			return file.Substring(directory.Length + 1);
		}
		throw new ArgumentException("Could not determine relative path of '" + file + "' to '" + directory + "'.");
	}
}
