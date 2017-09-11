using UnityEngine;

public class OptionsInitializer : MonoBehaviour
{
    //NOTE: DO NOT LEAVE THIS ON IN DISTRIBUTABLE BUILD!
    public bool DeleteAllOnStart = false;

    void Awake()
    {
        if (this.DeleteAllOnStart)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        bool fullscreen = PlayerPrefs.GetInt(OptionsValues.FULLSCREEN_KEY, 1) == 1;
        Screen.fullScreen = fullscreen;

        if (PlayerPrefs.HasKey(OptionsValues.RESOLUTION_WIDTH_KEY) && PlayerPrefs.HasKey(OptionsValues.RESOLUTION_HEIGHT_KEY))
        {
            int w = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_WIDTH_KEY);
            int h = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_HEIGHT_KEY);

            Screen.SetResolution(w, h, fullscreen);
        }
        else
        {
            Resolution[] resolutions = Screen.resolutions;
            Resolution res = fullscreen ? resolutions[resolutions.Length - 1] : resolutions[Mathf.Max(0, resolutions.Length - 2)];
            Screen.SetResolution(res.width, res.height, fullscreen);
            PlayerPrefs.SetInt(OptionsValues.RESOLUTION_WIDTH_KEY, res.width);
            PlayerPrefs.SetInt(OptionsValues.RESOLUTION_HEIGHT_KEY, res.height);
            PlayerPrefs.Save();
        }

        if (PlayerPrefs.HasKey(OptionsValues.VSYNC_KEY))
        {
            QualitySettings.vSyncCount = PlayerPrefs.GetInt(OptionsValues.VSYNC_KEY);
        }
    }
}
