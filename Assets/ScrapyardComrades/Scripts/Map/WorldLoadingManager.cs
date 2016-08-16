using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WorldLoadingManager : MonoBehaviour
{
    public string WorldMapName = "WorldMap";
    public string StartingAreaName = "Quad_0_0";
    public int MinTilesToTravelBetweenLoads = 8;
    public int TileRenderSize = 10;
    public int BoundsToLoadBuffer = 32;
    public int MinDistanceToLoad = 6;
    public bool FlipVertical = true;
    public GameObject MapLoaderPrefab;
    public List<GameObject> IgnoreRecenterObjects;
    public List<MapLoader> MapLoaders;
    public CollisionManager CollisionManager;

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
        if (this.MapLoaders == null)
            this.MapLoaders = new List<MapLoader>();

        gatherWorldMapInfo();
        for (int i = 0; i < _allMapQuads.Count; ++i)
        {
            if (_allMapQuads[i].Name == this.StartingAreaName)
            {
                _currentQuad = _allMapQuads[i];
                break;
            }
        }

        if (_currentQuad == null)
            Debug.LogWarning("Starting quad '" + this.StartingAreaName + "' not found. Quad count = " + _allMapQuads.Count);

        _targetLoadedQuads = new List<MapQuad>();
        _currentLoadedQuads = new List<MapQuad>();
        gatherTargetLoadedQuads();
        loadQuads(true);
        _currentLoadedQuads.AddRange(_targetLoadedQuads);

        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }

    void Update()
    {
        if (_tracker != null)
        {
            IntegerVector playerPosition = (Vector2)(_tracker.transform.position / this.TileRenderSize);

            if (!_currentQuad.CenteredBounds.Contains(playerPosition) &&
                (!_positionOfLastLoading.HasValue || Vector2.Distance(_tracker.transform.position, _positionOfLastLoading.Value) > this.MinDistanceToLoad * this.TileRenderSize))
            {
                IntegerVector offset = IntegerVector.Zero;

                // Change current quad
                for (int i = 0; i < _targetLoadedQuads.Count; ++i)
                {
                    IntegerRect otherRect = _targetLoadedQuads[i].GetRelativeBounds(_currentQuad);
                    if (otherRect.Contains(playerPosition))
                    {
                        _currentQuad = _targetLoadedQuads[i];
                        offset = otherRect.Center;
                        break;
                    }
                }

                // Get target quads to have loaded
                gatherTargetLoadedQuads();

                // Unload out of bounds quads
                for (int i = 0; i < _currentLoadedQuads.Count; ++i)
                {
                    if (!_targetLoadedQuads.Contains(_currentLoadedQuads[i]))
                    {
                        MapLoader loader = mapLoaderForQuadName(_currentLoadedQuads[i].Name);
                        if (loader != null)
                        {
                            loader.ClearMap();
                            loader.gameObject.SetActive(false);
                        }
                    }
                }

                // Recenter all objects except those specified to be ignored
                GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                Vector2 multipliedOffset = new Vector2(-offset.X * this.TileRenderSize, -offset.Y * this.TileRenderSize);
                for (int i = 0; i < rootObjects.Length; ++i)
                {
                    if (!this.IgnoreRecenterObjects.Contains(rootObjects[i]))
                    {
                        rootObjects[i].transform.position = rootObjects[i].transform.position + new Vector3(multipliedOffset.x, multipliedOffset.y, 0);
                    }
                }

                // Send recenter event so lerpers/tweens know to change targets
                GlobalEvents.Notifier.SendEvent(new WorldRecenterEvent(offset * -this.TileRenderSize));
                this.CollisionManager.ReorganizeSolids();

                // Load newly within range quads
                loadQuads(false);

                _currentLoadedQuads.Clear();
                _currentLoadedQuads.AddRange(_targetLoadedQuads);
                _positionOfLastLoading = _tracker.transform.position;
            }
        }
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
    private const string LAYER = "map";
    private const int BOUNDS_TO_LOAD = 32;
    private MapQuad _currentQuad;
    private List<MapQuad> _currentLoadedQuads;
    private List<MapQuad> _targetLoadedQuads;
    private Vector2? _positionOfLastLoading = null;
    private List<MapQuad> _allMapQuads;
    private Transform _tracker;

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
        for (int i = 0; i < this.MapLoaders.Count; ++i)
        {
            if (this.MapLoaders[i].gameObject.activeInHierarchy && this.MapLoaders[i].MapName == quadName)
                return this.MapLoaders[i];
        }
        return null;
    }

    private MapLoader findUnusedMapLoader(Vector2 position)
    {
        for (int i = 0; i < this.MapLoaders.Count; ++i)
        {
            if (!this.MapLoaders[i].gameObject.activeInHierarchy)
            {
                this.MapLoaders[i].gameObject.SetActive(true);
                this.MapLoaders[i].transform.position = new Vector3(position.x, position.y, this.MapLoaders[i].transform.position.z);
                return this.MapLoaders[i];
            }
        }

        GameObject newMapLoaderObject = Instantiate(this.MapLoaderPrefab, position, Quaternion.identity) as GameObject;
        MapLoader newMapLoader = newMapLoaderObject.GetComponent<MapLoader>();
        this.MapLoaders.Add(newMapLoader);
        return newMapLoader;
    }

    private void loadQuads(bool loadPlayer)
    {
        for (int i = 0; i < _targetLoadedQuads.Count; ++i)
        {
            if (!_currentLoadedQuads.Contains(_targetLoadedQuads[i]))
            {
                IntegerVector relativeCenter = _targetLoadedQuads[i].GetRelativeBounds(_currentQuad).Center;
                MapLoader loader = findUnusedMapLoader(new Vector2(relativeCenter.X * this.TileRenderSize, relativeCenter.Y * this.TileRenderSize));
                loader.MapName = _targetLoadedQuads[i].Name;
                loader.LoadPlayer = loadPlayer && _targetLoadedQuads[i] == _currentQuad;
                loader.LoadMap(true); // TODO - figure out how to handle when to load objects/remember where they were/discard them
            }
        }
    }
}
