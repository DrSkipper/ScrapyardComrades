using UnityEngine;

public class SCSpriteAnimation : ScriptableObject
{
    public Sprite[] Frames;
    public int LengthInFrames = 60;
    public bool LoopsByDefault = true;
    public int LoopFrame = 0;
    public AudioClip Sfx;
    public int SfxFrame;
}
