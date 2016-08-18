using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public string MapName = "GameplayTest";
    public TileRenderer PlatformsRenderer;
    public MapGeometryCreator GeometryCreator;
    public ObjectPlacer ObjectPlacer;
    public string PlatformsLayer = "platforms";
    public string ObjectsLayer = "objects";
    public bool FlipVertical = true;
    public bool LoadPlayer = false;
    public bool LoadOnStart = false;

    public bool Cleared { get { return _cleared; } }

    void Start()
    {
        if (this.LoadOnStart)
            this.LoadMap();
    }

    public void LoadMap(bool loadObjects = false)
    {
        this.ClearMap();
        _cleared = false;
        MapInfo mapInfo = gatherMapInfo();
        int[,] grid = mapInfo.GetLayerWithName(this.PlatformsLayer).Grid;
        _width = mapInfo.width;
        _height = mapInfo.height;
        this.transform.position = this.transform.position + new Vector3(-_width * this.PlatformsRenderer.TileRenderSize / 2, -_height * this.PlatformsRenderer.TileRenderSize / 2, 0);
        this.PlatformsRenderer.CreateMapWithGrid(grid);
        this.GeometryCreator.CreateGeometryForGrid(grid);

        if (loadObjects)
            this.ObjectPlacer.PlaceObjects(mapInfo.GetLayerWithName(this.ObjectsLayer).objects, mapInfo.width, mapInfo.height, this.LoadPlayer);
    }

    public void CorrectTiling(bool editor = false)
    {
#if UNITY_EDITOR
        this.ClearMap(editor);
        MapInfo mapInfo = gatherMapInfo();
        MapInfo.MapLayer platformsLayer = mapInfo.GetLayerWithName(this.PlatformsLayer);
        int[,] grid = platformsLayer.Grid;

        grid = correctTiles(grid);
        this.PlatformsRenderer.CreateMapWithGrid(grid);
        this.GeometryCreator.CreateGeometryForGrid(grid);
        _cleared = false;

        platformsLayer.Grid = grid;
        mapInfo.SetLayerWithName(this.PlatformsLayer, platformsLayer);

        string json = JsonConvert.SerializeObject(mapInfo, Formatting.Indented);
        using (StreamWriter stream = new StreamWriter(Path.Combine(Application.streamingAssetsPath, "LevelCorrections/" + this.MapName + ".json"), false, Encoding.GetEncoding("UTF-8")))
        {
            stream.Write(value: json);
        }
#endif
    }

    public void ClearMap(bool editor = false)
    {
        if (!_cleared)
        {
            _cleared = true;
            this.PlatformsRenderer.Clear();
            this.GeometryCreator.Clear(editor);
            this.transform.position = this.transform.position + new Vector3(_width * this.PlatformsRenderer.TileRenderSize / 2, _height * this.PlatformsRenderer.TileRenderSize / 2, this.transform.position.z);
            _width = 0;
            _height = 0;
        }
    }

    /**
     * Private
     */
    private const string PATH = "Levels/";
    private bool _cleared = false;
    private int _width = 0;
    private int _height = 0;

    private MapInfo gatherMapInfo()
    {
        TextAsset asset = Resources.Load<TextAsset>(PATH + this.MapName);
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(asset.text);
        return mapInfo;
    }

    private const int TILE_LONE = 1;
    private const int TILE_ISLAND_LEFT = 2;
    private const int TILE_ISLAND_MID = 3;
    private const int TILE_ISLAND_RIGHT = 4;
    private const int TILE_COLUMN_TOP = 5;
    private const int TILE_TOP_LEFT = 6;
    private const int TILE_TOP_MID = 7;
    private const int TILE_TOP_RIGHT = 8;
    private const int TILE_COLUMN_MID = 9;
    private const int TILE_MID_LEFT = 10;
    private const int TILE_MID_MID = 11;
    private const int TILE_MID_RIGHT = 12;
    private const int TILE_COLUMN_BOTTOM = 13;
    private const int TILE_BOTTOM_LEFT = 14;
    private const int TILE_BOTTOM_MID = 15;
    private const int TILE_BOTTOM_RIGHT = 16;

    private int[,] correctTiles(int[,] grid)
    {
        int[,] newGrid = new int[grid.GetLength(0), grid.GetLength(1)];

        for (int x = 0; x < grid.GetLength(0); ++x)
        {
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                if (grid[x, y] == 0)
                {
                    // Empty
                    newGrid[x, y] = 0;
                }
                else
                {
                    bool left = isFilled(grid, x - 1, y);
                    bool right = isFilled(grid, x + 1, y);
                    bool down = isFilled(grid, x, y - 1);
                    bool up = isFilled(grid, x, y + 1);

                    if (!left && !right && !down && !up)
                    {
                        // Lone
                        newGrid[x, y] = TILE_LONE; 
                    }
                    else if (!left && !right)
                    {
                        // Column
                        if (up && down)
                            newGrid[x, y] = TILE_COLUMN_MID;
                        else if (!up)
                            newGrid[x, y] = this.FlipVertical ? TILE_COLUMN_BOTTOM : TILE_COLUMN_TOP;
                        else
                            newGrid[x, y] = this.FlipVertical ? TILE_COLUMN_TOP : TILE_COLUMN_BOTTOM;
                    }
                    else if (!down && !up)
                    {
                        // Island
                        if (left && right)
                            newGrid[x, y] = TILE_ISLAND_MID;
                        else if (!left)
                            newGrid[x, y] = TILE_ISLAND_LEFT;
                        else
                            newGrid[x, y] = TILE_ISLAND_RIGHT;
                    }
                    else
                    {
                        if (up && down && left && right)
                        {
                            newGrid[x, y] = TILE_MID_MID;
                        }
                        else if (up && down)
                        {
                            if (right)
                                newGrid[x, y] = TILE_MID_LEFT;
                            else
                                newGrid[x, y] = TILE_MID_RIGHT;
                        }
                        else if (left && right)
                        {
                            if (up)
                                newGrid[x, y] = this.FlipVertical ? TILE_TOP_MID : TILE_BOTTOM_MID;
                            else
                                newGrid[x, y] = this.FlipVertical ? TILE_BOTTOM_MID : TILE_TOP_MID;
                        }
                        else if (up && right)
                        {
                            newGrid[x, y] = this.FlipVertical ? TILE_TOP_LEFT : TILE_BOTTOM_LEFT;
                        }
                        else if (up && left)
                        {
                            newGrid[x, y] = this.FlipVertical ? TILE_TOP_RIGHT : TILE_BOTTOM_RIGHT;
                        }
                        else if (down && right)
                        {
                            newGrid[x, y] = this.FlipVertical ? TILE_BOTTOM_LEFT : TILE_TOP_LEFT;
                        }
                        else if (down && left)
                        {
                            newGrid[x, y] = this.FlipVertical ? TILE_BOTTOM_RIGHT : TILE_TOP_RIGHT;
                        }
                    }
                }
            }
        }

        return newGrid;
    }

    private bool isFilled(int[,] grid, int x, int y, bool countOutOfBounds = true)
    {
        if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
            return countOutOfBounds;

        return grid[x, y] != 0;
    }
}
