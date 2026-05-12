using System;
using System.IO;
using System.Reflection;
using System.Text;
using Il2CppSystem;
using MelonLoader;
using MelonLoader.Utils;
using TLD.TinyJSON;
using UnityEngine;

namespace ModSettings;

public abstract class JsonModSettings : ModSettingsBase
{
	protected readonly string modName;

	protected readonly string jsonPath;

	public JsonModSettings()
		: this(null)
	{
	}

	public JsonModSettings(string? relativeJsonFilePath)
	{
		modName = GetType().Assembly.GetName().Name;
		jsonPath = ToAbsoluteJsonPath(relativeJsonFilePath ?? modName);
		LoadOrCreate();
	}

	private static string ToAbsoluteJsonPath(string relativePath)
	{
		if (string.IsNullOrEmpty(relativePath))
		{
			throw new ArgumentException("JSON file path cannot be null or empty");
		}
		if (relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
		{
			throw new ArgumentException("JSON file path contains an invalid path character: " + relativePath);
		}
		if (Path.IsPathRooted(relativePath))
		{
			throw new ArgumentException("JSON file path must be relative. Absolute paths are not allowed.");
		}
		if (Path.GetExtension(relativePath) != ".json")
		{
			relativePath += ".json";
		}
		return Path.Combine(MelonEnvironment.ModsDirectory, relativePath);
	}

	protected override void OnConfirm()
	{
		Save();
	}

	public void Save()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string contents = JSON.Dump(this, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints);
			File.WriteAllText(jsonPath, contents, Encoding.UTF8);
			Debug.Log(Object.op_Implicit("[" + modName + "] Config file saved to " + jsonPath));
		}
		catch (Exception value)
		{
			new Instance(modName).Error($"[{modName}] Error while trying to write config file {jsonPath}: {value}");
		}
	}

	public void Reload()
	{
		try
		{
			Variant variant = JSON.Load(File.ReadAllText(jsonPath, Encoding.UTF8));
			typeof(JSON).GetMethod("Populate").MakeGenericMethod(GetType()).Invoke(null, new object[2] { variant, this });
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				confirmedValues[fieldInfo] = fieldInfo.GetValue(this);
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"[{modName}] Error while trying to read config file {jsonPath}: {ex}");
			throw new IOException("Error while trying to read config file " + jsonPath, ex);
		}
	}

	private void LoadOrCreate()
	{
		if (File.Exists(jsonPath))
		{
			Reload();
			return;
		}
		Debug.Log(Object.op_Implicit($"[{modName}] Settings file {jsonPath} did not exist, writing default settings file"));
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			confirmedValues[fieldInfo] = fieldInfo.GetValue(this);
		}
		string contents = JSON.Dump(this, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints);
		File.WriteAllText(jsonPath, contents, Encoding.UTF8);
	}
}
