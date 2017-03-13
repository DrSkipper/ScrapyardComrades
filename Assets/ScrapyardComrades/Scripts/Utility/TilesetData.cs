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
        RowRight,

        // If surrounded on 4 sides, check if just one diagonal neighbor is missing, if so, we are the inner corner opposite to the missing neighbor
        InnerCornerTopLeft,
        InnerCornerTopRight,
        InnerCornerBottomLeft,
        InnerCornerBottomRight
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

    public static string GetAutotileSpriteName(TileType type, Dictionary<TileType, List<SpriteData>> autotileDict, TilesetData tileset)
    {
        if (autotileDict.ContainsKey(type))
        {
            return autotileDict[type][Random.Range(0, autotileDict[type].Count)].SpriteName;
        }
        else if ((type == TileType.TopLeft || type == TileType.TopRight) && autotileDict.ContainsKey(TileType.Top))
        {
            return autotileDict[TileType.Top][Random.Range(0, autotileDict[TileType.Top].Count)].SpriteName;
        }
        else if ((type == TileType.BottomLeft || type == TileType.BottomRight) && autotileDict.ContainsKey(TileType.Bottom))
        {
            return autotileDict[TileType.Bottom][Random.Range(0, autotileDict[TileType.Bottom].Count)].SpriteName;
        }
        else if (type != TileType.Empty && autotileDict.ContainsKey(TileType.Surrounded))
        {
            return autotileDict[TileType.Surrounded][Random.Range(0, autotileDict[TileType.Surrounded].Count)].SpriteName;
        }
        else if (type != TileType.Empty && autotileDict.ContainsKey(TileType.Lone))
        {
            return autotileDict[TileType.Lone][Random.Range(0, autotileDict[TileType.Lone].Count)].SpriteName;
        }
        else // if (type == TileType.Empty), or no common non-empty entries in tileset
        {
            return tileset.GetEmptySpriteName();
        }
    }

    public static TileType GetAutotileType(int x, int y, NewMapInfo.MapTile[,] data, Dictionary<string, SpriteData> spriteDataDict, bool forceFilled)
    {
        if (!forceFilled && (!spriteDataDict.ContainsKey(data[x, y].sprite_name) || spriteDataDict[data[x, y].sprite_name].Type == TileType.Empty))
            return TileType.Empty;

        NewMapInfo.MapTile left = x > 0 ? data[x - 1, y] : null;
        NewMapInfo.MapTile right = x < data.GetLength(0) - 1 ? data[x + 1, y] : null;
        NewMapInfo.MapTile down = y > 0 ? data[x, y - 1] : null;
        NewMapInfo.MapTile up = y < data.GetLength(1) - 1 ? data[x, y + 1] : null;

        bool leftFilled = left == null || (spriteDataDict.ContainsKey(left.sprite_name) && spriteDataDict[left.sprite_name].Type != TileType.Empty);
        bool rightFilled = right == null || (spriteDataDict.ContainsKey(right.sprite_name) && spriteDataDict[right.sprite_name].Type != TileType.Empty);
        bool downFilled = down == null || (spriteDataDict.ContainsKey(down.sprite_name) && spriteDataDict[down.sprite_name].Type != TileType.Empty);
        bool upFilled = up == null || (spriteDataDict.ContainsKey(up.sprite_name) && spriteDataDict[up.sprite_name].Type != TileType.Empty);

        if (!leftFilled && !rightFilled && !downFilled && !upFilled)
            return TileType.Lone;
        else if (!leftFilled && rightFilled && downFilled && upFilled)
            return TileType.MidLeft;
        else if (leftFilled && !rightFilled && downFilled && upFilled)
            return TileType.MidRight;
        else if (leftFilled && rightFilled && !downFilled && upFilled)
            return TileType.Bottom;
        else if (leftFilled && rightFilled && downFilled && !upFilled)
            return TileType.Top;
        else if (!leftFilled && !rightFilled && downFilled && upFilled)
            return TileType.ColumnMid;
        else if (!leftFilled && !rightFilled && !downFilled && upFilled)
            return TileType.ColumnBottom;
        else if (!leftFilled && !rightFilled && downFilled && !upFilled)
            return TileType.ColumnTop;
        else if (leftFilled && rightFilled && !downFilled && !upFilled)
            return TileType.RowMid;
        else if (!leftFilled && rightFilled && !downFilled && !upFilled)
            return TileType.RowLeft;
        else if (leftFilled && !rightFilled && !downFilled && !upFilled)
            return TileType.RowRight;
        else if (!leftFilled && rightFilled && downFilled && !upFilled)
            return TileType.TopLeft;
        else if (leftFilled && !rightFilled && downFilled && !upFilled)
            return TileType.TopRight;
        else if (!leftFilled && rightFilled && !downFilled && upFilled)
            return TileType.BottomLeft;
        else if (leftFilled && !rightFilled && !downFilled && upFilled)
            return TileType.BottomRight;
        else // if (leftFilled && rightFilled && downFilled && upFilled)
        {
            // Surrounded on cardinals, check if inner corner
            NewMapInfo.MapTile lowerLeft = x > 0 && y > 0 ? data[x - 1, y - 1] : null;
            NewMapInfo.MapTile lowerRight = x < data.GetLength(0) - 1 && y > 0 ? data[x + 1, y - 1] : null;
            NewMapInfo.MapTile upperLeft = x > 0 && y < data.GetLength(1) - 1 ? data[x - 1, y + 1] : null;
            NewMapInfo.MapTile upperRight = x < data.GetLength(0) - 1 && y < data.GetLength(1) - 1 ? data[x + 1, y + 1] : null;

            bool lowerLeftFilled = lowerLeft == null || (spriteDataDict.ContainsKey(lowerLeft.sprite_name) && spriteDataDict[lowerLeft.sprite_name].Type != TileType.Empty);
            bool lowerRightFilled = lowerRight == null || (spriteDataDict.ContainsKey(lowerRight.sprite_name) && spriteDataDict[lowerRight.sprite_name].Type != TileType.Empty);
            bool upperLeftFilled = upperLeft == null || (spriteDataDict.ContainsKey(upperLeft.sprite_name) && spriteDataDict[upperLeft.sprite_name].Type != TileType.Empty);
            bool upperRightFilled = upperRight == null || (spriteDataDict.ContainsKey(upperRight.sprite_name) && spriteDataDict[upperRight.sprite_name].Type != TileType.Empty);
            
            if (!lowerLeftFilled && lowerRightFilled && upperLeftFilled && upperRightFilled)
                return TileType.InnerCornerTopRight;
            else if (lowerLeftFilled && !lowerRightFilled && upperLeftFilled && upperRightFilled)
                return TileType.InnerCornerTopLeft;
            else if (lowerLeftFilled && lowerRightFilled && !upperLeftFilled && upperRightFilled)
                return TileType.InnerCornerBottomRight;
            else if (lowerLeftFilled && lowerRightFilled && upperLeftFilled && !upperRightFilled)
                return TileType.InnerCornerBottomLeft;
            return TileType.Surrounded;
        }
    }

    public string GetEmptySpriteName()
    {
        string nonAutoTile = null;
        string autoTile = null;

        if (this.Tiles != null)
        {
            for (int i = 0; i < this.Tiles.Length; ++i)
            {
                if (this.Tiles[i].Type == TileType.Empty)
                {
                    if (this.Tiles[i].AllowAutotile)
                    {
                        autoTile = this.Tiles[i].SpriteName;
                        break;
                    }
                    else if (nonAutoTile == null)
                    {
                        nonAutoTile = this.Tiles[i].SpriteName;
                    }
                }
            }
        }

        if (autoTile != null)
            return autoTile;
        else if (nonAutoTile != null)
            return nonAutoTile;
        return NewMapInfo.MapTile.EMPTY_TILE_SPRITE_NAME;
    }
}
