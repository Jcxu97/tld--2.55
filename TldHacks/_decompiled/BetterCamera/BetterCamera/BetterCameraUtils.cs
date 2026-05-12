using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace BetterCamera;

internal static class BetterCameraUtils
{
	public static void Save(this Texture2D TexToSave, string file)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)TexToSave).width, ((Texture)TexToSave).height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)1);
		Graphics.Blit((Texture)(object)TexToSave, temporary);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D val = new Texture2D(((Texture)TexToSave).width, ((Texture)TexToSave).height);
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0);
		val.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		byte[] array = Il2CppArrayBase<byte>.op_Implicit((Il2CppArrayBase<byte>)(object)ImageConversion.EncodeToPNG(val));
		if (array != null)
		{
			File.WriteAllBytes(file, array);
		}
		else if (Settings.instance.melonlogs)
		{
			MelonLogger.Msg("Could not encode camera photo: Bytes are empty.");
		}
	}
}
