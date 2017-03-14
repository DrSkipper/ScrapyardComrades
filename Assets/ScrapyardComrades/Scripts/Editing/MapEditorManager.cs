using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class MapEditorManager : MonoBehaviour
{
    public TileRenderer PlatformsRenderer;
    public TileRenderer BackgroundRenderer;
    public MapEditorGrid Grid;
    public MapEditorCursor Cursor;
    public TilesetData[] Tilesets;
    public string MapName;
    public Dictionary<string, MapEditorLayer> Layers;
    public string CurrentLayer;
    public LayerListPanel LayerListPanel;

    public List<string> DepthSortedLayers
    {
        get
        {
            List<string> retval = new List<string>(this.Layers.Keys);
            retval.Sort(depthCompareLayers);
            return retval;
        }
    }

    public int CurrentLayerIndex { get { for (int i = 0; i < _sortedLayers.Count; ++i) if (_sortedLayers[i] == this.CurrentLayer) return i; return 0; } }

    void Awake()
    {
        _atlases = MapLoader.CompileTextures(validAtlases().ToArray());
        _sprites = MapLoader.CompileSprites(_atlases);
        this.Layers = new Dictionary<string, MapEditorLayer>();
        this.CurrentLayer = PLATFORMS_LAYER;
        _previousCursorPos = new IntegerVector(-9999, -9999);
        
        // Load Data
        string path = Application.streamingAssetsPath + LEVELS_PATH + this.MapName;
        if (File.Exists(path))
        {
            _mapInfo = JsonConvert.DeserializeObject<NewMapInfo>(path);
        }
        else
        {
            _mapInfo = new NewMapInfo(this.MapName, DEFAULT_LEVEL_SIZE, DEFAULT_LEVEL_SIZE, DEFAULT_TILE_SIZE);
        }

        NewMapInfo.MapLayer platformsLayerData = _mapInfo.GetMapLayer(PLATFORMS_LAYER);
        if (platformsLayerData == null)
        {
            _mapInfo.AddTileLayer(PLATFORMS_LAYER);
            platformsLayerData = _mapInfo.GetMapLayer(PLATFORMS_LAYER);
            platformsLayerData.tileset_name = DEFAULT_PLATFORMS_TILESET;
        }

        NewMapInfo.MapLayer backgroundLayerData = _mapInfo.GetMapLayer(BACKGROUND_LAYER);
        if (backgroundLayerData == null)
        {
            _mapInfo.AddTileLayer(BACKGROUND_LAYER);
            backgroundLayerData = _mapInfo.GetMapLayer(BACKGROUND_LAYER);
            backgroundLayerData.tileset_name = DEFAULT_BACKGROUND_TILESET;
        }

        // Setup Grid
        this.Grid.InitializeGridForSize(_mapInfo.width, _mapInfo.height);
        
        // Setup Tile Layers
        this.Layers.Add(PLATFORMS_LAYER, new MapEditorTilesLayer(platformsLayerData, PLATFORMS_LAYER_DEPTH, _tilesets, this.PlatformsRenderer));
        this.Layers.Add(BACKGROUND_LAYER, new MapEditorTilesLayer(backgroundLayerData, PLATFORMS_LAYER_DEPTH + LAYER_DEPTH_INCREMENT, _tilesets, this.PlatformsRenderer));

        //TODO: Setup Object Layers

        // Setup Parallax Layers
        for (int i = 0; i < _mapInfo.parallax_layers.Count; ++i)
        {
            string name = PARALLAX_PREFIX + _mapInfo.parallax_layers[i].depth;
            this.Layers.Add(name, new MapEditorParallaxLayer(_mapInfo.parallax_layers[i], name));
        }

        // Handle Visuals
        _sortedLayers = this.DepthSortedLayers;
        this.LayerListPanel.ConfigureForLayers(_sortedLayers, this.CurrentLayer);
        updateVisuals();
        updateCurrentLayer();
    }

    void FixedUpdate()
    {
        if (MapEditorInput.SwapModes)
        {
            int currentLayerIndex = this.CurrentLayerIndex;
            ++currentLayerIndex;
            if (currentLayerIndex == _sortedLayers.Count)
                currentLayerIndex = 0;
            this.CurrentLayer = _sortedLayers[currentLayerIndex];
            this.LayerListPanel.ChangeCurrentLayer(this.CurrentLayer);
        }

        updateCurrentLayer();
    }

    /**
     * Private
     */
    private NewMapInfo _mapInfo;
    private Dictionary<string, TilesetData> _tilesets;
    private Dictionary<string, Texture2D> _atlases;
    private Dictionary<string, Sprite[]> _sprites;
    private IntegerVector _previousCursorPos;
    private List<string> _sortedLayers;

    private void updateCurrentLayer()
    {
        MapEditorLayer currentLayer = this.Layers[this.CurrentLayer];
        if (currentLayer.Type == MapEditorLayer.LayerType.Tiles)
            updateTileLayer(currentLayer as MapEditorTilesLayer);
    }

    private void updateTileLayer(MapEditorTilesLayer layer)
    {
        if (layer.Type == MapEditorLayer.LayerType.Tiles)
        {
            if (_previousCursorPos != this.Cursor.GridPos)
            {
                (layer as MapEditorTilesLayer).ApplyData(_previousCursorPos.X, _previousCursorPos.Y);
                _previousCursorPos = this.Cursor.GridPos;
                (layer as MapEditorTilesLayer).PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
            }

            if (MapEditorInput.ConfirmHeld)
            {
                (layer as MapEditorTilesLayer).ApplyBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
            }
        }
    }

    private void updateVisuals()
    {
        MapEditorTilesLayer platformsLayer = this.Layers[PLATFORMS_LAYER] as MapEditorTilesLayer;
        this.PlatformsRenderer.SetAtlas(_atlases[_tilesets[platformsLayer.Tileset.name].AtlasName]);
        this.PlatformsRenderer.CreateMapWithGrid(platformsLayer.Data);

        MapEditorTilesLayer backgroundLayer = this.Layers[BACKGROUND_LAYER] as MapEditorTilesLayer;
        this.BackgroundRenderer.SetAtlas(_atlases[_tilesets[backgroundLayer.Tileset.name].AtlasName]);
        this.BackgroundRenderer.CreateMapWithGrid(backgroundLayer.Data);

        //TODO: Parallax, props, objects
    }

    private int depthCompareLayers(string l1, string l2)
    {
        return Mathf.Clamp(this.Layers[l1].Depth - this.Layers[l2].Depth, -1, 1);
    }

    private List<Texture2D> validAtlases()
    {
        List<Texture2D> atlases = new List<Texture2D>();
        _tilesets = new Dictionary<string, TilesetData>();
        for (int i = 0; i < this.Tilesets.Length; ++i)
        {
            _tilesets.Add(this.Tilesets[i].name, this.Tilesets[i]);
            atlases.Add(Resources.Load<Texture2D>(this.Tilesets[i].AtlasName));
        }
        return atlases;
    }

    private const int DEFAULT_TILE_SIZE = 16;
    private const int DEFAULT_LEVEL_SIZE = 32;
    private const int PLATFORMS_LAYER_DEPTH = 0;
    private const int LAYER_DEPTH_INCREMENT = 2;
    private const string LEVELS_PATH = "/Levels/";
    private const string PLATFORMS_LAYER = "platforms";
    private const string BACKGROUND_LAYER = "background";
    private const string OBJECTS_LAYER = "objects";
    private const string PROPS_LAYER = "props";
    private const string PARALLAX_PREFIX = "parallax_";
    private const string DEFAULT_PLATFORMS_TILESET = "GenericPlatforms";
    private const string DEFAULT_BACKGROUND_TILESET = "GenericBackground";
}
