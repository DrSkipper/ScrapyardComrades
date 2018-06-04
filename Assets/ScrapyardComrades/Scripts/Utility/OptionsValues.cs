using UnityEngine;
using System.Collections.Generic;

public static class OptionsValues
{
    public const string FULLSCREEN_KEY = "FULLSCREEN";
    public const string RESOLUTION_KEY = "RESOLUTION";
    public const string VSYNC_KEY = "VSYNC";

    public const string RESOLUTION_WIDTH_KEY = "RES_W";
    public const string RESOLUTION_HEIGHT_KEY = "RES_H";

    public const int REFRESH_RATE = 60;

    private const int DEFAULT_WINDOWED_W = 960;
    private const int DEFAULT_WINDOWED_H = 540;


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
        bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreen)
        {
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, DEFAULT_WINDOWED_W);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, DEFAULT_WINDOWED_H);
            PlayerPrefs.SetInt(FULLSCREEN_KEY, 0);
            Screen.SetResolution(DEFAULT_WINDOWED_W, DEFAULT_WINDOWED_H, false, OptionsValues.REFRESH_RATE);
        }
        else
        {
            Resolution resolution = _fullscreenResolutions[_fullscreenResolutions.Length - 1];
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, resolution.width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, resolution.height);
            PlayerPrefs.SetInt(FULLSCREEN_KEY, 1);
            Screen.SetResolution(resolution.width, resolution.height, true, OptionsValues.REFRESH_RATE);
        }
    }

    private static void changeResolution(int dir)
    {
        guaranteeFullscreenResolutions();
        bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        int w = /*fullscreen ? */Screen.currentResolution.width;// : Screen.width;
        int h = /*fullscreen ? */Screen.currentResolution.height;// : Screen.height;
        w = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, w);
        h = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, h);
        int i = 0;

        Debug.Log("changeResolutions: dir =  " + dir + "old res= " + w + "x" + h);

        for (; i < _fullscreenResolutions.Length; ++i)
        {
            if (_fullscreenResolutions[i].width == w && _fullscreenResolutions[i].height == h)
                break;
        }
        
        i += dir;
        int max = fullscreen ? _fullscreenResolutions.Length : _fullscreenResolutions.Length - 1;
        if (i >= max)
            i = 0;
        else if (i < 0)
            i = max - 1;

        w = _fullscreenResolutions[i].width;
        h = _fullscreenResolutions[i].height;
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, w);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, h);
        Screen.SetResolution(w, h, fullscreen, OptionsValues.REFRESH_RATE);

        Debug.Log("changeResolutions: new res: " + w + "x" + h);
    }

    private static void changeVsync()
    {
        int vsync = QualitySettings.vSyncCount == 0 ? 1 : 0;
        PlayerPrefs.SetInt(VSYNC_KEY, vsync);
        QualitySettings.vSyncCount = vsync;
    }

    private static void guaranteeFullscreenResolutions()
    {
        if (_fullscreenResolutions == null)
        {
            List<Resolution> resolutions = new List<Resolution>(Screen.resolutions);
            int[] guaranteedWidths = new int[] { DEFAULT_WINDOWED_W, 1280, 1920 };
            int[] guaranteedHeights = new int[] { DEFAULT_WINDOWED_H, 720, 1080 };

            // Make sure all our guaranteed resolutions are present in the selectable resolutions list
            for (int i = 0; i < guaranteedWidths.Length; ++i)
            {
                int w = guaranteedWidths[i];
                int h = guaranteedHeights[i];

                if (!alreadyHasResolution(w, h, resolutions))
                {
                    int insertIndex = indexToInsertRes(w, h, resolutions);
                    resolutions.Insert(insertIndex, createResolution(w, h, REFRESH_RATE));
                }
            }

            // Remove duplicate resolutions
            List<Resolution> uniqueResolutions = new List<Resolution>();
            for (int i = 0; i < resolutions.Count; ++i)
            {
                bool isUnique = true;
                for (int j = uniqueResolutions.Count - 1; j >= 0; --j)
                {
                    if (resolutions[i].width == uniqueResolutions[j].width && resolutions[i].height == uniqueResolutions[j].height)
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                    uniqueResolutions.Add(createResolution(resolutions[i].width, resolutions[i].height, REFRESH_RATE));
            }

            _fullscreenResolutions = uniqueResolutions.ToArray();

            // Log resolutions array
            string log = "_fullscreenResolutions: \n";
            for (int i = 0; i < _fullscreenResolutions.Length; ++i)
            {
                log += "" + i + ": " + _fullscreenResolutions[i] + "\n";
            }
            Debug.Log(log);
        }
    }

    private static Resolution createResolution(int w, int h, int rHz)
    {
        Resolution res = new Resolution();
        res.width = w;
        res.height = h;
        res.refreshRate = REFRESH_RATE;
        return res;
    }

    private static bool alreadyHasResolution(int w, int h, List<Resolution> res)
    {
        for (int i = 0; i < res.Count; ++i)
        {
            Resolution r = res[i];
            if (r.width == w && r.height == h)
                return true;
        }
        return false;
    }

    public static int indexToInsertRes(int w, int h, List<Resolution> res)
    {
        int insertBeforeIndex = 0;
        for (; insertBeforeIndex < res.Count; ++insertBeforeIndex)
        {
            Resolution r = res[insertBeforeIndex];
            if (r.width > w || (r.width == w && r.height > h))
                break;
        }
        return insertBeforeIndex;
    }
}
