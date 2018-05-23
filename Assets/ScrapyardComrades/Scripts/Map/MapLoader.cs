using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public const string LEVELS_PATH = "/Levels/";

    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;
    public TileRenderer BGRenderer;
    public TileRenderer BGParallaxRenderer;
    public MapGeometryCreator GeometryCreator;
    public ObjectPlacer ObjectPlacer;
    public WorldLoadingManager WorldLoadingManager;

    public bool Cleared { get { return _cleared; } }

    void Awake()
    {
        this.PlatformsRenderer.GetComponent<MeshRenderer>().sortingLayerName = MapEditorManager.PLATFORMS_LAYER;
        this.BGRenderer.GetComponent<MeshRenderer>().sortingLayerName = MapEditorManager.BACKGROUND_LAYER;
    }

    public void LoadMap(TilesetData platformsTileset, TilesetData bgTileset, string platformsAtlas, string bgAtlas, Dictionary<string, PooledObject> objectPrefabs, Dictionary<string, PooledObject> propPrefabs, PooledObject lightPrefab)
    {
        this.ClearMap();
        _cleared = false;
        
        NewMapInfo mapInfo = this.WorldLoadingManager != null ? this.WorldLoadingManager.GetMapInfoForQuad(this.MapName) : GatherMapInfo(this.MapName);
        NewMapInfo.MapLayer platformsLayer = mapInfo.GetMapLayer(MapEditorManager.PLATFORMS_LAYER);
        NewMapInfo.MapTile[,] platformsGrid = platformsLayer.GetDataGrid();
        _width = mapInfo.width;
        _height = mapInfo.height;
        this.transform.position = this.transform.position + new Vector3(-_width * this.PlatformsRenderer.TileRenderSize / 2, -_height * this.PlatformsRenderer.TileRenderSize / 2, 0);
        this.PlatformsRenderer.SetAtlas(platformsAtlas);
        this.PlatformsRenderer.CreateMapWithGrid(platformsGrid);

        NewMapInfo.MapLayer bgLayer = mapInfo.GetMapLayer(MapEditorManager.BACKGROUND_LAYER);
        if (bgLayer != null)
        {
            this.BGRenderer.SetAtlas(bgAtlas);
            this.BGRenderer.CreateMapWithGrid(bgLayer.GetDataGrid());
        }
        this.GeometryCreator.CreateGeometryForGrid(platformsGrid, platformsTileset.GetSpriteDataDictionary(), false);
        
        this.ObjectPlacer.PlaceObjects(mapInfo.objects, objectPrefabs, this.MapName, true, MapEditorManager.OBJECTS_LAYER);
        this.ObjectPlacer.PlaceObjects(mapInfo.props, propPrefabs, this.MapName, false, MapEditorManager.PROPS_LAYER);
        if (mapInfo.props_background != null && mapInfo.props_background.Count > 0)
            this.ObjectPlacer.PlaceObjects(mapInfo.props_background, propPrefabs, this.MapName, false, MapEditorManager.PROPS_BACK_LAYER);
        this.ObjectPlacer.PlaceLights(mapInfo.lights, lightPrefab);
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
            this.GeometryCreator.Clear(editor);
            this.ObjectPlacer.WipeSpawns();
            this.transform.position = this.transform.position + new Vector3(_width * this.PlatformsRenderer.TileRenderSize / 2, _height * this.PlatformsRenderer.TileRenderSize / 2, this.transform.position.z);
            _width = 0;
            _height = 0;
        }
    }

    public void AddColliders()
    {
        this.GeometryCreator.AddColliders();
    }

    public void EnableVisual(bool enable)
    {
        this.PlatformsRenderer.renderer.enabled = enable;
        this.BGRenderer.renderer.enabled = enable;
        this.ObjectPlacer.EnableNonTrackedObjects(enable);
    }

    public static NewMapInfo GatherMapInfo(string mapName)
    {
        string path = Application.streamingAssetsPath + LEVELS_PATH + mapName + StringExtensions.JSON_SUFFIX;
        if (File.Exists(path))
        {
            return JsonConvert.DeserializeObject<NewMapInfo>(File.ReadAllText(path));
        }
        return null;
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
    private bool _cleared = false;
    private int _width = 0;
    private int _height = 0;
}
