using UnityEngine;

public static class ScenePersistentLoading
{
    public struct LoadInfo
    {
        public string LevelToLoad;
        public bool IgnorePlayerSave;

        public LoadInfo(string levelToLoad, bool ignorePlayerSave)
        {
            this.LevelToLoad = levelToLoad;
            this.IgnorePlayerSave = ignorePlayerSave;
        }
    }

    public static bool IsLoading { get; private set; }

    public static void BeginLoading(string levelName, bool ignorePlayerSave = false)
    {
        IsLoading = true;
        _levelToLoad = levelName;
        _ignorePlayerSave = ignorePlayerSave;
    }

    public static LoadInfo? ConsumeLoad()
    {
        if (IsLoading)
        {
            IsLoading = false;
            return new LoadInfo(_levelToLoad, _ignorePlayerSave);
        }
        return null;
    }

    /**
     * Private
     */
    private static string _levelToLoad;
    private static bool _ignorePlayerSave;
}
