using UnityEngine;
using System.Collections.Generic;

public abstract class MapEditorLayer
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

    public abstract void SaveData(NewMapInfo mapInfo);
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
        guaranteeFillFlags();
    }

    public void ApplyBrush(int x, int y)
    {
        string spriteName = getBrushSprite(x, y);
        this.Data[x, y].sprite_name = spriteName;
        this.Data[x, y].is_filled = shouldBeFilled(spriteName);

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
            this.Visual.SetSpriteIndexForTile(x, y, getBrushSprite(x, y));
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

    public override void SaveData(NewMapInfo mapInfo)
    {
        guaranteeFillFlags();
        NewMapInfo.MapLayer layer = mapInfo.GetMapLayer(this.Name);
        layer.SetDataGrid(this.Data);
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

    private string getBrushSprite(int x, int y)
    {
        if (this.EraserEnabled)
            return this.Tileset.GetEmptySpriteName();
        if (this.AutoTileEnabled)
            return this.getAutoTileSprite(x, y, true);
        return this.CurrentSpriteName;
    }

    private bool shouldBeFilled(string spriteName)
    {
        return spriteName != this.Tileset.GetEmptySpriteName() && spriteName != NewMapInfo.MapTile.EMPTY_TILE_SPRITE_NAME;
    }

    private void guaranteeFillFlags()
    {
        for (int x = 0; x < this.Data.GetLength(0); ++x)
        {
            for (int y = 0; y < this.Data.GetLength(1); ++y)
            {
                this.Data[x, y].is_filled = shouldBeFilled(this.Data[x, y].sprite_name);
            }
        }
    }
}

public class MapEditorObjectsLayer : MapEditorLayer
{
    public List<NewMapInfo.MapObject> Objects;
    public GameObject[] ObjectPrefabs;
    public GameObject CurrentPrefab { get { return this.ObjectPrefabs[_currentPrefab]; } }
    public List<GameObject> LoadedObjects;

    public MapEditorObjectsLayer(string name, int depth, List<NewMapInfo.MapObject> objects, GameObject[] prefabs, int nextId)
    {
        this.Name = name;
        this.Type = LayerType.Objects;
        this.Depth = depth;
        this.Objects = objects;
        _currentPrefab = 0;
        this.ObjectPrefabs = prefabs;
        this.LoadedObjects = new List<GameObject>();
        _nextId = nextId;
    }

    public void CyclePrev()
    {
        --_currentPrefab;
        if (_currentPrefab < 0)
            _currentPrefab = this.ObjectPrefabs.Length - 1;
    }

    public void CycleNext()
    {
        ++_currentPrefab;
        if (_currentPrefab >= this.ObjectPrefabs.Length)
            _currentPrefab = 0;
    }

    public void AddObject(GameObject gameObject)
    {
        string prefabName = this.CurrentPrefab.name;
        gameObject.name = prefabName + "_" + _nextId;
        ++_nextId;
        if (_nextId == int.MaxValue)
            _nextId = 0;
        NewMapInfo.MapObject mapObject = new NewMapInfo.MapObject();
        mapObject.name = gameObject.name;
        mapObject.prefab_name = prefabName;
        mapObject.x = Mathf.RoundToInt(gameObject.transform.position.x);
        mapObject.y = Mathf.RoundToInt(gameObject.transform.position.y);
        mapObject.z = this.Depth;
        this.Objects.Add(mapObject);
    }

    public void RemoveObject(GameObject toRemove)
    {
        this.Objects.RemoveAll(o => o.name == toRemove.name);
        this.LoadedObjects.Remove(toRemove);
    }

    public GameObject PrefabForName(string prefabName)
    {
        for (int i = 0; i < this.ObjectPrefabs.Length; ++i)
        {
            if (this.ObjectPrefabs[i].name == prefabName)
                return this.ObjectPrefabs[i];
        }
        return null;
    }

    public override void SaveData(NewMapInfo mapInfo)
    {
        if (this.Name == MapEditorManager.OBJECTS_LAYER)
        {
            mapInfo.objects = this.Objects;
            mapInfo.next_object_id = _nextId;
        }
        else
        {
            mapInfo.props = this.Objects;
            mapInfo.next_prop_id = _nextId;
        }
    }

    /**
     * Private
     */
    private int _currentPrefab;
    private int _nextId;
}

public class MapEditorParallaxLayer : MapEditorLayer
{
    public string SpriteName;
    public bool Loops;
    public float Height;
    public float XPosition;
    public float ParallaxRatio;

    public MapEditorParallaxLayer(NewMapInfo.ParallaxLayer parallaxLayer, string name)
    {
        this.Name = name;
        this.Type = LayerType.Parallax;
        this.Depth = parallaxLayer.depth;
        this.SpriteName = parallaxLayer.sprite_name;
        this.Loops = parallaxLayer.loops;
        this.Height = parallaxLayer.height;
        this.XPosition = parallaxLayer.x_position;
        this.ParallaxRatio = parallaxLayer.parallax_ratio;
    }

    public override void SaveData(NewMapInfo mapInfo)
    {
        NewMapInfo.ParallaxLayer layer = mapInfo.GetParallaxLayer(this.Depth);
        layer.sprite_name = this.SpriteName;
        layer.loops = this.Loops;
        layer.height = this.Height;
        layer.x_position = this.XPosition;
        layer.parallax_ratio = this.ParallaxRatio;
    }
}
