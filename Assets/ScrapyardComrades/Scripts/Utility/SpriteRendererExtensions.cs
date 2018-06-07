using UnityEngine;

public static class SpriteRendererExtensions
{
    public static void SetAlpha(this SpriteRenderer self, float a)
    {
        Color c = self.color;
        c.a = Mathf.Clamp(a, 0.0f, 1.0f);
        self.color = c;
    }

    public static void AddAlpha(this SpriteRenderer self, float da)
    {
        Color c = self.color;
        c.a = Mathf.Clamp(c.a + da, 0.0f, 1.0f);
        self.color = c;
    }
}
