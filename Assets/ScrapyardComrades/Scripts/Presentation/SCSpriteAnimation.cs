using UnityEngine;

public class SCSpriteAnimation : ScriptableObject
{
    public Sprite[] Frames;
    public int LengthInFrames = 60;
    public bool LoopsByDefault = true;
    public int LoopFrame = 0;
    public SoundData.Key SfxKey;
    public SoundData.Key SfxKey2;
    public int SfxFrame;
    public int SfxFrame2;
}
