using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class MapEditorManager : MonoBehaviour, IPausable
{
    public const int DEFAULT_TILE_SIZE = 16;
    public const string PLATFORMS_LAYER = "platforms";
    public const string BACKGROUND_LAYER = "background";
    public const string OBJECTS_LAYER = "objects";
    public const string PROPS_LAYER = "props";

    public CameraController CameraController;
    public TileRenderer PlatformsRenderer;
    public TileRenderer BackgroundRenderer;
    public MapEditorGrid Grid;
    public MapEditorCursor Cursor;
    public Transform ObjectCursor;
    public TilesetCollection TilesetCollection;
    public string MapName;
    public Dictionary<string, MapEditorLayer> Layers;
    public string CurrentLayer;
    public LayerListPanel LayerListPanel;
    public ActivateAndAnimateImage SaveIcon;
    public ActivateAndAnimateImage FadeOut;
    public TimedCallbacks TimedCallbacks;
    public string WorldEditorSceneName = "WorldEditor";
    public float LoadTime = 1.0f;
    public GameObject[] ObjectPrefabs;
    public GameObject[] PropPrefabs;
    public Sprite[] ParallaxSprites;
    public ParallaxQuadGroup ParallaxVisualPrefab;
    public Transform ParallaxParent;

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

    public Texture2D GetAtlasForName(string atlasName)
    {
        return _atlases[atlasName];
    }

    void Awake()
    {
        if (ScenePersistentLoading.IsLoading)
            this.MapName = ScenePersistentLoading.ConsumeLoad().Value.LevelToLoad;
        _atlases = MapLoader.CompileTextures(validAtlases().ToArray());
        this.Layers = new Dictionary<string, MapEditorLayer>();
        this.CurrentLayer = PLATFORMS_LAYER;
        _previousCursorPos = new IntegerVector(-9999, -9999);
        _parallaxVisuals = new Dictionary<string, ParallaxQuadGroup>();

        // Load Data
        _mapInfo = MapLoader.GatherMapInfo(this.MapName);
        if (_mapInfo == null)
            _mapInfo = new NewMapInfo(this.MapName, DEFAULT_LEVEL_SIZE, DEFAULT_LEVEL_SIZE, DEFAULT_TILE_SIZE);

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
        _objectPrecisionIncrement = Mathf.Clamp(Mathf.RoundToInt(this.Grid.GridSpaceSize / OBJECT_PRECISION_PER_TILE), 1, this.Grid.GridSpaceSize);
        this.ObjectCursor.SetPosition2D(_mapInfo.width * this.Grid.GridSpaceSize / 2, _mapInfo.height * this.Grid.GridSpaceSize / 2);

        // Setup Object Layers
        this.Layers.Add(OBJECTS_LAYER, new MapEditorObjectsLayer(OBJECTS_LAYER, PLATFORMS_LAYER_DEPTH - LAYER_DEPTH_INCREMENT, _mapInfo.objects, this.ObjectPrefabs, _mapInfo.next_object_id));
        this.Layers.Add(PROPS_LAYER, new MapEditorObjectsLayer(PROPS_LAYER, PLATFORMS_LAYER_DEPTH + LAYER_DEPTH_INCREMENT, _mapInfo.props, this.PropPrefabs, _mapInfo.next_prop_id));

        // Setup Tile Layers
        this.Layers.Add(PLATFORMS_LAYER, new MapEditorTilesLayer(platformsLayerData, PLATFORMS_LAYER_DEPTH, _tilesets, this.PlatformsRenderer));
        this.Layers.Add(BACKGROUND_LAYER, new MapEditorTilesLayer(backgroundLayerData, PLATFORMS_LAYER_DEPTH + LAYER_DEPTH_INCREMENT * 2, _tilesets, this.BackgroundRenderer));

        // Setup Parallax Layers
        this.ParallaxParent.SetPosition2D(_mapInfo.width * this.Grid.GridSpaceSize / 2, _mapInfo.height * this.Grid.GridSpaceSize / 2);
        if (_mapInfo.parallax_layers.Count == 0)
        {
            int foregroundDepth = PLATFORMS_LAYER_DEPTH - 2 * LAYER_DEPTH_INCREMENT;
            _mapInfo.AddParallaxLayer(foregroundDepth);
            string foreground = PARALLAX_PREFIX + foregroundDepth;
            MapEditorParallaxLayer foregroundLayer = new MapEditorParallaxLayer(_mapInfo.GetParallaxLayer(foregroundDepth), foreground);
            this.Layers.Add(foreground, foregroundLayer);

            int backgroundDepth = PLATFORMS_LAYER_DEPTH + 3 * LAYER_DEPTH_INCREMENT;
            _mapInfo.AddParallaxLayer(backgroundDepth);
            string background = PARALLAX_PREFIX + backgroundDepth;
            MapEditorParallaxLayer backgroundLayer = new MapEditorParallaxLayer(_mapInfo.GetParallaxLayer(backgroundDepth), background);
            this.Layers.Add(background, backgroundLayer);

            ParallaxQuadGroup foregroundQuad = Instantiate<ParallaxQuadGroup>(this.ParallaxVisualPrefab);
            ParallaxQuadGroup backgroundQuad = Instantiate<ParallaxQuadGroup>(this.ParallaxVisualPrefab);
            foregroundQuad.CameraController = this.CameraController;
            backgroundQuad.CameraController = this.CameraController;
            foregroundQuad.transform.parent = this.ParallaxParent;
            backgroundQuad.transform.parent = this.ParallaxParent;
            foregroundQuad.transform.SetPosition(this.ParallaxParent.position.x, this.ParallaxParent.position.y, foregroundDepth);
            backgroundQuad.transform.SetPosition(this.ParallaxParent.position.x, this.ParallaxParent.position.y, backgroundDepth);
            _parallaxVisuals.Add(foreground, foregroundQuad);
            _parallaxVisuals.Add(background, backgroundQuad);
        }
        else
        {
            for (int i = 0; i < _mapInfo.parallax_layers.Count; ++i)
            {
                string name = PARALLAX_PREFIX + _mapInfo.parallax_layers[i].depth;
                MapEditorParallaxLayer layer = new MapEditorParallaxLayer(_mapInfo.parallax_layers[i], name);
                this.Layers.Add(name, layer);
                ParallaxQuadGroup quad = Instantiate<ParallaxQuadGroup>(this.ParallaxVisualPrefab);
                quad.CameraController = this.CameraController;
                quad.transform.SetPosition(this.ParallaxParent.position.x, this.ParallaxParent.position.y, _mapInfo.parallax_layers[i].depth);
                quad.transform.parent = this.ParallaxParent;
                _parallaxVisuals.Add(name, quad);
            }
        }

        // Handle Visuals
        _sortedLayers = this.DepthSortedLayers;
        this.LayerListPanel.ConfigureForLayers(_sortedLayers, this.CurrentLayer);
    }

    void Start()
    {
        updateVisuals();
        updateCurrentLayer();
    }

    void FixedUpdate()
    {
        if (_exiting)
            return;

        if (MapEditorInput.CycleNextAlt || MapEditorInput.CyclePrevAlt)
        {
            int currentLayerIndex = this.CurrentLayerIndex;
            currentLayerIndex = MapEditorInput.CycleNextAlt ? currentLayerIndex + 1 : currentLayerIndex - 1;
            if (currentLayerIndex >= _sortedLayers.Count)
                currentLayerIndex = 0;
            else if (currentLayerIndex < 0)
                currentLayerIndex = _sortedLayers.Count - 1;
            leaveLayer(this.CurrentLayer);
            this.CurrentLayer = _sortedLayers[currentLayerIndex];
            enterLayer(this.CurrentLayer);
            this.LayerListPanel.ChangeCurrentLayer(this.CurrentLayer);

            if (!this.Cursor.Hidden && this.Layers[this.CurrentLayer].Type != MapEditorLayer.LayerType.Tiles)
                this.Cursor.Hide();
            else if (this.Cursor.Hidden && this.Layers[this.CurrentLayer].Type == MapEditorLayer.LayerType.Tiles)
                this.Cursor.UnHide();
        }
        else if (MapEditorInput.Start)
        {
            this.Save();
        }
        else if (MapEditorInput.Exit)
        {
            _exiting = true;
            this.FadeOut.Run();
            this.Cursor.Hide();
            this.Save();
            this.TimedCallbacks.AddCallback(this, loadWorldEditor, this.LoadTime);
        }

        updateCurrentLayer();
    }

    public void Save()
    {
        this.SaveIcon.Run();
        foreach (MapEditorLayer layer in this.Layers.Values)
            layer.SaveData(_mapInfo);
        File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + this.MapName + MapLoader.JSON_SUFFIX, JsonConvert.SerializeObject(_mapInfo, Formatting.Indented));
    }

    public void HandleReturnFromMenu()
    {
        // Update visual data for current layer
        MapEditorLayer currentLayer = this.Layers[this.CurrentLayer];
        switch (currentLayer.Type)
        {
            default:
            case MapEditorLayer.LayerType.Tiles:
                break;
            case MapEditorLayer.LayerType.Objects:
                break;
            case MapEditorLayer.LayerType.Parallax:
                MapEditorParallaxLayer parallaxLayer = currentLayer as MapEditorParallaxLayer;
                _parallaxVisuals[this.CurrentLayer].CreateMeshForLayer(findParallaxSprite(parallaxLayer.SpriteName), parallaxLayer.Loops, parallaxLayer.Height, parallaxLayer.XPosition, parallaxLayer.ParallaxRatio, _mapInfo.width * this.Grid.GridSpaceSize);
                break;
        }
    }

    /**
     * Private
     */
    private NewMapInfo _mapInfo;
    private Dictionary<string, TilesetData> _tilesets;
    private Dictionary<string, Texture2D> _atlases;
    private IntegerVector _previousCursorPos;
    private List<string> _sortedLayers;
    private Dictionary<string, ParallaxQuadGroup> _parallaxVisuals;
    private bool _tileEraserEnabled;
    private bool _exiting;
    private int _objectPrecisionIncrement;

    private Sprite findParallaxSprite(string spriteName)
    {
        if (spriteName == null)
            return null;
        
        if (spriteName != null)
        {
            for (int i = 0; i < this.ParallaxSprites.Length; ++i)
            {
                if (this.ParallaxSprites[i].name == spriteName)
                    return this.ParallaxSprites[i];
            }
        }
        return null;
    }

    private void loadWorldEditor()
    {
        SceneManager.LoadScene(this.WorldEditorSceneName);
    }

    private void updateCurrentLayer()
    {
        MapEditorLayer currentLayer = this.Layers[this.CurrentLayer];
        if (currentLayer.Type == MapEditorLayer.LayerType.Tiles)
            updateTileLayer(currentLayer as MapEditorTilesLayer);
        else if (currentLayer.Type == MapEditorLayer.LayerType.Objects)
            updateObjectsLayer(currentLayer as MapEditorObjectsLayer);
    }

    private void enterLayer(string layerName)
    {
        MapEditorLayer layer = this.Layers[layerName];
        if (layer.Type == MapEditorLayer.LayerType.Tiles)
            enterTileLayer(layer as MapEditorTilesLayer);
        else if (layer.Type == MapEditorLayer.LayerType.Objects)
            enterObjectsLayer(layer as MapEditorObjectsLayer);
    }

    private void leaveLayer(string layerName)
    {
        MapEditorLayer layer = this.Layers[layerName];
        if (layer.Type == MapEditorLayer.LayerType.Tiles)
            leaveTileLayer(layer as MapEditorTilesLayer);
        else if (layer.Type == MapEditorLayer.LayerType.Objects)
            leaveObjectsLayer(layer as MapEditorObjectsLayer);
    }

    private void updateTileLayer(MapEditorTilesLayer layer)
    {
        bool newPos = false;
        if (_previousCursorPos != this.Cursor.GridPos)
        {
            newPos = true;
            (layer as MapEditorTilesLayer).ApplyData(_previousCursorPos.X, _previousCursorPos.Y);
            _previousCursorPos = this.Cursor.GridPos;
            (layer as MapEditorTilesLayer).PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
        }

        if (MapEditorInput.Confirm || (newPos && MapEditorInput.ConfirmHeld))
        {
            (layer as MapEditorTilesLayer).ApplyBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
        }
        else if (MapEditorInput.Cancel)
        {
            _tileEraserEnabled = !_tileEraserEnabled;
            this.Cursor.EnableEraser(_tileEraserEnabled);
            layer.EraserEnabled = _tileEraserEnabled;
            layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
        }
    }

    private void updateObjectsLayer(MapEditorObjectsLayer layer)
    {
        if (MapEditorInput.Confirm)
        {
            addObject(layer);
        }
        else if (MapEditorInput.NavLeft)
        {
            this.ObjectCursor.SetX(this.ObjectCursor.position.x - _objectPrecisionIncrement);
            if (this.ObjectCursor.position.x < _objectPrecisionIncrement)
                this.ObjectCursor.SetX(_mapInfo.width * this.Grid.GridSpaceSize - _objectPrecisionIncrement);
        }
        else if (MapEditorInput.NavRight)
        {
            this.ObjectCursor.SetX(this.ObjectCursor.position.x + _objectPrecisionIncrement);
            if (this.ObjectCursor.position.x > _mapInfo.width * this.Grid.GridSpaceSize - _objectPrecisionIncrement)
                this.ObjectCursor.SetX(_objectPrecisionIncrement);
        }
        else if (MapEditorInput.NavDown)
        {
            this.ObjectCursor.SetY(this.ObjectCursor.position.y - _objectPrecisionIncrement);
            if (this.ObjectCursor.position.y < _objectPrecisionIncrement)
                this.ObjectCursor.SetY(_mapInfo.height * this.Grid.GridSpaceSize - _objectPrecisionIncrement);
        }
        else if (MapEditorInput.NavUp)
        {
            this.ObjectCursor.SetY(this.ObjectCursor.position.y + _objectPrecisionIncrement);
            if (this.ObjectCursor.position.y > _mapInfo.height * this.Grid.GridSpaceSize - _objectPrecisionIncrement)
                this.ObjectCursor.SetY(_objectPrecisionIncrement);
        }
        else if (MapEditorInput.CyclePrev)
        {
            removeObjectBrush();
            layer.CyclePrev();
            addObjectBrush(layer);
        }
        else if (MapEditorInput.CycleNext)
        {
            removeObjectBrush();
            layer.CycleNext();
            addObjectBrush(layer);
        }
    }

    private void leaveTileLayer(MapEditorTilesLayer layer)
    {
        layer.EraserEnabled = false;
        layer.ApplyData(_previousCursorPos.X, _previousCursorPos.Y);
    }

    private void leaveObjectsLayer(MapEditorObjectsLayer layer)
    {
        removeObjectBrush();
    }

    private void enterTileLayer(MapEditorTilesLayer layer)
    {
        this.CameraController.SetTracker(this.Cursor.transform);
        layer.EraserEnabled = _tileEraserEnabled;
        layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
    }

    private void enterObjectsLayer(MapEditorObjectsLayer layer)
    {
        this.CameraController.SetTracker(this.ObjectCursor);
        this.ObjectCursor.SetZ(layer.Depth);
        addObjectBrush(layer);
    }

    private void removeObjectBrush()
    {
        Transform child = this.ObjectCursor.GetChild(0);
        this.ObjectCursor.DetachChildren();
        Destroy(child.gameObject);
    }

    private void addObjectBrush(MapEditorObjectsLayer layer)
    {
        GameObject child = Instantiate<GameObject>(layer.CurrentPrefab);
        child.transform.parent = this.ObjectCursor;
        child.transform.SetLocalPosition(0, 0, 0);
    }

    private void addObject(MapEditorObjectsLayer layer)
    {
        GameObject newObject = Instantiate<GameObject>(layer.CurrentPrefab);
        newObject.transform.SetPosition(this.ObjectCursor.position.x, this.ObjectCursor.position.y, layer.Depth);
        layer.AddObject(newObject);
    }

    private void loadObjects(MapEditorObjectsLayer layer)
    {
        for (int i = 0; i < layer.Objects.Count; ++i)
        {
            GameObject newObject = Instantiate<GameObject>(layer.PrefabForName(layer.Objects[i].prefab_name));
            newObject.name = layer.Objects[i].name;
            newObject.transform.SetPosition(layer.Objects[i].x, layer.Objects[i].y, layer.Depth);
            layer.LoadedObjects.Add(newObject);
        }
    }

    private void updateVisuals()
    {
        MapEditorTilesLayer platformsLayer = this.Layers[PLATFORMS_LAYER] as MapEditorTilesLayer;
        this.PlatformsRenderer.SetAtlas(_atlases[_tilesets[platformsLayer.Tileset.name].AtlasName]);
        this.PlatformsRenderer.CreateMapWithGrid(platformsLayer.Data);
        this.PlatformsRenderer.transform.SetZ(platformsLayer.Depth);

        MapEditorTilesLayer backgroundLayer = this.Layers[BACKGROUND_LAYER] as MapEditorTilesLayer;
        this.BackgroundRenderer.SetAtlas(_atlases[_tilesets[backgroundLayer.Tileset.name].AtlasName]);
        this.BackgroundRenderer.CreateMapWithGrid(backgroundLayer.Data);
        this.BackgroundRenderer.transform.SetZ(backgroundLayer.Depth);

        MapEditorObjectsLayer objectsLayer = this.Layers[OBJECTS_LAYER] as MapEditorObjectsLayer;
        loadObjects(objectsLayer);

        MapEditorObjectsLayer propsLayer = this.Layers[PROPS_LAYER] as MapEditorObjectsLayer;
        loadObjects(propsLayer);

        for (int i = 0; i < _mapInfo.parallax_layers.Count; ++i)
        {
            string name = PARALLAX_PREFIX + _mapInfo.parallax_layers[i].depth;
            MapEditorParallaxLayer layer = this.Layers[name] as MapEditorParallaxLayer;
            _parallaxVisuals[name].CreateMeshForLayer(findParallaxSprite(layer.SpriteName), layer.Loops, layer.Height, layer.XPosition, layer.ParallaxRatio, _mapInfo.width * this.Grid.GridSpaceSize);
        }
    }

    private int depthCompareLayers(string l1, string l2)
    {
        return Mathf.Clamp(this.Layers[l1].Depth - this.Layers[l2].Depth, -1, 1);
    }

    private List<Texture2D> validAtlases()
    {
        List<Texture2D> atlases = new List<Texture2D>();
        _tilesets = new Dictionary<string, TilesetData>();
        for (int i = 0; i < this.TilesetCollection.Tilesets.Length; ++i)
        {
            _tilesets.Add(this.TilesetCollection.Tilesets[i].name, this.TilesetCollection.Tilesets[i]);
            atlases.Add(Resources.Load<Texture2D>(this.TilesetCollection.Tilesets[i].AtlasName));
        }
        return atlases;
    }

    private const int DEFAULT_LEVEL_SIZE = 32;
    private const int PLATFORMS_LAYER_DEPTH = 0;
    private const int LAYER_DEPTH_INCREMENT = 2;
    private const int OBJECT_PRECISION_PER_TILE = 8;
    private const string PARALLAX_PREFIX = "parallax_";
    private const string DEFAULT_PLATFORMS_TILESET = "GenericPlatforms";
    private const string DEFAULT_BACKGROUND_TILESET = "GenericBackground";
}
