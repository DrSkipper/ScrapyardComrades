using UnityEngine;

public class ResolutionLimiter : MonoBehaviour
{
    public int ResolutionDoublingThreshold;

    private void Start()
    {
        if (Screen.fullScreen)
        {
            int screenHeight = Screen.height;
            int screenWidth = Screen.width;

            while (screenHeight > this.ResolutionDoublingThreshold)
            {
                screenHeight /= 2;
                screenWidth /= 2;
            }

            Screen.SetResolution(screenWidth, screenHeight, true);
        }
    }
}
