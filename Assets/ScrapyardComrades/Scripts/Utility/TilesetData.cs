using UnityEngine;
using System.Collections.Generic;

public class TilesetData : ScriptableObject
{
    [System.Serializable]
    public enum TileType
    {
        Empty,
        TopLeft,
        Top,
        TopRight,
        MidLeft,
        Surrounded,
        MidRight,
        BottomLeft,
        Bottom,
        BottomRight,
        Lone,
        ColumnTop,
        ColumnMid,
        ColumnBottom,
        RowLeft,
        RowMid,
        RowRight
    }

    [System.Serializable]
    public struct SpriteData
    {
        public string SpriteName;
        public TileType Type;
        public bool AllowAutotile;
    }

    public string AtlasName;
    public int StandardTileSize;
    public List<SpriteData> Tiles;
}
