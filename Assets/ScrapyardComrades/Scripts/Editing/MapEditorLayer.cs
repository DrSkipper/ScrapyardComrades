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
        Parallax,
        Lighting
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

    public BrushState CurrentBrushState;
    public string[,] GroupBrush;

    public IntegerVector GroupBrushLowerLeft { get { return _groupBrushLowerLeft; } }
    public IntegerVector GroupBrushUpperRight { get { return _groupBrushUpperRight; } }

    public enum BrushState
    {
        SinglePaint,
        GroupSet,
        GroupPaint
    }

    public Dictionary<string, TilesetData.SpriteData> SpriteDataDict { get { return _spriteDataDict; } }

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
        this.CurrentBrushState = BrushState.SinglePaint;
    }

    public void ApplyBrush(int x, int y)
    {
        if (this.CurrentBrushState == BrushState.GroupPaint)
        {
            paintGroup(x, y);
        }
        else if (this.CurrentBrushState == BrushState.SinglePaint)
        {
            paintSprite(x, y, getBrushSprite(x, y), this.AutoTileEnabled);
        }
        
        this.ApplyData(x, y);
    }

    public void PreviewBrush(int x, int y)
    {
        if (x >= 0 && x < this.Data.GetLength(0) && y >= 0 && y < this.Data.GetLength(1))
        {
            if (this.CurrentBrushState == BrushState.GroupPaint)
            {
                previewGroup(x, y);
            }
            else if (this.CurrentBrushState == BrushState.SinglePaint)
            {
                this.Visual.SetSpriteIndexForTile(x, y, getBrushSprite(x, y));
            }
        }
    }

    public void BeginGroupSet(int x, int y)
    {
        this.CurrentBrushState = BrushState.GroupSet;
        _groupBrushLowerLeft.X = x;
        _groupBrushLowerLeft.Y = y;
        _groupBrushUpperRight.X = x;
        _groupBrushUpperRight.Y = y;
        grabGroup();
    }

    public void UpdateGroupSet(int x, int y)
    {
        if (x < _groupBrushLowerLeft.X)
            _groupBrushLowerLeft.X = x;
        else if (x > _groupBrushUpperRight.X)
            _groupBrushUpperRight.X = x;
        if (y < _groupBrushLowerLeft.Y)
            _groupBrushLowerLeft.Y = y;
        else if (y > _groupBrushUpperRight.Y)
            _groupBrushUpperRight.Y = y;
        grabGroup();
    }

    public IntegerVector EndGroupSet(int x, int y)
    {
        this.CurrentBrushState = this.GroupBrush.GetLength(0) > 1 || this.GroupBrush.GetLength(1) > 1 ? BrushState.GroupPaint : BrushState.SinglePaint;
        return _groupBrushLowerLeft;
    }

    public void ApplyData(int x, int y)
    {
        if (x < 0 || y < 0)
            return;

        if (this.CurrentBrushState == BrushState.GroupPaint)
            applyGroupData(x, y);
        else
            applyIndividualData(x, y);
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
    private List<int> _groupXs = new List<int>();
    private List<int> _groupYs = new List<int>();
    private List<string> _groupSpriteNames = new List<string>();
    public IntegerVector _groupBrushLowerLeft;
    public IntegerVector _groupBrushUpperRight;

    private void applyGroupData(int x, int y)
    {
        for (int i = 0; i < this.GroupBrush.GetLength(0); ++i)
        {
            for (int j = 0; j < this.GroupBrush.GetLength(1); ++j)
            {
                if (x + i < this.Data.GetLength(0) && y + j < this.Data.GetLength(1))
                    applyIndividualData(x + i, y + j);
            }
        }
    }

    private void applyIndividualData(int x, int y)
    {
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

    private void grabGroup()
    {
        this.GroupBrush = new string[_groupBrushUpperRight.X - _groupBrushLowerLeft.X + 1, _groupBrushUpperRight.Y - _groupBrushLowerLeft.Y + 1];
        for (int i = 0; i < this.GroupBrush.GetLength(0); ++i)
        {
            for (int j = 0; j < this.GroupBrush.GetLength(1); ++j)
            {
                this.GroupBrush[i, j] = this.Data[_groupBrushLowerLeft.X + i, _groupBrushLowerLeft.Y + j].sprite_name;
            }
        }
    }

    private void paintGroup(int x, int y)
    {
        for (int i = 0; i < this.GroupBrush.GetLength(0); ++i)
        {
            for (int j = 0; j < this.GroupBrush.GetLength(1); ++j)
            {
                if (x + i < this.Data.GetLength(0) && y + j < this.Data.GetLength(1))
                    paintSprite(x + i, y + j, this.GroupBrush[i, j], false);
            }
        }
    }

    private void paintSprite(int x, int y, string spriteName, bool autotile)
    {
        this.Data[x, y].sprite_name = spriteName;
        this.Data[x, y].is_filled = shouldBeFilled(spriteName);

        if (autotile)
        {
            if (x > 0)
                this.Data[x - 1, y].sprite_name = getAutoTileSprite(x - 1, y, false);
            if (x < this.Data.GetLength(0) - 1)
                this.Data[x + 1, y].sprite_name = getAutoTileSprite(x + 1, y, false);
            if (y > 0)
                this.Data[x, y - 1].sprite_name = getAutoTileSprite(x, y - 1, false);
            if (y < this.Data.GetLength(1) - 1)
                this.Data[x, y + 1].sprite_name = getAutoTileSprite(x, y + 1, false);
            if (x > 0 && y > 0)
                this.Data[x - 1, y - 1].sprite_name = getAutoTileSprite(x - 1, y - 1, false);
            if (x < this.Data.GetLength(0) - 1 && y > 0)
                this.Data[x + 1, y - 1].sprite_name = getAutoTileSprite(x + 1, y - 1, false);
            if (x > 0 && y < this.Data.GetLength(1) - 1)
                this.Data[x - 1, y + 1].sprite_name = getAutoTileSprite(x - 1, y + 1, false);
            if (x < this.Data.GetLength(0) - 1 && y < this.Data.GetLength(1) - 1)
                this.Data[x + 1, y + 1].sprite_name = getAutoTileSprite(x + 1, y + 1, false);
        }
    }

    private void previewGroup(int x, int y)
    {
        _groupXs.Clear();
        _groupYs.Clear();
        _groupSpriteNames.Clear();
        for (int i = 0; i < this.GroupBrush.GetLength(0); ++i)
        {
            for (int j = 0; j < this.GroupBrush.GetLength(1); ++j)
            {
                if (x + i < this.Data.GetLength(0) && y + j < this.Data.GetLength(1))
                {
                    _groupXs.Add(x + i);
                    _groupYs.Add(y + j);
                    _groupSpriteNames.Add(this.GroupBrush[i, j]);
                }
            }
        }
        this.Visual.SetSpriteIndicesForTiles(_groupXs.ToArray(), _groupYs.ToArray(), _groupSpriteNames.ToArray());
    }

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
    public Object[] ObjectPrefabs;
    public Object CurrentPrefab { get { return this.ObjectPrefabs[_currentPrefab]; } }
    public List<GameObject> LoadedObjects;
    public bool EraserEnabled;

    public MapEditorObjectsLayer(string name, int depth, List<NewMapInfo.MapObject> objects, Object[] prefabs, int nextId)
    {
        this.Name = name;
        this.Type = LayerType.Objects;
        this.Depth = depth;
        this.Objects = objects;
        _currentPrefab = 0;
        this.ObjectPrefabs = prefabs;
        this.LoadedObjects = new List<GameObject>();
        _nextId = nextId;
        this.EraserEnabled = false;
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
        gameObject.name = this.Name + UNDERSCORE + prefabName + UNDERSCORE + _nextId;
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
        this.LoadedObjects.Add(gameObject);
    }

    public void RemoveObject(GameObject toRemove)
    {
        this.Objects.RemoveAll(o => o.name == toRemove.name);
        this.LoadedObjects.Remove(toRemove);
    }

    public Object PrefabForName(string prefabName)
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
    public const string UNDERSCORE = "_";
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

public class MapEditorLightingLayer : MapEditorLayer
{
    public NewMapInfo.MapLight CurrentProperties;
    public List<NewMapInfo.MapLight> Lights;
    public List<GameObject> LoadedLights;
    public bool EraserEnabled;

    public MapEditorLightingLayer(string name, int depth, List<NewMapInfo.MapLight> lights, PooledObject prefab, int nextId)
    {
        this.Name = name;
        this.Type = LayerType.Lighting;
        this.Depth = depth;
        this.Lights = lights != null ? lights : new List<NewMapInfo.MapLight>();
        _prefab = prefab;
        this.EraserEnabled = false;
        this.LoadedLights = new List<GameObject>();
        _nextId = nextId;

        this.CurrentProperties = new NewMapInfo.MapLight();
        if (this.Lights.Count > 0)
        {
            this.CopyValues(this.CurrentProperties, this.Lights[0]);
        }
        else
        {
            this.CurrentProperties.light_type = (int)LightType.Point;
        }
    }

    public void ApplyBrush(Transform cursor)
    {
        PooledObject light = _prefab.Retain();
        light.GetComponent<SCLight>().ConfigureLight(this.CurrentProperties);
        light.transform.parent = cursor;
        light.transform.SetLocalPosition(0, 0, 0);
    }

    public void AddObject(GameObject gameObject)
    {
        string prefabName = _prefab.name;
        gameObject.name = this.Name + MapEditorObjectsLayer.UNDERSCORE + prefabName + MapEditorObjectsLayer.UNDERSCORE + _nextId;
        ++_nextId;
        if (_nextId == int.MaxValue)
            _nextId = 0;

        NewMapInfo.MapLight mapLight = new NewMapInfo.MapLight();
        mapLight.name = gameObject.name;
        mapLight.x = Mathf.RoundToInt(gameObject.transform.position.x);
        mapLight.y = Mathf.RoundToInt(gameObject.transform.position.y);

        this.CopyValues(mapLight, this.CurrentProperties);

        gameObject.GetComponent<SCLight>().ConfigureLight(mapLight);
        this.Lights.Add(mapLight);
        this.LoadedLights.Add(gameObject);
    }

    public void CopyValues(NewMapInfo.MapLight receiver, NewMapInfo.MapLight source)
    {
        receiver.light_type = source.light_type;
        receiver.intensity = source.intensity;
        receiver.range = source.range;
        receiver.distance = source.distance;
        receiver.spot_angle = source.spot_angle;
        receiver.rot_x = source.rot_x;
        receiver.rot_y = source.rot_y;
        receiver.r = source.r;
        receiver.g = source.g;
        receiver.b = source.b;
    }

    public void RemoveObject(GameObject toRemove)
    {
        this.Lights.RemoveAll(o => o.name == toRemove.name);
        this.LoadedLights.Remove(toRemove);
    }

    public override void SaveData(NewMapInfo mapInfo)
    {
        mapInfo.lights = this.Lights;
        mapInfo.next_light_id = _nextId;
    }

    /**
     * Private
     */
    private PooledObject _prefab;
    private int _nextId;
}