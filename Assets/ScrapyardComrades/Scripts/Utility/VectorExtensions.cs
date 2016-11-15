using UnityEngine;

public static class VectorExtensions
{
    public static int IntX(this Vector2 self) { return Mathf.RoundToInt(self.x); }
    public static int IntY(this Vector2 self) { return Mathf.RoundToInt(self.y); }

    public static Vector2 VectorAtAngle(this Vector2 self, float angle)
    {
        float ourAngle = Vector2.Angle(Vector2.up, self);
        if (self.x < 0.0f)
            ourAngle = -ourAngle;
        float rad = Mathf.Deg2Rad * (ourAngle + angle);
        return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;
    }

    public static Vector2 EaseTowards(this Vector2 self, Vector2 destination, float t, float duration, Easing.EasingDelegate function)
    {
        float totalDistance = (destination - self).magnitude;
        float newDistance = function(t, 0.0f, totalDistance, duration);
        return Vector2.MoveTowards(self, destination, newDistance);
    }
}
