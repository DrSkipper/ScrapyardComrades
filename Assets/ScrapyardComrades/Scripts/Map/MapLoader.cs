using UnityEngine;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;

    void Start()
    {
        TextAsset asset = Resources.Load<TextAsset>(PATH + this.MapName);
        object data = JsonConvert.DeserializeObject(asset.text);
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
}
