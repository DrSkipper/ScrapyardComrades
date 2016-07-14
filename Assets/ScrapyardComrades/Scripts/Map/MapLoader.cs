using UnityEngine;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;
    public MapGeometryCreator GeometryCreator;
    public ObjectPlacer ObjectPlacer;
    public string PlatformsLayer = "platforms";
    public string ObjectsLayer = "objects";

    void Start()
    {
        TextAsset asset = Resources.Load<TextAsset>(PATH + this.MapName);
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(asset.text);
        int[,] grid = mapInfo.GetLayerWithName(this.PlatformsLayer).Grid;
        this.PlatformsRenderer.CreateMapWithGrid(grid);
        this.GeometryCreator.CreateGeometryForGrid(grid);
        this.ObjectPlacer.PlaceObjects(mapInfo.GetLayerWithName(this.ObjectsLayer).objects, mapInfo.width, mapInfo.height);
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
}
