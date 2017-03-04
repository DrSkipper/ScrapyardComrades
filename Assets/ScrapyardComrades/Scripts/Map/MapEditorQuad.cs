using UnityEngine;
using System.Collections.Generic;

public class MapEditorQuad : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public MapEditorGrid Grid;
    public TileRenderer PlatformsRenderer;
    public TileRenderer BGRenderer;
    public Texture2D[] ValidAtlases;
    public string PlatformsLayer = "platforms";
    public string BGLayer = "background";
    public string ObjectsLayer = "objects";
    public bool LoadOnStart = false;

    public bool Cleared { get { return _cleared; } }

    void Start()
    {
        if (this.LoadOnStart)
            this.LoadMap();
    }

    public void LoadMap(bool loadObjects = false)
    {
        this.ClearMap();
        _cleared = false;

        if (_atlases == null || _atlases.Count == 0)
        {
            _atlases = MapLoader.CompileTextures(this.ValidAtlases);
            _sprites = MapLoader.CompileSprites(_atlases);
        }

        MapInfo mapInfo = MapLoader.GatherMapInfo(this.MapName);
        MapGridSpaceInfo[,] grid = mapInfo.GetLayerWithName(this.PlatformsLayer).GetGrid(mapInfo.tilesets);
        _width = mapInfo.width;
        _height = mapInfo.height;
        this.Grid.InitializeGridForSize(_width, _height);
        this.PlatformsRenderer.Atlas = _atlases[this.PlatformsLayer];
        this.PlatformsRenderer.Sprites = _sprites[this.PlatformsLayer];
        this.PlatformsRenderer.CreateMapWithGrid(grid);
        MapInfo.MapLayer bgLayer = mapInfo.GetLayerWithName(this.BGLayer);
        if (bgLayer != null)
        {
            MapGridSpaceInfo[,] bgGrid = bgLayer.GetGrid(mapInfo.tilesets);
            this.BGRenderer.Atlas = _atlases[this.BGLayer];
            this.BGRenderer.Sprites = _sprites[this.BGLayer];
            this.BGRenderer.CreateMapWithGrid(bgGrid);
        }
    }

    public void ClearMap(bool editor = false)
    {
        if (!_cleared)
        {
            _cleared = true;
            if (editor)
            {
                this.PlatformsRenderer.Clear();
                this.BGRenderer.Clear();
            }
            _width = 0;
            _height = 0;
        }
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
    private Dictionary<string, Texture2D> _atlases;
    private Dictionary<string, Sprite[]> _sprites;
    private bool _cleared = false;
    private int _width = 0;
    private int _height = 0;
}
