using UnityEngine;

public interface IMovingPlatform
{
    Vector2 Velocity { get; }
    void PlayRidingEffects();
}
