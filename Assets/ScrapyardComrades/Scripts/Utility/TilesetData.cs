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

        public SpriteData(string spriteName)
        {
            this.SpriteName = spriteName;
            this.Type = TileType.Empty;
            this.AllowAutotile = false;
        }
    }

    public string AtlasName;
    public int StandardTileSize;
    public SpriteData[] Tiles;

    public Dictionary<string, SpriteData> GetSpriteDataDictionary()
    {
        Dictionary<string, SpriteData> dict = new Dictionary<string, SpriteData>();
        if (this.Tiles != null)
        {
            for (int i = 0; i < this.Tiles.Length; ++i)
            {
                dict[this.Tiles[i].SpriteName] = this.Tiles[i];
            }
        }
        return dict;
    }

    public void ApplySpriteDataDictionary(Dictionary<string, SpriteData> dict)
    {
        this.Tiles = new List<SpriteData>(dict.Values).ToArray();
    }

    public Dictionary<TileType, List<SpriteData>> GetAutotileDictionary()
    {
        Dictionary<TileType, List<SpriteData>> dict = new Dictionary<TileType, List<SpriteData>>();

        if (this.Tiles != null)
        {
            for (int i = 0; i < this.Tiles.Length; ++i)
            {
                SpriteData tile = this.Tiles[i];
                if (tile.AllowAutotile)
                {
                    if (!dict.ContainsKey(tile.Type))
                        dict.Add(tile.Type, new List<SpriteData>());
                    dict[tile.Type].Add(tile);
                }
            }
        }

        return dict;
    }
}
