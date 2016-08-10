using UnityEngine;
using System.Collections.Generic;

public class WorldLoadingManager : MonoBehaviour
{
    public string WorldMapName = "WorldMap";
    public string StartingAreaName = "Quad_0_0";
    public int MinTilesToTravelBetweenLoads = 8;
    public int TileRenderSize = 10;
    public int BoundsToLoadBuffer = 32;
    public GameObject MapLoaderPrefab;

    public struct MapQuad
    {
        public string Name;
        public IntegerRect Bounds;
        public IntegerRect CenteredBounds;
        public IntegerRect BoundsToLoad;
    }

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }

    void Update()
    {
        if (_tracker != null)
        {
            IntegerVector playerPosition = (Vector2)(_tracker.transform.position / this.TileRenderSize);
            if (!_currentQuad.CenteredBounds.Contains(playerPosition))
            {

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
}
