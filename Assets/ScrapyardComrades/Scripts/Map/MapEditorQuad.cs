using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class MapEditorQuad : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public string DirectoryPath = "Levels";
    public MapEditorGrid Grid;
    public TileRenderer PlatformsRenderer;
    public TileRenderer BGRenderer;
    public string PlatformsLayer = "platforms";
    public string BGLayer = "background";
    public string ObjectsLayer = "objects";
    public bool LoadOnStart = false;
    public bool FlipVertical = true;

    public bool Cleared { get { return _cleared; } }
    public Dictionary<string, Texture2D> Atlases;
    public Dictionary<string, Sprite[]> Sprites;

    void Start()
    {
        if (this.LoadOnStart)
            this.LoadMap();
    }

    public void LoadMap()
    {
        this.ClearMap();
        _cleared = false;

        this.Load();
        MapGridSpaceInfo[,] grid = _mapInfo.GetLayerWithName(this.PlatformsLayer).GetGrid(_mapInfo.tilesets);
        _width = _mapInfo.width;
        _height = _mapInfo.height;
        this.Grid.InitializeGridForSize(_width, _height);
        this.PlatformsRenderer.Atlas = this.Atlases[this.PlatformsLayer];
        this.PlatformsRenderer.Sprites = this.Sprites[this.PlatformsLayer];
        this.PlatformsRenderer.FlipVertical = this.FlipVertical;
        this.PlatformsRenderer.CreateMapWithGrid(grid);
        MapInfo.MapLayer bgLayer = _mapInfo.GetLayerWithName(this.BGLayer);
        if (bgLayer != null)
        {
            MapGridSpaceInfo[,] bgGrid = bgLayer.GetGrid(_mapInfo.tilesets);
            this.BGRenderer.Atlas = this.Atlases[this.BGLayer];
            this.BGRenderer.Sprites = this.Sprites[this.BGLayer];
            this.BGRenderer.FlipVertical = this.FlipVertical;
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

    public void Load()
    {
        string path = Application.streamingAssetsPath + "/" + this.DirectoryPath + "/" + this.MapName;
        if (File.Exists(path))
        {
            this.FlipVertical = false;
            _mapInfo = JsonConvert.DeserializeObject<MapInfo>(File.ReadAllText(path));
        }
        else
        {
            this.FlipVertical = true;
            _mapInfo = MapLoader.GatherMapInfo(this.MapName);
        }
    }

    public void Save()
    {
        //TODO: complete

        string path = Application.streamingAssetsPath + "/" + this.DirectoryPath + "/" + this.MapName;
        File.WriteAllText(path, JsonConvert.SerializeObject(_mapInfo));
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
    private bool _cleared = false;
    private int _width = 0;
    private int _height = 0;
    private MapInfo _mapInfo;
}
