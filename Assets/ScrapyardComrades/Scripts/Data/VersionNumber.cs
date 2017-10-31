using UnityEngine;

public static class VersionNumber
{
    /**
     * Did you remember to turn off DeleteAllOnStart on OptionsInitializer??
     */
    public const string VERSION_TITLE = "Alpha";
    public const int MAJOR_VERSION = 0;
    public const int MINOR_VERSION = 1;
    public const int PATCH_NUMBER = 4;

    public static string FullVersionString
    {
        get
        {
            return VERSION_TITLE + StringExtensions.SPACE + MAJOR_VERSION + StringExtensions.PERIOD + MINOR_VERSION + StringExtensions.PERIOD + PATCH_NUMBER;
        }
    }
}
