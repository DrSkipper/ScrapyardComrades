using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WorldLoadingManager : MonoBehaviour
{
    public string WorldMapName = "WorldMap";
    public string StartingAreaName = "Quad_0_0";
    public int MinTilesToTravelBetweenLoads = 8;
    public int TileRenderSize = 10;
    public int BoundsToLoadBuffer = 32;
    public GameObject MapLoaderPrefab;
    public List<GameObject> IgnoreRecenterObjects;
    public List<MapLoader> MapLoaders;

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

        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }

    void Update()
    {
        if (_tracker != null)
        {
            IntegerVector playerPosition = (Vector2)(_tracker.transform.position / this.TileRenderSize);
            if (!_currentQuad.CenteredBounds.Contains(playerPosition))
            {
                // Change current quad
                for (int i = 0; i < _targetLoadedQuads.Count; ++i)
                {
                    if (_targetLoadedQuads[i].GetRelativeBounds(_currentQuad).Contains(playerPosition))
                    {
                        _currentQuad = _targetLoadedQuads[i];
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
                        // unload
                    }
                }

                // Recenter all objects except those specified to be ignored
                GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                {
                    for (int i = 0; i < rootObjects.Length; ++i)
                    {
                        if (!this.IgnoreRecenterObjects.Contains(rootObjects[i]))
                        {
                            // recenter
                        }
                    }
                }

                // send recenter event so lerpers/tweens know to change targets

                // load newly within range quads
            }
        }
    }

    /**
     * Private
     */
    private MapQuad _currentQuad;
    private List<MapQuad> _currentLoadedQuads;
    private List<MapQuad> _targetLoadedQuads;
    private Vector2 _positionOfLastLoading;
    private List<MapQuad> _allMapQuads;
    private Transform _tracker;

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
}
