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
    public const string PROPS_BACK_LAYER = "props_back";
    public const string PROPS_PATH = "Props/";
    public const string LIGHTING_LAYER = "lights";
    public const string PARALLAX_BACK_SORT_LAYER = "parallax_back";
    public const string PARALLAX_FRONT_SORT_LAYER = "parallax_front";
    public const int PLATFORMS_LAYER_DEPTH = 0;

    public CameraController CameraController;
    public TileRenderer PlatformsRenderer;
    public TileRenderer BackgroundRenderer;
    public MapEditorGrid Grid;
    public MapEditorCursor Cursor;
    public Transform ObjectCursor;
    public MapEditorBorder SelectionBorder;
    public LineRenderer ObjectEraseLine;
    //public Color EraseColor;
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
    public PrefabCollection ObjectPrefabs;
    public PrefabCollection PropPrefabs;
    public PooledObject SpriteObjectPrefab;
    public ParallaxQuadGroup ParallaxVisualPrefab;
    public Shader ParallaxLitShader;
    public Shader ParallaxUnlitShader;
    public Transform ParallaxParent;
    public PooledObject LightPrefab;
    public Color[] ValidLightColors;

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

    public List<Sprite> ParallaxSprites { get { return _parallaxSprites; } }

    void Awake()
    {
        if (ScenePersistentLoading.IsLoading)
            this.MapName = ScenePersistentLoading.ConsumeLoad().Value.LevelToLoad;
        this.Layers = new Dictionary<string, MapEditorLayer>();
        this.CurrentLayer = PLATFORMS_LAYER;
        _previousCursorPos = new IntegerVector(-9999, -9999);
        _parallaxVisuals = new Dictionary<string, ParallaxQuadGroup>();
        compileTilesets();
        _parallaxSprites = IndexedSpriteManager.GetAllSpritesAtPath(ParallaxLayerController.PARALLAX_PATH);

        this.ObjectEraseLine.sortingLayerName = LIGHTING_LAYER;

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
        this.Layers.Add(OBJECTS_LAYER, new MapEditorObjectsLayer(OBJECTS_LAYER, PLATFORMS_LAYER_DEPTH - LAYER_DEPTH_INCREMENT, _mapInfo.objects, this.ObjectPrefabs.Prefabs.ToArray(), _mapInfo.next_object_id));
        List<Object> props = new List<Object>(IndexedSpriteManager.GetAllSpritesAtPath(PROPS_PATH).ToArray());
        if (this.PropPrefabs.Prefabs != null)
        {
            for (int i = 0; i < this.PropPrefabs.Prefabs.Count; ++i)
                props.Add(this.PropPrefabs.Prefabs[i].gameObject);
        }
        this.Layers.Add(PROPS_LAYER, new MapEditorObjectsLayer(PROPS_LAYER, PLATFORMS_LAYER_DEPTH + LAYER_DEPTH_INCREMENT, _mapInfo.props, props.ToArray(), _mapInfo.next_prop_id));
        this.Layers.Add(PROPS_BACK_LAYER, new MapEditorObjectsLayer(PROPS_BACK_LAYER, PLATFORMS_LAYER_DEPTH + LAYER_DEPTH_INCREMENT * 3, _mapInfo.props_background, props.ToArray(), _mapInfo.next_prop_bg_id));

        // Setup Lighting layer
        this.Layers.Add(LIGHTING_LAYER, new MapEditorLightingLayer(LIGHTING_LAYER, PLATFORMS_LAYER_DEPTH - LAYER_DEPTH_INCREMENT * 2, _mapInfo.lights, this.LightPrefab, _mapInfo.next_light_id));

        // Setup Tile Layers
        this.Layers.Add(PLATFORMS_LAYER, new MapEditorTilesLayer(platformsLayerData, PLATFORMS_LAYER_DEPTH, _tilesets, this.PlatformsRenderer));
        this.Layers.Add(BACKGROUND_LAYER, new MapEditorTilesLayer(backgroundLayerData, PLATFORMS_LAYER_DEPTH + LAYER_DEPTH_INCREMENT * 2, _tilesets, this.BackgroundRenderer));

        this.PlatformsRenderer.GetComponent<MeshRenderer>().sortingLayerName = PLATFORMS_LAYER;
        this.BackgroundRenderer.GetComponent<MeshRenderer>().sortingLayerName = BACKGROUND_LAYER;

        // Setup Parallax Layers
        this.ParallaxParent.SetPosition2D(_mapInfo.width * this.Grid.GridSpaceSize / 2, _mapInfo.height * this.Grid.GridSpaceSize / 2);
        if (_mapInfo.parallax_layers.Count == 0)
        {
            for (int i = 0; i < DEFAULT_FOREGROUND_PARALLAX_LAYERS; ++i)
                addParallaxLayer(true);

            for (int i = 0; i < DEFAULT_BACKGROUND_PARALLAX_LAYERS; ++i)
                addParallaxLayer(false);
        }
        else
        {
            for (int i = 0; i < _mapInfo.parallax_layers.Count; ++i)
                loadParallaxLayer(i);
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
            cycleLayers(MapEditorInput.CycleNextAlt);
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
        File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + this.MapName + StringExtensions.JSON_SUFFIX, JsonConvert.SerializeObject(_mapInfo, Formatting.Indented));
    }

    public void HandleReturnFromMenu()
    {
        // Update visual data for current layer
        MapEditorLayer currentLayer = this.Layers[this.CurrentLayer];
        switch (currentLayer.Type)
        {
            default:
            case MapEditorLayer.LayerType.Objects:
                break;
            case MapEditorLayer.LayerType.Tiles:
                ((MapEditorTilesLayer)currentLayer).PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                break;
            case MapEditorLayer.LayerType.Parallax:
                MapEditorParallaxLayer parallaxLayer = currentLayer as MapEditorParallaxLayer;
                _parallaxVisuals[this.CurrentLayer].gameObject.layer = LayerMask.NameToLayer(parallaxLayer.LayerName);
                _parallaxVisuals[this.CurrentLayer].CreateMeshForLayer(findParallaxSprite(parallaxLayer.SpriteName), parallaxLayer.Loops, parallaxLayer.Height, parallaxLayer.XPosition, parallaxLayer.ParallaxRatio, _mapInfo.width * this.Grid.GridSpaceSize, parallaxLayer.Lit ? this.ParallaxLitShader : this.ParallaxUnlitShader);
                break;
            case MapEditorLayer.LayerType.Lighting:
                removeObjectBrush();
                (currentLayer as MapEditorLightingLayer).ApplyBrush(this.ObjectCursor);
                break;
        }
    }

    /**
     * Private
     */
    private NewMapInfo _mapInfo;
    private Dictionary<string, TilesetData> _tilesets;
    private IntegerVector _previousCursorPos;
    private List<string> _sortedLayers;
    private Dictionary<string, ParallaxQuadGroup> _parallaxVisuals;
    private bool _tileEraserEnabled;
    private bool _objectEraserEnabled;
    private bool _exiting;
    private int _objectPrecisionIncrement;
    private int _foregroundParallaxCount;
    private int _backgroundParallaxCount;
    private List<Sprite> _parallaxSprites;

    private Sprite findParallaxSprite(string spriteName)
    {
        if (spriteName == null)
            return null;
        
        if (spriteName != null)
        {
            for (int i = 0; i < _parallaxSprites.Count; ++i)
            {
                if (_parallaxSprites[i].name == spriteName)
                    return _parallaxSprites[i];
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
        else if (currentLayer.Type == MapEditorLayer.LayerType.Parallax)
            updateParallaxLayer(currentLayer as MapEditorParallaxLayer);
        else if (currentLayer.Type == MapEditorLayer.LayerType.Lighting)
            updateLightingLayer(currentLayer as MapEditorLightingLayer);
    }

    private void enterLayer(string layerName)
    {
        MapEditorLayer layer = this.Layers[layerName];
        if (layer.Type == MapEditorLayer.LayerType.Tiles)
            enterTileLayer(layer as MapEditorTilesLayer);
        else if (layer.Type == MapEditorLayer.LayerType.Objects)
            enterObjectsLayer(layer as MapEditorObjectsLayer);
        else if (layer.Type == MapEditorLayer.LayerType.Lighting)
            enterLightingLayer(layer as MapEditorLightingLayer);
    }

    private void leaveLayer(string layerName)
    {
        if (!this.Layers.ContainsKey(layerName))
            return;

        MapEditorLayer layer = this.Layers[layerName];
        if (layer.Type == MapEditorLayer.LayerType.Tiles)
            leaveTileLayer(layer as MapEditorTilesLayer);
        else if (layer.Type == MapEditorLayer.LayerType.Objects)
            leaveObjectsLayer(layer as MapEditorObjectsLayer);
        else if (layer.Type == MapEditorLayer.LayerType.Lighting)
            leaveLightingLayer(layer as MapEditorLightingLayer);
    }

    private void updateTileLayer(MapEditorTilesLayer layer)
    {
        bool newPos = false;
        if (_previousCursorPos != this.Cursor.GridPos)
        {
            newPos = true;
            if (layer.CurrentBrushState != MapEditorTilesLayer.BrushState.GroupSet)
            {
                layer.ApplyData(_previousCursorPos.X, _previousCursorPos.Y);
                layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
            }
            _previousCursorPos = this.Cursor.GridPos;
        }

        if (layer.CurrentBrushState != MapEditorTilesLayer.BrushState.GroupSet)
        {
            if (layer.CurrentBrushState == MapEditorTilesLayer.BrushState.GroupPaint && MapEditorInput.CyclePrev)
            {
                layer.ApplyData(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                layer.CurrentBrushState = MapEditorTilesLayer.BrushState.SinglePaint;
                layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                this.SelectionBorder.gameObject.SetActive(false);
            }
            else if (MapEditorInput.Confirm || (newPos && MapEditorInput.ConfirmHeld))
            {
                layer.ApplyBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
            }
            else if (MapEditorInput.Cancel && layer.CurrentBrushState == MapEditorTilesLayer.BrushState.SinglePaint)
            {
                _tileEraserEnabled = !_tileEraserEnabled;
                this.Cursor.EnableEraser(_tileEraserEnabled);
                layer.EraserEnabled = _tileEraserEnabled;
                layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
            }
            else if (MapEditorInput.CycleNext)
            {
                layer.ApplyData(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                layer.BeginGroupSet(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                _tileEraserEnabled = false;
                this.Cursor.EnableEraser(_tileEraserEnabled);
                layer.EraserEnabled = _tileEraserEnabled;
                this.SelectionBorder.SetMinMax(layer.GroupBrushLowerLeft, layer.GroupBrushUpperRight);
                this.SelectionBorder.gameObject.SetActive(true);
            }
            else if (newPos && layer.CurrentBrushState == MapEditorTilesLayer.BrushState.GroupPaint)
            {
                this.SelectionBorder.SetMinMax(this.Cursor.GridPos, this.Cursor.GridPos + new IntegerVector(layer.GroupBrush.GetLength(0) - 1, layer.GroupBrush.GetLength(1) - 1));
            }
        }
        else
        {
            if (MapEditorInput.CyclePrev)
            {
                layer.CurrentBrushState = MapEditorTilesLayer.BrushState.SinglePaint;
                layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                this.SelectionBorder.gameObject.SetActive(false);
            }
            else if (newPos)
            {
                layer.UpdateGroupSet(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                this.SelectionBorder.SetMinMax(layer.GroupBrushLowerLeft, layer.GroupBrushUpperRight);
            }
            else if (MapEditorInput.CycleNext)
            {
                this.Cursor.GridPos = layer.EndGroupSet(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                this.Cursor.MoveToGridPos();
                layer.PreviewBrush(this.Cursor.GridPos.X, this.Cursor.GridPos.Y);
                this.SelectionBorder.SetMinMax(this.Cursor.GridPos, this.Cursor.GridPos + new IntegerVector(layer.GroupBrush.GetLength(0) - 1, layer.GroupBrush.GetLength(1) - 1));
            }
        }
    }

    private void updateObjectsLayer(MapEditorObjectsLayer layer)
    {
        GameObject toErase = null;

        if (layer.EraserEnabled)
        {
            this.ObjectEraseLine.enabled = true;
            toErase = findEraseTarget(layer);
            if (toErase == null)
                this.ObjectEraseLine.SetPositions(new Vector3[0]);
            else
                this.ObjectEraseLine.SetPositions(new Vector3[]{ this.ObjectCursor.transform.position, toErase.transform.position });
        }
        else
        {
            this.ObjectEraseLine.enabled = false;
        }

        if (MapEditorInput.Confirm)
        {
            if (!layer.EraserEnabled)
                addObject(layer);
            else
                eraseObject(layer, toErase);
        }
        else if (MapEditorInput.Cancel)
        {
            _objectEraserEnabled = !_objectEraserEnabled;
            layer.EraserEnabled = _objectEraserEnabled;

            this.ObjectCursor.gameObject.SetActive(!_objectEraserEnabled);
        }
        else
        {
            updateObjectMovement();

            if (MapEditorInput.CyclePrev)
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
    }

    private void updateLightingLayer(MapEditorLightingLayer layer)
    {
        GameObject toErase = null;

        if (layer.EraserEnabled)
        {
            this.ObjectEraseLine.enabled = true;
            toErase = findEraseTarget(layer);
            if (toErase == null)
                this.ObjectEraseLine.SetPositions(new Vector3[0]);
            else
                this.ObjectEraseLine.SetPositions(new Vector3[] { this.ObjectCursor.transform.position, toErase.transform.position });
        }
        else
        {
            this.ObjectEraseLine.enabled = false;
        }

        if (MapEditorInput.Confirm)
        {
            if (!layer.EraserEnabled)
                addLight(layer);
            else
                eraseObject(layer, toErase);
        }
        else if (MapEditorInput.Cancel)
        {
            _objectEraserEnabled = !_objectEraserEnabled;
            layer.EraserEnabled = _objectEraserEnabled;

            this.ObjectCursor.gameObject.SetActive(!_objectEraserEnabled);
        }
        else
        {
            updateObjectMovement();
        }
    }

    private void updateObjectMovement()
    {
        if (MapEditorInput.NavLeft)
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
    }

    private void updateParallaxLayer(MapEditorParallaxLayer layer)
    {
        if (MapEditorInput.Action)
        {
            addParallaxLayer(layer.Depth <= 0);
            _sortedLayers = this.DepthSortedLayers;
            this.LayerListPanel.ConfigureForLayers(_sortedLayers, this.CurrentLayer);
        }
        if (MapEditorInput.Cancel)
        {
            removeParallaxLayer(layer);
            _sortedLayers = this.DepthSortedLayers;
            this.LayerListPanel.ConfigureForLayers(_sortedLayers, this.CurrentLayer);
        }
    }

    private void leaveTileLayer(MapEditorTilesLayer layer)
    {
        layer.EraserEnabled = false;
        layer.ApplyData(_previousCursorPos.X, _previousCursorPos.Y);
        layer.CurrentBrushState = MapEditorTilesLayer.BrushState.SinglePaint;
        this.SelectionBorder.gameObject.SetActive(false);
    }

    private void leaveObjectsLayer(MapEditorObjectsLayer layer)
    {
        this.ObjectEraseLine.enabled = false;
        layer.EraserEnabled = false;
        removeObjectBrush();
        this.ObjectCursor.gameObject.SetActive(true);
    }

    private void leaveLightingLayer(MapEditorLightingLayer layer)
    {
        this.ObjectEraseLine.enabled = false;
        layer.EraserEnabled = false;
        removeObjectBrush();
        this.ObjectCursor.gameObject.SetActive(true);
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
        layer.EraserEnabled = _objectEraserEnabled;
        addObjectBrush(layer);
        this.ObjectCursor.gameObject.SetActive(!_objectEraserEnabled);
    }

    private void enterLightingLayer(MapEditorLightingLayer layer)
    {
        this.CameraController.SetTracker(this.ObjectCursor);
        this.ObjectCursor.SetZ(layer.Depth);
        layer.EraserEnabled = _objectEraserEnabled;
        layer.ApplyBrush(this.ObjectCursor);
        this.ObjectCursor.gameObject.SetActive(!_objectEraserEnabled);
    }

    private void removeObjectBrush()
    {
        Transform child = this.ObjectCursor.GetChild(0);
        this.ObjectCursor.DetachChildren();
        child.GetComponent<PooledObject>().Release();
    }

    private void addObjectBrush(MapEditorObjectsLayer layer)
    {
        Object prefab = layer.CurrentPrefab;
        PooledObject brushPooledObject = loadPooledObject(prefab);
        GameObject child = brushPooledObject.gameObject;
        child.transform.parent = this.ObjectCursor;
        child.transform.SetLocalPosition(0, 0, 0);

        Renderer r = child.GetComponent<Renderer>();
        if (r != null)
            r.sortingLayerName = layer.Name;
    }

    private void addObject(MapEditorObjectsLayer layer)
    {
        Object prefab = layer.CurrentPrefab;
        PooledObject brushPooledObject = loadPooledObject(prefab);
        GameObject newObject = brushPooledObject.gameObject;
        newObject.transform.SetPosition(this.ObjectCursor.position.x, this.ObjectCursor.position.y, layer.Depth);
        layer.AddObject(newObject);
    }

    private void addLight(MapEditorLightingLayer layer)
    {
        GameObject newLight = this.LightPrefab.Retain().gameObject;
        newLight.transform.SetPosition(this.ObjectCursor.position.x, this.ObjectCursor.position.y, layer.Depth);
        layer.AddObject(newLight);

        foreach (MonoBehaviour c in newLight.GetComponents<MonoBehaviour>())
        {
            if (!(c is PooledObject))
                c.enabled = false;
        }
    }

    private GameObject findEraseTarget(MapEditorLayer layer)
    {
        float closest = 10000.0f;
        GameObject find = null;

        List<GameObject> loadedObjects = layer.Type == MapEditorLayer.LayerType.Objects ? (layer as MapEditorObjectsLayer).LoadedObjects : (layer as MapEditorLightingLayer).LoadedLights;

        for (int i = 0; i < loadedObjects.Count; ++i)
        {
            GameObject go = loadedObjects[i];
            float dist = Vector2.Distance(this.ObjectCursor.transform.position, go.transform.position);
            if (dist < closest)
            {
                closest = dist;
                find = go;
            }
        }

        return find;
    }

    private void eraseObject(MapEditorLayer layer, GameObject toErase)
    {
        if (toErase != null)
        {
            if (layer.Type == MapEditorLayer.LayerType.Objects)
                (layer as MapEditorObjectsLayer).RemoveObject(toErase);
            else if (layer.Type == MapEditorLayer.LayerType.Lighting)
                (layer as MapEditorLightingLayer).RemoveObject(toErase);
            ObjectPools.Release(toErase);
        }
    }

    private void loadObjects(MapEditorObjectsLayer layer)
    {
        for (int i = 0; i < layer.Objects.Count; ++i)
        {
            Object prefab = layer.PrefabForName(layer.Objects[i].prefab_name);
            GameObject newObject = loadPooledObject(prefab).gameObject;
            newObject.name = layer.Objects[i].name;
            newObject.transform.SetPosition(layer.Objects[i].x, layer.Objects[i].y, layer.Depth);
            layer.LoadedObjects.Add(newObject);
            Renderer r = newObject.GetComponent<Renderer>();
            if (r != null)
            {
                r.sortingLayerName = layer.Name;
                r.sortingOrder = i;
            }
        }
    }

    private void loadLights(MapEditorLightingLayer layer)
    {
        for (int i = 0; i < layer.Lights.Count; ++i)
        {
            GameObject newObject = this.LightPrefab.Retain().gameObject;
            newObject.name = layer.Lights[i].name;
            newObject.transform.SetPosition(layer.Lights[i].x, layer.Lights[i].y, layer.Depth);
            newObject.GetComponent<SCLight>().ConfigureLight(layer.Lights[i]);
            layer.LoadedLights.Add(newObject);

            foreach (MonoBehaviour c in newObject.GetComponents<MonoBehaviour>())
            {
                if (!(c is PooledObject))
                    c.enabled = false;
            }
        }
    }

    private PooledObject loadPooledObject(Object prefab)
    {
        PooledObject newPooledObject;
        if (prefab is Sprite)
        {
            newPooledObject = this.SpriteObjectPrefab.Retain();
            newPooledObject.GetComponent<SpriteRenderer>().sprite = prefab as Sprite;
        }
        else
        {
            newPooledObject = prefab is GameObject ? (prefab as GameObject).GetComponent<PooledObject>().Retain() : (prefab as PooledObject).Retain();

            foreach (MonoBehaviour c in newPooledObject.GetComponents<MonoBehaviour>())
            {
                if (!(c is PooledObject))
                    c.enabled = false;
            }
        }
        return newPooledObject;
    }

    private void updateVisuals()
    {
        MapEditorTilesLayer platformsLayer = this.Layers[PLATFORMS_LAYER] as MapEditorTilesLayer;
        this.PlatformsRenderer.SetAtlas(_tilesets[platformsLayer.Tileset.name].AtlasName);
        this.PlatformsRenderer.CreateMapWithGrid(platformsLayer.Data);
        this.PlatformsRenderer.transform.SetZ(platformsLayer.Depth);

        MapEditorTilesLayer backgroundLayer = this.Layers[BACKGROUND_LAYER] as MapEditorTilesLayer;
        this.BackgroundRenderer.SetAtlas(_tilesets[backgroundLayer.Tileset.name].AtlasName);
        this.BackgroundRenderer.CreateMapWithGrid(backgroundLayer.Data);
        this.BackgroundRenderer.transform.SetZ(backgroundLayer.Depth);

        MapEditorObjectsLayer objectsLayer = this.Layers[OBJECTS_LAYER] as MapEditorObjectsLayer;
        loadObjects(objectsLayer);

        MapEditorObjectsLayer propsLayer = this.Layers[PROPS_LAYER] as MapEditorObjectsLayer;
        loadObjects(propsLayer);

        MapEditorObjectsLayer propsBackLayer = this.Layers[PROPS_BACK_LAYER] as MapEditorObjectsLayer;
        loadObjects(propsBackLayer);

        MapEditorLightingLayer lightingLayer = this.Layers[LIGHTING_LAYER] as MapEditorLightingLayer;
        loadLights(lightingLayer);

        for (int i = 0; i < _mapInfo.parallax_layers.Count; ++i)
        {
            string name = PARALLAX_PREFIX + _mapInfo.parallax_layers[i].depth;
            MapEditorParallaxLayer layer = this.Layers[name] as MapEditorParallaxLayer;
            int layerIndex = LayerMask.NameToLayer(layer.LayerName);
            _parallaxVisuals[name].gameObject.layer = layerIndex;
            _parallaxVisuals[name].CreateMeshForLayer(findParallaxSprite(layer.SpriteName), layer.Loops, layer.Height, layer.XPosition, layer.ParallaxRatio, _mapInfo.width * this.Grid.GridSpaceSize, layer.Lit ? this.ParallaxLitShader : this.ParallaxUnlitShader);
        }
    }

    private int depthCompareLayers(string l1, string l2)
    {
        return Mathf.Clamp(this.Layers[l1].Depth - this.Layers[l2].Depth, -1, 1);
    }

    private void compileTilesets()
    {
        _tilesets = new Dictionary<string, TilesetData>();
        for (int i = 0; i < this.TilesetCollection.Tilesets.Length; ++i)
        {
            _tilesets.Add(this.TilesetCollection.Tilesets[i].name, this.TilesetCollection.Tilesets[i]);
        }
    }

    private void cycleLayers(bool next)
    {
        int currentLayerIndex = this.CurrentLayerIndex;
        currentLayerIndex = next ? currentLayerIndex + 1 : currentLayerIndex - 1;
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

    private void addParallaxLayer(bool foreground)
    {
        int depth = foreground ? PLATFORMS_LAYER_DEPTH - (3 + _foregroundParallaxCount) * LAYER_DEPTH_INCREMENT : PLATFORMS_LAYER_DEPTH + (4 + _backgroundParallaxCount) * LAYER_DEPTH_INCREMENT;
        _mapInfo.AddParallaxLayer(depth);
        string name = PARALLAX_PREFIX + depth;
        MapEditorParallaxLayer layer = new MapEditorParallaxLayer(_mapInfo.GetParallaxLayer(depth), name);
        this.Layers.Add(name, layer);
        ParallaxQuadGroup quad = Instantiate<ParallaxQuadGroup>(this.ParallaxVisualPrefab);
        quad.CameraController = this.CameraController;
        quad.transform.parent = this.ParallaxParent;
        quad.transform.SetPosition(this.ParallaxParent.position.x, this.ParallaxParent.position.y, depth);
        _parallaxVisuals.Add(name, quad);

        if (layer.Depth <= PLATFORMS_LAYER_DEPTH)
        {
            quad.MeshRenderer.sortingLayerName = PARALLAX_FRONT_SORT_LAYER;
            ++_foregroundParallaxCount;
        }
        else
        {
            quad.MeshRenderer.sortingLayerName = PARALLAX_BACK_SORT_LAYER;
            ++_backgroundParallaxCount;
        }
    }

    private void loadParallaxLayer(int index)
    {
        string name = PARALLAX_PREFIX + _mapInfo.parallax_layers[index].depth;
        MapEditorParallaxLayer layer = new MapEditorParallaxLayer(_mapInfo.parallax_layers[index], name);
        this.Layers.Add(name, layer);
        ParallaxQuadGroup quad = Instantiate<ParallaxQuadGroup>(this.ParallaxVisualPrefab);
        quad.CameraController = this.CameraController;
        quad.transform.SetPosition(this.ParallaxParent.position.x, this.ParallaxParent.position.y, _mapInfo.parallax_layers[index].depth);
        quad.transform.parent = this.ParallaxParent;
        _parallaxVisuals.Add(name, quad);

        if (layer.Depth <= PLATFORMS_LAYER_DEPTH)
        {
            quad.MeshRenderer.sortingLayerName = PARALLAX_FRONT_SORT_LAYER;
            ++_foregroundParallaxCount;
        }
        else
        {
            quad.MeshRenderer.sortingLayerName = PARALLAX_BACK_SORT_LAYER;
            ++_backgroundParallaxCount;
        }
    }

    private void removeParallaxLayer(MapEditorParallaxLayer layer)
    {
        if ((layer.Depth <= 0 && _foregroundParallaxCount <= 1) || (layer.Depth > 0 && _backgroundParallaxCount <= 1))
            return;

        ParallaxQuadGroup quad = _parallaxVisuals[layer.Name];
        _parallaxVisuals.Remove(layer.Name);
        Destroy(quad.gameObject);
        _mapInfo.RemoveParallaxLayer(layer.Depth);
        this.Layers.Remove(layer.Name);

        cycleLayers(layer.Depth <= 0);
    }

    private const int DEFAULT_LEVEL_SIZE = 32;
    private const int LAYER_DEPTH_INCREMENT = 2;
    private const int OBJECT_PRECISION_PER_TILE = 8;
    private const string PARALLAX_PREFIX = "parallax_";
    private const string DEFAULT_PLATFORMS_TILESET = "GenericPlatforms";
    private const string DEFAULT_BACKGROUND_TILESET = "GenericBackground";
    private const int DEFAULT_FOREGROUND_PARALLAX_LAYERS = 1;
    private const int DEFAULT_BACKGROUND_PARALLAX_LAYERS = 1;
}
