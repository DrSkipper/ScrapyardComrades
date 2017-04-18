using UnityEngine;
using System.Collections.Generic;

public class PackedSpriteGroup : ScriptableObject
{
    public const string INDEXED_TEXTURES_PATH = "Assets/ScrapyardComrades/Textures/IndexedTextures/";
    public const string TEXTURE_SUFFIX = ".png";

    public Texture2D[] Atlases;
    public string[] AtlasNames;

    public List<Sprite[]> Sprites;
}
