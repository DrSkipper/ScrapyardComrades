using UnityEngine;

public static class ScenePersistentLoading
{
    public static bool IsLoading { get; private set; }

    public static void BeginLoading(string levelName)
    {
        IsLoading = true;
        _levelToLoad = levelName;
    }

    public static string ConsumeLoad()
    {
        if (IsLoading)
            return _levelToLoad;
        return null;
    }

    /**
     * Private
     */
    private static string _levelToLoad;
}
