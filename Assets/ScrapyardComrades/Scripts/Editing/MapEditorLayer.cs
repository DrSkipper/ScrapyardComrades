using UnityEngine;
using System.Collections.Generic;

public class MapEditorLayer
{
    public string Name;
    public LayerType Type;
    public int Depth;

    public enum LayerType
    {
        Tiles,
        Objects,
        Parallax
    }
}

public class MapEditorTilesLayer : MapEditorLayer
{
    public TilesetData Tileset;
    public NewMapInfo.MapTile[,] Data;
    public string CurrentSpriteName;
    public bool AutoTileEnabled;
    public bool EraserEnabled;
    public TileRenderer Visual;

    public MapEditorTilesLayer(NewMapInfo.MapLayer mapLayer, int depth, Dictionary<string, TilesetData> tilesets, TileRenderer visual)
    {
        this.Name = mapLayer.name;
        this.Type = LayerType.Tiles;
        this.Depth = depth;
        this.Tileset = tilesets[mapLayer.tileset_name];
        this.Data = mapLayer.GetDataGrid();
        this.CurrentSpriteName = NewMapInfo.MapTile.EMPTY_TILE_SPRITE_NAME;
        this.AutoTileEnabled = true;
        this.EraserEnabled = false;
        this.Visual = visual;
        _spriteDataDict = this.Tileset.GetSpriteDataDictionary();
        _autotileDict = this.Tileset.GetAutotileDictionary();
    }

    public void ApplyBrush(int x, int y)
    {
        this.Data[x, y].sprite_name = this.AutoTileEnabled ? getAutoTileSprite(x, y, !this.EraserEnabled) : this.CurrentSpriteName;

        if (this.AutoTileEnabled)
        {
            if (x > 0)
                this.Data[x - 1, y].sprite_name = getAutoTileSprite(x - 1, y, false);
            if (x < this.Data.GetLength(0) - 1)
                this.Data[x + 1, y].sprite_name = getAutoTileSprite(x + 1, y, false);
            if (y > 0)
                this.Data[x, y - 1].sprite_name = getAutoTileSprite(x, y - 1, false);
            if (y < this.Data.GetLength(1) - 1)
                this.Data[x, y + 1].sprite_name = getAutoTileSprite(x, y + 1, false);
        }

        this.ApplyData(x, y);
    }

    public void PreviewBrush(int x, int y)
    {
        if (x >= 0 && x < this.Data.GetLength(0) && y >=0 && y < this.Data.GetLength(1))
            this.Visual.SetSpriteIndexForTile(x, y, this.AutoTileEnabled ? getAutoTileSprite(x, y, !this.EraserEnabled) : this.CurrentSpriteName);
        //TODO: Preview surrounding autotile if enabled
    }

    public void ApplyData(int x, int y)
    {
        if (x < 0 || y < 0)
            return;

        int minX = x > 0 ? x - 1 : x;
        int minY = y > 0 ? y - 1 : y;
        int maxX = x < this.Data.GetLength(0) - 1 ? x + 1 : x;
        int maxY = y < this.Data.GetLength(1) - 1 ? y + 1 : y;

        for (int i = minX; i <= maxX; ++i)
        {
            for (int j = minY; j <= maxY; ++j)
            {
                this.Visual.SetSpriteIndexForTile(i, j, this.Data[i, j].sprite_name);
            }
        }
    }

    /**
     * Private
     */
    private Dictionary<string, TilesetData.SpriteData> _spriteDataDict;
    private Dictionary<TilesetData.TileType, List<TilesetData.SpriteData>> _autotileDict;

    private string getAutoTileSprite(int x, int y, bool forceFilled)
    {
        return TilesetData.GetAutotileSpriteName(TilesetData.GetAutotileType(x, y, this.Data, _spriteDataDict, forceFilled), _autotileDict, this.Tileset);
    }
}

public class MapEditorObjectsLayer : MapEditorLayer
{
    public List<GameObject> Objects;

    public MapEditorObjectsLayer(string name, int depth, List<GameObject> objects)
    {
        this.Name = name;
        this.Type = LayerType.Objects;
        this.Depth = depth;
        this.Objects = objects;
    }
}

public class MapEditorParallaxLayer : MapEditorLayer
{
    public string SpriteName;
    public bool Loops;
    public float Height;

    public MapEditorParallaxLayer(NewMapInfo.ParallaxLayer parallaxLayer, string name)
    {
        this.Name = name;
        this.Type = LayerType.Parallax;
        this.Depth = parallaxLayer.depth;
        this.SpriteName = parallaxLayer.sprite_name;
        this.Loops = parallaxLayer.loops;
        this.Height = parallaxLayer.height;
    }
}
