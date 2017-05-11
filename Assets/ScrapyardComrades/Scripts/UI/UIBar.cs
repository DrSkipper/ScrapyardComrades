using UnityEngine;

public class UIBar : MonoBehaviour
{
    public RectTransform BarTransform;
    public int TargetLength = 104;
    public Dimension BarDimension = Dimension.Width;

    public enum Dimension
    {
        Width,
        Height
    }

    public void ChangeTargetLength(int newLength)
    {
        if (this.TargetLength != newLength)
        {
            this.TargetLength = newLength;
            ((RectTransform)this.transform).sizeDelta = new Vector2(newLength, this.BarTransform.sizeDelta.y);
        }
    }

    public void UpdateLength(float current, float max)
    {
        this.FillPercent(current / max);
    }

    public void EmptyCompletely()
    {
        this.FillPercent(0.0f);
    }

    public void FillCompletely()
    {
        this.FillPercent(1.0f);
    }

    public void FillPercent(float percent)
    {
        int target = Mathf.RoundToInt(this.TargetLength * percent);

        if (this.BarDimension == Dimension.Width)
        {
            this.BarTransform.sizeDelta = new Vector2(target, this.BarTransform.sizeDelta.y);
        }
        else
        {
            this.BarTransform.sizeDelta = new Vector2(this.BarTransform.sizeDelta.x, target);
        }
    }
}
