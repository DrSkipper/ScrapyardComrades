using UnityEngine;
using System.Collections.Generic;

public class PackedSpriteGroup : ScriptableObject
{
    public const string INDEXED_TEXTURES_PATH = "Assets/ScrapyardComrades/Textures/IndexedTextures/";
    public const string TEXTURE_SUFFIX = ".png";

    public AtlasEntry[] Atlases;

    [System.Serializable]
    public struct AtlasEntry
    {
        public string RelativePath;
        public Texture2D Atlas;
        public Sprite[] Sprites;
    }
}
