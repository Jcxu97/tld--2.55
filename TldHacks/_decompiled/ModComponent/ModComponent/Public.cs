using Il2Cpp;
using ModComponent.Utils;

namespace ModComponent;

public class Public
{
	public static bool IsLoaded()
	{
		return IsReady();
	}

	public static bool IsReady()
	{
		return Implementation.isReady;
	}

	public static bool IsGameScene()
	{
		if (!string.IsNullOrEmpty(GameManager.m_ActiveScene) && !GameManager.m_ActiveScene.StartsWith("MainMenu") && !(GameManager.m_ActiveScene == "Boot"))
		{
			return !(GameManager.m_ActiveScene == "Empty");
		}
		return false;
	}

	public static void AddDependencyEntry(string MCFileName, string[] RequiresMCFileNames, bool RequiresDlc = false)
	{
		DependencyChecker.AddEntry(MCFileName, RequiresMCFileNames, RequiresDlc);
	}
}
