﻿using UnityEngine;
using System.Collections.Generic;

public class PackedSpriteGroup : ScriptableObject
{
    public const string INDEXED_TEXTURES_PATH = "Assets/ScrapyardComrades/Textures/IndexedTextures/";
    public const string TEXTURE_SUFFIX = ".png";

    public AtlasEntry[] Atlases;

    [System.Serializable]
    public struct AtlasEntry
    {
        public string AtlasName;
        public string RelativePath;
        public Sprite[] Sprites;

        public Texture2D Atlas
        {
            get
            {
                return this.Sprites != null && this.Sprites.Length > 0 ? this.Sprites[0].texture : null;
            }
        }
    }
}
