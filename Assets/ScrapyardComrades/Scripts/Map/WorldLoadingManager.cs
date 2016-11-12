using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WorldLoadingManager : MonoBehaviour, IPausable
{
    public string WorldMapName = "WorldMap";
    public string StartingAreaName = "Quad_0_0";
    public int MinTilesToTravelBetweenLoads = 8;
    public int TileRenderSize = 10;
    public int BoundsToLoadBuffer = 32;
    public int MinDistanceToLoad = 6;
    public bool FlipVertical = true;
    public PooledObject MapLoaderPrefab;
    public List<GameObject> IgnoreRecenterObjects;
    public CollisionManager CollisionManager;
    public Dictionary<string, Texture2D> CachedAtlases;
    public Dictionary<string, Sprite[]> CachedSprites;
    public const int MAX_MAP_LOADERS = 12;

    public IntegerRectCollider CurrentQuadBoundsCheck;

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
        _activeMapLoaders = new List<MapLoader>(MAX_MAP_LOADERS);

        gatherWorldMapInfo();
        _quadData = new Dictionary<string, MapInfo>();

        for (int i = 0; i < _allMapQuads.Count; ++i)
        {
            if (_allMapQuads[i].Name == this.StartingAreaName)
                _currentQuad = _allMapQuads[i];

            _quadData.Add(_allMapQuads[i].Name, MapLoader.GatherMapInfo(_allMapQuads[i].Name));
        }

        if (_currentQuad == null)
            Debug.LogWarning("Starting quad '" + this.StartingAreaName + "' not found. Quad count = " + _allMapQuads.Count);

        this.CollisionManager.RemoveAllSolids();
        _targetLoadedQuads = new List<MapQuad>();
        _currentLoadedQuads = new List<MapQuad>();
        gatherTargetLoadedQuads();
        loadQuads(true);
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
                loadQuads(false);
                _currentLoadedQuads.Clear();
                _currentLoadedQuads.AddRange(_targetLoadedQuads);
                _positionOfLastLoading = _tracker.transform.position;
            }
        }
    }

    public MapInfo GetMapInfoForQuad(string quadName)
    {
        return _quadData[quadName];
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
    private const string LAYER = "map";
    private const int BOUNDS_TO_LOAD = 32;
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
    private Dictionary<string, MapInfo> _quadData;

    private void gatherWorldMapInfo()
    {
        TextAsset asset = Resources.Load<TextAsset>(PATH + this.WorldMapName);
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(asset.text);
        _allMapQuads = new List<MapQuad>();

        MapInfo.MapLayer layer = mapInfo.GetLayerWithName(LAYER);
        for (int i = 0; i < layer.objects.Length; ++i)
        {
            MapInfo.MapObject mapObject = layer.objects[i];
            MapQuad quad = new MapQuad();
            quad.Name = mapObject.name;
            int y = this.FlipVertical ? mapInfo.height - mapObject.y - mapObject.height / 2 : mapObject.y + mapObject.height / 2;
            quad.Bounds = new IntegerRect(mapObject.x + mapObject.width / 2, y, mapObject.width, mapObject.height);
            quad.CenteredBounds = new IntegerRect(0, 0, mapObject.width, mapObject.height);
            quad.BoundsToLoad = new IntegerRect(quad.Bounds.Center.X, quad.Bounds.Center.Y, mapObject.width + BOUNDS_TO_LOAD, mapObject.height + BOUNDS_TO_LOAD);
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

        // Send recenter event so lerpers/tweens know to change targets
        GlobalEvents.Notifier.SendEvent(new WorldRecenterEvent(_recenterOffset * -this.TileRenderSize));
    }

    private void updateBoundsCheck()
    {
        this.CurrentQuadBoundsCheck.Size = new IntegerVector(_currentQuad.CenteredBounds.Size.X * this.TileRenderSize, _currentQuad.CenteredBounds.Size.Y * this.TileRenderSize);
    }

    private void loadQuads(bool loadPlayer)
    {
        for (int i = 0; i < _targetLoadedQuads.Count; ++i)
        {
            if (!_currentLoadedQuads.Contains(_targetLoadedQuads[i]))
            {
                IntegerVector relativeCenter = _targetLoadedQuads[i].GetRelativeBounds(_currentQuad).Center;
                MapLoader loader = aquireMapLoader(new Vector2(relativeCenter.X * this.TileRenderSize, relativeCenter.Y * this.TileRenderSize));
                loader.MapName = _targetLoadedQuads[i].Name;
                loader.LoadPlayer = loadPlayer && _targetLoadedQuads[i] == _currentQuad;
                loader.LoadMap(true); //TODO - figure out how to handle when to load objects/remember where they were/discard them
            }
        }

        for (int i = 0; i < _activeMapLoaders.Count; ++i)
        {
            _activeMapLoaders[i].AddColliders();
        }
    }
}
