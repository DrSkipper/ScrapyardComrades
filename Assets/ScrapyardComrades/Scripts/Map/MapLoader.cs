using UnityEngine;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;
    public string PlatformsLayer = "platforms";

    void Start()
    {
        TextAsset asset = Resources.Load<TextAsset>(PATH + this.MapName);
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(asset.text);
        this.PlatformsRenderer.CreateMapWithGrid(mapInfo.GetLayerWithName(this.PlatformsLayer).Grid);
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
}
