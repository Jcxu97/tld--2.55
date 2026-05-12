using System.IO;

namespace ModComponent.Mapper;

internal static class PackManager
{
	internal static void SetItemPackNotWorking(string pathToZipFile, string errorMessage)
	{
		Logger.LogItemPackError(Path.GetFileNameWithoutExtension(pathToZipFile), errorMessage);
	}
}
