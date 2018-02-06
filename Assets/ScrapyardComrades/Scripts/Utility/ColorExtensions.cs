using UnityEngine;

public static class ColorExtensions
{
    /**
     * r,g,b values are from 0 to 1
     *  h = [0,360], s = [0,1], v = [0,1]
     *		if s == 0, then h = -1 (undefined)
     */

    public struct HSV
    {
        public float h;
        public float s;
        public float v;

        public HSV(float hue = 0.0f, float saturation = 0.0f, float value = 0.0f)
        {
            h = hue;
            s = saturation;
            v = value;
        }

        public Color RGBColor { get { return ColorWithHSV(this); } }
    }

    public static HSV GetHSV(this Color self)
    {
        HSV hsv = new HSV();
        float min = Mathf.Min(self.r, self.g, self.b);
        float max = Mathf.Max(self.r, self.g, self.b);

        hsv.v = max;               // v

        if (max != 0)
        {
            float delta = max - min;
            hsv.s = delta / max;       // s

            if (self.r == max)
                hsv.h = (self.g - self.b) / delta;       // between yellow & magenta
            else if (self.g == max)
                hsv.h = 2 + (self.b - self.r) / delta;   // between cyan & yellow
            else
                hsv.h = 4 + (self.r - self.g) / delta;   // between magenta & cyan

            hsv.h *= 60;               // degrees
            if (hsv.h < 0)
                hsv.h += 360;
        }
        else
        {
            // r = g = b = 0		// s = 0, h is undefined
            hsv.s = 0;
            hsv.h = -1;
        }

        return hsv;
    }

    public static Color ColorWithHSV(HSV hsv)
    {
        return ColorWithHSV(hsv.h, hsv.s, hsv.v);
    }

    public static Color ColorWithHSV(float h, float s, float v, float a = 1.0f)
    {
        Color color = new Color();
        color.a = a;

        if (s == 0)
        {
            // achromatic (grey)
            color.r = color.g = color.b = v;
            return color;
        }

        h /= 60;            // sector 0 to 5
        int i = Mathf.FloorToInt(h);
        float f = h - i;          // factorial part of h
        float p = v * (1 - s);
        float q = v * (1 - s * f);
        float t = v * (1 - s * (1 - f));

        switch (i)
        {
            case 0:
                color.r = v;
                color.g = t;
                color.b = p;
                break;
            case 1:
                color.r = q;
                color.g = v;
                color.b = p;
                break;
            case 2:
                color.r = p;
                color.g = v;
                color.b = t;
                break;
            case 3:
                color.r = p;
                color.g = q;
                color.b = v;
                break;
            case 4:
                color.r = t;
                color.g = p;
                color.b = v;
                break;
            default:        // case 5:
                color.r = v;
                color.g = p;
                color.b = q;
                break;
        }

        return color;
    }

    public static HSV LerpTo(this HSV self, HSV target, float t, float duration)
    {
        float hDiff = target.h - self.h;
        if (hDiff > 180.0f)
            hDiff -= 360.0f;
        else if (hDiff < -180.0f)
            hDiff += 360.0f;

        return new HSV(
            Easing.Linear(t, self.h, hDiff, duration),
            Easing.Linear(t, self.s, target.s - self.s, duration),
            Easing.Linear(t, self.v, target.v - self.v, duration)
        );
    }
}
