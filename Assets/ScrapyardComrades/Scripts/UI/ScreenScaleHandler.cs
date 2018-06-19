using UnityEngine;
using UnityEngine.UI;

public class ScreenScaleHandler : MonoBehaviour
{
    public CanvasScaler CanvasScaler;
    public float HighScreenScale = 2.0f;
    public float LowScreenScale = 1.0f;
    public int FullscreenCutoff = 540;
    public int WindowedCutoff = 540;

    void Start()
    {
        updateScaleFactor();
        GlobalEvents.Notifier.Listen(OptionsValueChangedEvent.NAME, this, onOptionChanged);
    }

    /**
     * Private
     */
    private void onOptionChanged(LocalEventNotifier.Event e)
    {
        OptionsValueChangedEvent oce = e as OptionsValueChangedEvent;
        if (oce.OptionName == OptionsValues.RESOLUTION_KEY || oce.OptionName == OptionsValues.FULLSCREEN_KEY)
            updateScaleFactor();
    }

    private void updateScaleFactor()
    {
        int h = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
        bool fullscreen = PlayerPrefs.GetInt(OptionsValues.FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreen)
        {
            this.CanvasScaler.scaleFactor = h >= this.FullscreenCutoff ? this.HighScreenScale : this.LowScreenScale;
        }
        else
        {
            this.CanvasScaler.scaleFactor = h >= this.WindowedCutoff ? this.HighScreenScale : this.LowScreenScale;
        }
    }
}
