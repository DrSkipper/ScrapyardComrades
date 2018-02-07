using UnityEngine;

public static class OptionsValues
{
    public const string FULLSCREEN_KEY = "FULLSCREEN";
    public const string RESOLUTION_KEY = "RESOLUTION";
    public const string VSYNC_KEY = "VSYNC";

    public const string RESOLUTION_WIDTH_KEY = "RES_W";
    public const string RESOLUTION_HEIGHT_KEY = "RES_H";

    public static void ChangeValue(string key, int dir)
    {
        switch (key)
        {
            default:
                break;
            case FULLSCREEN_KEY:
                changeFullscreen();
                break;
            case RESOLUTION_KEY:
                changeResolution(dir);
                break;
            case VSYNC_KEY:
                changeVsync();
                break;
        }

        // Save the player prefs
        PlayerPrefs.Save();

        // Broadcast notification that an options value has been changed
        if (GlobalEvents.Notifier != null)
        {
            if (_valueChangeEvent == null)
                _valueChangeEvent = new OptionsValueChangedEvent(key);
            else
                _valueChangeEvent.OptionName = key;

            GlobalEvents.Notifier.SendEvent(_valueChangeEvent, true);
        }
    }

    public static string GetDisplaySuffix(string key)
    {
        switch (key)
        {
            default:
                return StringExtensions.EMPTY;
            case FULLSCREEN_KEY:
                return getFullscreenSuffix();
            case RESOLUTION_KEY:
                return getResolutionSuffix();
            case VSYNC_KEY:
                return getVsyncSuffix();
        }
    }

    /**
     * Private
     */
    private static Resolution[] _fullscreenResolutions;
    private static OptionsValueChangedEvent _valueChangeEvent;
    private const string ON = "ON";
    private const string OFF = "OFF";

    private static string getFullscreenSuffix()
    {
        return PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1 ? ON : OFF;
    }

    private static string getResolutionSuffix()
    {
        Resolution res = Screen.currentResolution;
        int w = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, res.width);
        int h = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, res.height);
        return StringExtensions.EMPTY + w + "x" + h;
    }

    private static string getVsyncSuffix()
    {
        return PlayerPrefs.GetInt(VSYNC_KEY, QualitySettings.vSyncCount == 0 ? 0 : 1) == 0 ? OFF : ON;
    }

    private static void changeFullscreen()
    {
        guaranteeFullscreenResolutions();
        if (Screen.fullScreen)
        {
            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;

            if (h >= _fullscreenResolutions[_fullscreenResolutions.Length - 1].height)
            {
                int i = _fullscreenResolutions.Length > 1 ? _fullscreenResolutions.Length - 2 : 0;
                w = _fullscreenResolutions[i].width;
                h = _fullscreenResolutions[i].height;
                PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, w);
                PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, h);
                PlayerPrefs.SetInt(FULLSCREEN_KEY, 0);
                Screen.SetResolution(w, h, false);
            }
            else
            {
                PlayerPrefs.SetInt(FULLSCREEN_KEY, 0);
                Screen.fullScreen = false;
            }
        }
        else
        {
            Resolution resolution = _fullscreenResolutions[_fullscreenResolutions.Length - 1];
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, resolution.width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, resolution.height);
            PlayerPrefs.SetInt(FULLSCREEN_KEY, 1);
            Screen.SetResolution(resolution.width, resolution.height, true);
        }
    }

    private static void changeResolution(int dir)
    {
        guaranteeFullscreenResolutions();
        int w = Screen.fullScreen ? Screen.currentResolution.width : Screen.width;
        int h = Screen.fullScreen ? Screen.currentResolution.height : Screen.height;
        int i = 0;

        for (; i < _fullscreenResolutions.Length; ++i)
        {
            if (_fullscreenResolutions[i].width == w && _fullscreenResolutions[i].height == h)
                break;
        }

        i += dir;
        int max = Screen.fullScreen ? _fullscreenResolutions.Length : _fullscreenResolutions.Length - 1;
        if (i >= max)
            i = 0;
        else if (i < 0)
            i = max - 1;

        w = _fullscreenResolutions[i].width;
        h = _fullscreenResolutions[i].height;
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, w);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, h);
        Screen.SetResolution(w, h, Screen.fullScreen);
    }

    private static void changeVsync()
    {
        int vsync = QualitySettings.vSyncCount == 0 ? 1 : 0; ;
        PlayerPrefs.SetInt(VSYNC_KEY, vsync);
        QualitySettings.vSyncCount = vsync;
    }

    private static void guaranteeFullscreenResolutions()
    {
        if (_fullscreenResolutions == null)
            _fullscreenResolutions = Screen.resolutions;
    }
}
