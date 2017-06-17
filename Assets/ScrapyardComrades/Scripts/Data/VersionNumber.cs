using UnityEngine;

public static class VersionNumber
{
    public static string VERSION_TITLE = "Alpha";
    public static int MAJOR_VERSION = 0;
    public static int MINOR_VERSION = 1;
    public static int PATCH_NUMBER = 0;

    public static string FullVersionString
    {
        get
        {
            return VERSION_TITLE + StringExtensions.SPACE + MAJOR_VERSION + StringExtensions.PERIOD + MINOR_VERSION + StringExtensions.PERIOD + PATCH_NUMBER;
        }
    }
}
