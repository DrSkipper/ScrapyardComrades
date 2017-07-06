using UnityEngine;

public class SCPickup : ScriptableObject
{
    public Sprite Sprite;
    public int Damage;
    public int StunTime;
    public float ThrowVelocity;
    public KeyType Key;

    public enum KeyType
    {
        None,
        Red,
        Blue,
        Yellow
    }
}
