using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class WorldLoadingManager : MonoBehaviour, IPausable, CameraBoundsHandler
{
    public const int MAX_MAP_LOADERS = 12;
    public const string ROOM_TRANSITION_SEQUENCE = "room_transition";
    public const string WORLD_INFO_PATH = "/Levels/WorldMap/";

    public string WorldMapName = "World";
    public string StartingAreaName = "intro_0";
    public int WorldGridSpaceSize = 16;
    public int MinTilesToTravelBetweenLoads = 8;
    public int TileRenderSize = 10;
    public int MinDistanceToLoad = 6;
    public bool FlipVertical = true;
    public PooledObject MapLoaderPrefab;
    public List<GameObject> IgnoreRecenterObjects;
    public CollisionManager CollisionManager;
    public EntityTracker EntityTracker;
    public TilesetCollection TilesetCollection;
    public PrefabCollection ObjectPrefabs;
    public PrefabCollection PropPrefabs;
    public PooledObject LightPrefab;

    public IntegerRectCollider CurrentQuadBoundsCheck;
    public IntegerRectCollider GetBounds() { return this.CurrentQuadBoundsCheck; }
    public string CurrentQuadName { get { return _currentQuad.Name; } }
    public int CurrentQuadWidth { get { return _currentQuad.Bounds.Size.X * this.TileRenderSize; } }
    public NewMapInfo CurrentMapInfo { get { return _quadData[this.CurrentQuadName]; } }

    public class MapQuad
    {
        public string Name;
        public IntegerRect Bounds;
        public IntegerRect CenteredBounds;
        public IntegerRect BoundsToLoad;

        public IntegerRect GetRelativeBounds(MapQuad other)
        {
            IntegerRect offsetRect = this.CenteredBounds;
            offsetRect.Center.X = this.Bounds.Center.X - other.Bounds.Center.X;
            offsetRect.Center.Y = this.Bounds.Center.Y - other.Bounds.Center.Y;
            return offsetRect;
        }
    }

    void Awake()
    {
        if (ScenePersistentLoading.IsLoading)
        {
            ScenePersistentLoading.LoadInfo loadInfo = ScenePersistentLoading.ConsumeLoad().Value;
            this.StartingAreaName = loadInfo.LevelToLoad;
            //TODO: Handle IgnorePlayerSave bool here once player saving is implemented
        }

        _activeMapLoaders = new List<MapLoader>(MAX_MAP_LOADERS);
        _tilesets = new Dictionary<string, TilesetData>();

        for (int i = 0; i < this.TilesetCollection.Tilesets.Length; ++i)
        {
            _tilesets.Add(this.TilesetCollection.Tilesets[i].name, this.TilesetCollection.Tilesets[i]);
        }

        gatherWorldMapInfo();
        _quadData = new Dictionary<string, NewMapInfo>();

        for (int i = 0; i < _allMapQuads.Count; ++i)
        {
            if (_allMapQuads[i].Name == this.StartingAreaName)
                _currentQuad = _allMapQuads[i];

            _quadData.Add(_allMapQuads[i].Name, MapLoader.GatherMapInfo(_allMapQuads[i].Name));
        }

        if (_currentQuad == null)
            Debug.LogWarning("Starting quad '" + this.StartingAreaName + "' not found. Quad count = " + _allMapQuads.Count);

        _objectPrefabs = new Dictionary<string, PooledObject>();
        _propPrefabs = new Dictionary<string, PooledObject>();
        if (this.ObjectPrefabs.Prefabs != null)
        {
            for (int i = 0; i < this.ObjectPrefabs.Prefabs.Count; ++i)
            {
                _objectPrefabs[this.ObjectPrefabs.Prefabs[i].name] = this.ObjectPrefabs.Prefabs[i];
            }
        }
        if (this.PropPrefabs.Prefabs != null)
        {
            for (int i = 0; i < this.PropPrefabs.Prefabs.Count; ++i)
            {
                _propPrefabs[this.PropPrefabs.Prefabs[i].name] = this.PropPrefabs.Prefabs[i];
            }
        }

        this.CollisionManager.RemoveAllSolids();
        _targetLoadedQuads = new List<MapQuad>();
        _currentLoadedQuads = new List<MapQuad>();
        gatherTargetLoadedQuads();
        loadCurrentQuad();
        _currentLoadedQuads.Add(_currentQuad);
        loadQuads();
        _currentLoadedQuads.AddRange(_targetLoadedQuads);
        updateBoundsCheck();

        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }

    void Update()
    {
        if (_tracker != null)
        {
            _trackerPosition = (Vector2)(_tracker.transform.position / this.TileRenderSize);

            if (!_currentQuad.CenteredBounds.Contains(_trackerPosition) &&
                (!_positionOfLastLoading.HasValue || Vector2.Distance(_tracker.transform.position, _positionOfLastLoading.Value) > this.MinDistanceToLoad * this.TileRenderSize))
            {
                _recenterOffset = IntegerVector.Zero;
                changeCurrentQuad();
                gatherTargetLoadedQuads();
                unloadQuads();
                recenter();
                loadQuads();
                _currentLoadedQuads.Clear();
                _currentLoadedQuads.AddRange(_targetLoadedQuads);
                _positionOfLastLoading = _tracker.transform.position;
                PauseController.BeginSequence(ROOM_TRANSITION_SEQUENCE);
            }
        }
    }

    public NewMapInfo GetMapInfoForQuad(string quadName)
    {
        return _quadData[quadName];
    }

    public static WorldInfo ReadWorldMapInfo(string worldMapName)
    {
        string path = Application.streamingAssetsPath + WORLD_INFO_PATH + worldMapName + MapLoader.JSON_SUFFIX;
        if (File.Exists(path))
        {
            return JsonConvert.DeserializeObject<WorldInfo>(File.ReadAllText(path));
        }
        return null;
    }

    /**
     * Private
     */
    private const int BOUNDS_TO_LOAD = 8;
    private const int LOAD_PHASE_CHANGE_CURRENT = 0;
    private const int LOAD_PHASE_GATHER_TARGETS = 1;
    private const int LOAD_PHASE_UNLOAD_QUADS = 2;
    private const int LOAD_PHASE_RECENTER = 3;
    private const int LOAD_PHASE_LOAD_QUADS = 4;
    private const int LOAD_SPACING = 2;
    private List<MapLoader> _activeMapLoaders;
    private MapQuad _currentQuad;
    private List<MapQuad> _currentLoadedQuads;
    private List<MapQuad> _targetLoadedQuads;
    private Vector2? _positionOfLastLoading = null;
    private List<MapQuad> _allMapQuads;
    private Transform _tracker;
    private IntegerVector _recenterOffset = IntegerVector.Zero;
    private IntegerVector _trackerPosition = IntegerVector.Zero;
    private Dictionary<string, NewMapInfo> _quadData;
    private Dictionary<string, TilesetData> _tilesets;
    private Dictionary<string, PooledObject> _objectPrefabs;
    private Dictionary<string, PooledObject> _propPrefabs;

    private void gatherWorldMapInfo()
    {
        WorldInfo worldInfo = ReadWorldMapInfo(this.WorldMapName);
        _allMapQuads = new List<MapQuad>();
        
        for (int i = 0; i < worldInfo.level_quads.Length; ++i)
        {
            WorldInfo.LevelQuad levelQuad = worldInfo.level_quads[i];
            MapQuad quad = new MapQuad();
            quad.Name = levelQuad.name;
            IntegerVector pos = new IntegerVector(levelQuad.x * this.WorldGridSpaceSize, levelQuad.y * this.WorldGridSpaceSize);
            quad.Bounds = IntegerRect.CreateFromMinMax(pos, new IntegerVector(pos.X + levelQuad.width * this.WorldGridSpaceSize, pos.Y + levelQuad.height * this.WorldGridSpaceSize));
            quad.CenteredBounds = new IntegerRect(0, 0, quad.Bounds.Size.X, quad.Bounds.Size.Y);
            quad.BoundsToLoad = new IntegerRect(quad.Bounds.Center.X, quad.Bounds.Center.Y, quad.Bounds.Size.X + BOUNDS_TO_LOAD, quad.Bounds.Size.Y + BOUNDS_TO_LOAD);
            _allMapQuads.Add(quad);
        }
    }

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _tracker = (e as PlayerSpawnedEvent).PlayerObject.transform;
    }

    private void gatherTargetLoadedQuads()
    {
        _targetLoadedQuads.Clear();
        for (int i = 0; i < _allMapQuads.Count; ++i)
        {
            if (_currentQuad.BoundsToLoad.Overlaps(_allMapQuads[i].Bounds))
            {
                _targetLoadedQuads.Add(_allMapQuads[i]);
            }
        }
    }

    private MapLoader mapLoaderForQuadName(string quadName)
    {
        for (int i = 0; i < _activeMapLoaders.Count; ++i)
        {
            if (_activeMapLoaders[i].gameObject.activeInHierarchy && _activeMapLoaders[i].MapName == quadName)
                return _activeMapLoaders[i];
        }
        return null;
    }

    private MapLoader aquireMapLoader(Vector2 position)
    {
        PooledObject loader = this.MapLoaderPrefab.Retain();
        loader.transform.position = new Vector3(position.x, position.y, loader.transform.position.z);
        MapLoader newMapLoader = loader.GetComponent<MapLoader>();
        newMapLoader.WorldLoadingManager = this;
        newMapLoader.ObjectPlacer.EntityTracker = this.EntityTracker;
        _activeMapLoaders.Add(newMapLoader);
        return newMapLoader;
    }

    private void changeCurrentQuad()
    {
        // Change current quad
        for (int i = 0; i < _targetLoadedQuads.Count; ++i)
        {
            IntegerRect otherRect = _targetLoadedQuads[i].GetRelativeBounds(_currentQuad);
            if (otherRect.Contains(_trackerPosition))
            {
                _currentQuad = _targetLoadedQuads[i];
                _recenterOffset = otherRect.Center;
                break;
            }
        }
    }

    private void unloadQuads()
    {
        // Unload out of bounds quads
        for (int i = 0; i < _currentLoadedQuads.Count; ++i)
        {
            if (!_targetLoadedQuads.Contains(_currentLoadedQuads[i]))
            {
                MapLoader loader = mapLoaderForQuadName(_currentLoadedQuads[i].Name);
                if (loader != null)
                {
                    loader.ClearMap();
                    _activeMapLoaders.Remove(loader);
                    ObjectPools.Release(loader.gameObject);
                }
            }
        }

        this.CollisionManager.RemoveAllSolids();
    }

    private void recenter()
    {
        // Recenter all objects except those specified to be ignored
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        Vector2 multipliedOffset = new Vector2(-_recenterOffset.X * this.TileRenderSize, -_recenterOffset.Y * this.TileRenderSize);
        for (int i = 0; i < rootObjects.Length; ++i)
        {
            if (!this.IgnoreRecenterObjects.Contains(rootObjects[i]))
            {
                rootObjects[i].transform.position = rootObjects[i].transform.position + new Vector3(multipliedOffset.x, multipliedOffset.y, 0);
            }
        }

        updateBoundsCheck();
        this.EntityTracker.QuadsUnloaded(_targetLoadedQuads, _currentQuad, this.TileRenderSize);

        // Send recenter event so lerpers/tweens know to change targets
        GlobalEvents.Notifier.SendEvent(new WorldRecenterEvent(_recenterOffset * -this.TileRenderSize));
    }

    private void updateBoundsCheck()
    {
        this.CurrentQuadBoundsCheck.Size = new IntegerVector(_currentQuad.CenteredBounds.Size.X * this.TileRenderSize, _currentQuad.CenteredBounds.Size.Y * this.TileRenderSize);
    }

    private void loadQuads()
    {
        for (int i = 0; i < _targetLoadedQuads.Count; ++i)
        {
            if (!_currentLoadedQuads.Contains(_targetLoadedQuads[i]))
            {
                loadQuad(_targetLoadedQuads[i]);
            }
        }

        for (int i = 0; i < _activeMapLoaders.Count; ++i)
        {
            _activeMapLoaders[i].AddColliders();
        }
    }

    private void loadCurrentQuad()
    {
        loadQuad(_currentQuad);
    }

    private void loadQuad(MapQuad quad)
    {
        IntegerVector relativeCenter = quad.GetRelativeBounds(_currentQuad).Center;
        MapLoader loader = aquireMapLoader(new Vector2(relativeCenter.X * this.TileRenderSize, relativeCenter.Y * this.TileRenderSize));
        loader.MapName = quad.Name;
        NewMapInfo mapInfo = _quadData[quad.Name];
        string platformsTilesetName = mapInfo.GetMapLayer(MapEditorManager.PLATFORMS_LAYER).tileset_name;
        string backgroundTilesetName = mapInfo.GetMapLayer(MapEditorManager.BACKGROUND_LAYER).tileset_name;
        TilesetData platformsTileset = _tilesets[platformsTilesetName];
        TilesetData backgroundTileset = _tilesets[backgroundTilesetName];
        loader.LoadMap(platformsTileset, backgroundTileset, platformsTileset.AtlasName, backgroundTileset.AtlasName, _objectPrefabs, _propPrefabs, this.LightPrefab);
    }
}
