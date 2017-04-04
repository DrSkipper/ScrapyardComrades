using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public const string LEVELS_PATH = "/Levels/";
    public const string JSON_SUFFIX = ".json";

    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;
    public TileRenderer BGRenderer;
    public MapGeometryCreator GeometryCreator;
    public ObjectPlacer ObjectPlacer;
    public WorldLoadingManager WorldLoadingManager;

    public bool Cleared { get { return _cleared; } }

    public void LoadMap(TilesetData platformsTileset, TilesetData bgTileset, Texture2D platformsAtlas, Texture2D bgAtlas, Dictionary<string, PooledObject> objectPrefabs, Dictionary<string, PooledObject> propPrefabs)
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
        
        this.ObjectPlacer.PlaceObjects(mapInfo.objects, objectPrefabs, this.MapName, true);
        this.ObjectPlacer.PlaceObjects(mapInfo.props, propPrefabs, this.MapName, false);
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

    public static Dictionary<string, Texture2D> CompileTextures(Texture2D[] textures)
    {
        Dictionary<string, Texture2D> textDict = new Dictionary<string, Texture2D>();
        for (int i = 0; i < textures.Length; ++i)
        {
            if (!textDict.ContainsKey(textures[i].name))
                textDict.Add(textures[i].name, textures[i]);
        }
        return textDict;
    }

    public static Dictionary<string, Sprite[]> CompileSprites(Dictionary<string, Texture2D> textures)
    {
        Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();
        foreach (string key in textures.Keys)
        {
            sprites.Add(key, textures[key].GetSpritesArray(TilesetEditorManager.TILESETS_PATH));
        }
        return sprites;
    }

    public static NewMapInfo GatherMapInfo(string mapName)
    {
        //TextAsset asset = Resources.Load<TextAsset>(PATH + mapName);
        //MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(asset.text);
        
        string path = Application.streamingAssetsPath + LEVELS_PATH + mapName + JSON_SUFFIX;
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
