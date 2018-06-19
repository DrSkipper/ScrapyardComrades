using UnityEngine;
using UnityEngine.UI;

public class ImageResScaler : MonoBehaviour
{
    public RectTransform ImageTransform;
    public float AspectCutoff = 1.5f;
    public float Divisor = 2.0f;
    bool OffsetYPos = true;

    private void Start()
    {
        int w = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_WIDTH_KEY, Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
        float ratio = w / (float)h;

        if (ratio < this.AspectCutoff)
        {
            float x = this.ImageTransform.sizeDelta.x;
            float y = this.ImageTransform.sizeDelta.y;
            x = Mathf.Round(x / this.Divisor);
            y = Mathf.Round(y / this.Divisor);
            this.ImageTransform.sizeDelta = new Vector2(x, y);

            if (this.OffsetYPos)
            {
                this.ImageTransform.anchoredPosition = new Vector2(this.ImageTransform.anchoredPosition.x, this.ImageTransform.anchoredPosition.y - Mathf.RoundToInt(y - (y / this.Divisor)));
            }
        }
    }
}
