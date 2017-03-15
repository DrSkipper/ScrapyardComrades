using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class WorldMapEditorManager : MonoBehaviour, CameraBoundsHandler
{
    public string WorldMapName;
    public int WorldGridSpaceSize = 16;
    public int GridSpaceRenderSize = 20;
    public RectTransform WorldPanel;
    public ContextMenu ContextMenu;
    public MapEditorCursor Cursor;
    public MapEditorGrid Grid;
    public IntegerRectCollider WorldBounds;
    public ActivateAndAnimateImage SaveIcon;
    public ActivateAndAnimateImage FadeOut;
    public NewLevelPanel NewLevelPanel;
    public TimedCallbacks TimedCallbacks;
    public PooledObject QuadPrefab;
    public string WorldMapFilePath = "Levels/WorldMap/World.json";
    public string LevelEditorSceneName = "LevelEditing";
    public float LevelLoadTime = 1.0f;

    public IntegerRectCollider GetBounds()
    {
        return this.WorldBounds;
    }

    void Start()
    {
        this.Load();
        this.NewLevelPanel.CompletionCallback = levelCreated;
        this.Grid.InitializeGridForSize(_worldInfo.width, _worldInfo.height);
        _quadVisuals = new Dictionary<string, WorldEditorQuadVisual>();
        this.WorldPanel.sizeDelta = new Vector2(_worldInfo.width * this.GridSpaceRenderSize, _worldInfo.height * this.GridSpaceRenderSize);
        this.WorldBounds.Size = this.WorldPanel.sizeDelta;
        this.WorldBounds.Offset = this.WorldPanel.sizeDelta / 2;

        for (int i = 0; i < _worldInfo.level_quads.Length; ++i)
        {
            WorldInfo.LevelQuad levelQuad = _worldInfo.level_quads[i];
            _quadVisuals.Add(levelQuad.name, loadQuad(levelQuad));
        }

        this.ContextMenu.EnterState(hoveredQuadVisual() == null ? NO_SELECTION_STATE : HOVER_STATE);
        _cursorPrev = this.Cursor.GridPos;
    }

    void Update()
    {
        if (_exiting || _paused)
            return;

        if (MapEditorInput.Confirm)
        {
            if (_selectedQuad == null)
            {
                WorldEditorQuadVisual hoveredQuad = hoveredQuadVisual();
                if (hoveredQuad != null)
                {
                    this.ContextMenu.EnterState(SELECTION_STATE);
                    _selectedQuad = hoveredQuad;
                    _selectedQuad.Select();
                    this.Cursor.Hide();
                }
            }
            else
            {
                this.ContextMenu.EnterState(HOVER_STATE);
                this.Cursor.UnHide();
                this.Cursor.MoveToGridPos();
                _selectedQuad.UnSelect();
                _selectedQuad = null;
            }
        }
        else if (_selectedQuad != null)
        {
            if (MapEditorInput.NavLeft && canMoveLeft(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                --newPos.X;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X - 1, this.Cursor.GridPos.Y);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MapEditorInput.NavRight && canMoveRight(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                ++newPos.X;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X + 1, this.Cursor.GridPos.Y);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MapEditorInput.NavDown && canMoveDown(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                --newPos.Y;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X, this.Cursor.GridPos.Y - 1);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MapEditorInput.NavUp && canMoveUp(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                ++newPos.Y;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X, this.Cursor.GridPos.Y + 1);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MapEditorInput.Exit)
            {
                _worldInfo.RemoveLevelQuad(_selectedQuad.QuadName);
                _quadVisuals.Remove(_selectedQuad.QuadName);
                File.Delete(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + _selectedQuad.QuadName + MapLoader.JSON_SUFFIX);
                _selectedQuad.GetComponent<PooledObject>().Release();
                _selectedQuad = null;
                this.ContextMenu.EnterState(NO_SELECTION_STATE);
                this.Cursor.MoveToGridPos();
                this.Cursor.UnHide();
                this.Save();
            }
            else if (MapEditorInput.ResizeLeft && _selectedQuad.QuadBounds.Size.X > 1)
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                --levelQuad.width;
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                quadInfo.ResizeWidth(levelQuad.width);
                saveQuadResize(levelQuad, quadInfo);
            }
            else if (MapEditorInput.ResizeRight && canMoveRight(_selectedQuad))
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                ++levelQuad.width;
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                quadInfo.ResizeWidth(levelQuad.width);
                saveQuadResize(levelQuad, quadInfo);
            }
            else if (MapEditorInput.ResizeDown && _selectedQuad.QuadBounds.Size.Y > 1)
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                --levelQuad.height;
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                quadInfo.ResizeHeight(levelQuad.height);
                saveQuadResize(levelQuad, quadInfo);
            }
            else if (MapEditorInput.ResizeUp && canMoveUp(_selectedQuad))
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                ++levelQuad.height;
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                quadInfo.ResizeHeight(levelQuad.height);
                saveQuadResize(levelQuad, quadInfo);
            }
        }
        else if (MapEditorInput.Start)
        {
            this.Save();
        }
        else if (MapEditorInput.Action)
        {
            WorldEditorQuadVisual hoveredQuad = hoveredQuadVisual();
            if (hoveredQuad != null)
            {
                this.Save();
                this.Cursor.Hide();
                ScenePersistentLoading.BeginLoading(hoveredQuad.QuadName);
                _exiting = true;
                this.TimedCallbacks.AddCallback(this, loadLevelEditor, this.LevelLoadTime);
                this.FadeOut.Run();
            }
            else
            {
                pause();
                this.NewLevelPanel.Show();
            }
        }

        if (_selectedQuad == null && _cursorPrev != this.Cursor.GridPos)
        {
            WorldEditorQuadVisual hoveredQuad = hoveredQuadVisual();
            if (hoveredQuad != null)
                this.ContextMenu.EnterState(HOVER_STATE);
            else
                this.ContextMenu.EnterState(NO_SELECTION_STATE);
        }
        _cursorPrev = this.Cursor.GridPos;
    }

    public void Save()
    {
        this.SaveIcon.Run();

        for (int i = 0; i < _worldInfo.level_quads.Length; ++i)
        {
            WorldInfo.LevelQuad levelQuad = _worldInfo.level_quads[i];
            WorldEditorQuadVisual quadVisual = _quadVisuals[levelQuad.name];
            levelQuad.x = quadVisual.QuadBounds.Min.X;
            levelQuad.y = quadVisual.QuadBounds.Min.Y;
            levelQuad.width = quadVisual.QuadBounds.Size.X;
            levelQuad.height = quadVisual.QuadBounds.Size.Y;
        }
        
        File.WriteAllText(Application.streamingAssetsPath + "/" + this.WorldMapFilePath, JsonConvert.SerializeObject(_worldInfo, Formatting.Indented));
    }

    public void Load()
    {
        string path = Application.streamingAssetsPath + "/" + this.WorldMapFilePath;
        if (File.Exists(path))
        {
            _worldInfo = JsonConvert.DeserializeObject<WorldInfo>(File.ReadAllText(path));
        }
        else
        {
            _worldInfo = mapInfoToWorldInfo(WorldLoadingManager.ReadWorldMapInfo(this.WorldMapName), this.WorldGridSpaceSize);
        }
    }

    /**
     * Private
     */
    private const string HOVER_STATE = "Hover";
    private const string SELECTION_STATE = "Selection";
    private const string NO_SELECTION_STATE = "NoSelection";
    private Dictionary<string, WorldEditorQuadVisual> _quadVisuals;
    private WorldEditorQuadVisual _selectedQuad;
    private WorldInfo _worldInfo;
    private IntegerVector _cursorPrev;
    private bool _exiting;
    private bool _paused;

    private void saveQuadResize(WorldInfo.LevelQuad levelQuad, NewMapInfo quadInfo)
    {
        _selectedQuad.ConfigureForQuad(levelQuad.name, quadInfo, this.WorldGridSpaceSize, this.GridSpaceRenderSize, new IntegerVector(levelQuad.x, levelQuad.y));
        _selectedQuad.MoveToGridPos(this.Grid);
        File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + levelQuad.name + MapLoader.JSON_SUFFIX, JsonConvert.SerializeObject(quadInfo, Formatting.Indented));
        this.Save();
    }

    private WorldEditorQuadVisual loadQuad(WorldInfo.LevelQuad levelQuad)
    {
        PooledObject quadVisualObject = this.QuadPrefab.Retain();
        ((RectTransform)quadVisualObject.transform).SetParent(this.WorldPanel, false);
        IntegerVector pos = new IntegerVector(levelQuad.x, levelQuad.y);
        IntegerVector size = new IntegerVector(levelQuad.width, levelQuad.height);

        //((RectTransform)quadVisualObject.transform).sizeDelta = new Vector2(size.X * this.GridSpaceRenderSize, size.Y * this.GridSpaceRenderSize);

        WorldEditorQuadVisual quadVisual = quadVisualObject.GetComponent<WorldEditorQuadVisual>();
        quadVisual.ConfigureForQuad(levelQuad.name, MapLoader.GatherMapInfo(levelQuad.name), this.WorldGridSpaceSize, this.GridSpaceRenderSize, pos);
        quadVisual.MoveToGridPos(this.Grid);
        //TODO: Change map object's size if necessary based on loaded quad data
        return quadVisual;
    }

    private void levelCreated(string levelName, string platforms, string background)
    {
        unpause();
        levelName = findUsableName(levelName);
        NewMapInfo newMapInfo = new NewMapInfo(levelName, this.WorldGridSpaceSize * 1, this.WorldGridSpaceSize * 1, MapEditorManager.DEFAULT_TILE_SIZE);
        newMapInfo.AddTileLayer(MapEditorManager.PLATFORMS_LAYER);
        newMapInfo.AddTileLayer(MapEditorManager.BACKGROUND_LAYER);
        NewMapInfo.MapLayer platformsLayer = newMapInfo.GetMapLayer(MapEditorManager.PLATFORMS_LAYER);
        NewMapInfo.MapLayer backgroundLayer = newMapInfo.GetMapLayer(MapEditorManager.BACKGROUND_LAYER);
        platformsLayer.tileset_name = platforms;
        backgroundLayer.tileset_name = background;

        File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + levelName + MapLoader.JSON_SUFFIX, JsonConvert.SerializeObject(newMapInfo, Formatting.Indented));
        _worldInfo.AddLevelQuad(levelName, this.Cursor.GridPos.X, this.Cursor.GridPos.Y, 1, 1);
        _quadVisuals.Add(levelName, loadQuad(_worldInfo.GetLevelQuad(levelName)));
        this.Save();
    }

    private string findUsableName(string startingName)
    {
        bool done = false;
        while (!done)
        {
            done = true;
            foreach (string name in _quadVisuals.Keys)
            {
                if (name == startingName)
                {
                    string finalDigit = name.Substring(name.Length - 1);
                    int digit;
                    if (int.TryParse(finalDigit, out digit))
                        startingName = name.Substring(0, name.Length - 1) + ((int)(digit + 1));
                    else
                        startingName += "0";
                    done = false;
                    break;
                }
            }
        }
        return startingName;
    }

    private void pause()
    {
        _paused = true;
        this.Cursor.Hide();
    }

    private void unpause()
    {
        _paused = false;
        this.Cursor.UnHide();
    }

    private void loadLevelEditor()
    {
        SceneManager.LoadScene(this.LevelEditorSceneName);
    }

    private static WorldInfo mapInfoToWorldInfo(MapInfo mapInfo, int worldGridSpaceSize)
    {
        WorldInfo worldInfo = new WorldInfo();
        worldInfo.width = mapInfo.width;
        worldInfo.height = mapInfo.height;
        worldInfo.tile_size = mapInfo.tilewidth;
        List<WorldInfo.LevelQuad> levelQuads = new List<WorldInfo.LevelQuad>();

        MapInfo.MapLayer layer = mapInfo.GetLayerWithName(WorldLoadingManager.LAYER);
        for (int i = 0; i < layer.objects.Length; ++i)
        {
            MapInfo.MapObject mapObject = layer.objects[i];
            WorldInfo.LevelQuad levelQuad = new WorldInfo.LevelQuad();
            levelQuad.name = mapObject.name;
            levelQuad.x = mapObject.x / worldGridSpaceSize;
            levelQuad.y = mapObject.y / worldGridSpaceSize;
            levelQuad.width = mapObject.width / worldGridSpaceSize;
            levelQuad.height = mapObject.height / worldGridSpaceSize;
            levelQuads.Add(levelQuad);
        }

        worldInfo.level_quads = levelQuads.ToArray();
        return worldInfo;
    }

    private WorldEditorQuadVisual hoveredQuadVisual()
    {
        //TODO: Iterating through all quads here probably not great - should probably have a grid system of storage
        foreach (WorldEditorQuadVisual quadVisual in _quadVisuals.Values)
        {
            if (quadVisual.QuadBounds.Contains(this.Cursor.GridPos) && (quadVisual.QuadBounds.Size.X % 2 == 1 || this.Cursor.GridPos.X != quadVisual.QuadBounds.Max.X) && (quadVisual.QuadBounds.Size.X % 2 == 1 || this.Cursor.GridPos.Y != quadVisual.QuadBounds.Max.Y))
            {
                return quadVisual;
            }
        }

        return null;
    }

    private bool canMoveLeft(WorldEditorQuadVisual quadVisual)
    {
        IntegerRect newBounds = quadVisual.QuadBounds;
        if (newBounds.Min.X <= 0)
            return false;
        newBounds.Center = new IntegerVector(newBounds.Center.X - 1, newBounds.Center.Y);
        return validQuadLocation(newBounds, quadVisual);
    }

    private bool canMoveRight(WorldEditorQuadVisual quadVisual)
    {
        IntegerRect newBounds = quadVisual.QuadBounds;
        if (newBounds.Max.X >= this.Grid.Width)
            return false;
        newBounds.Center = new IntegerVector(newBounds.Center.X + 1, newBounds.Center.Y);
        return validQuadLocation(newBounds, quadVisual);
    }

    private bool canMoveDown(WorldEditorQuadVisual quadVisual)
    {
        IntegerRect newBounds = quadVisual.QuadBounds;
        if (newBounds.Min.Y <= 0)
            return false;
        newBounds.Center = new IntegerVector(newBounds.Center.X, newBounds.Center.Y - 1);
        return validQuadLocation(newBounds, quadVisual);
    }

    private bool canMoveUp(WorldEditorQuadVisual quadVisual)
    {
        IntegerRect newBounds = quadVisual.QuadBounds;
        if (newBounds.Max.Y >= this.Grid.Height)
            return false;
        newBounds.Center = new IntegerVector(newBounds.Center.X, newBounds.Center.Y + 1);
        return validQuadLocation(newBounds, quadVisual);
    }

    private bool validQuadLocation(IntegerRect bounds, WorldEditorQuadVisual quadVisual = null)
    {

        foreach (WorldEditorQuadVisual other in _quadVisuals.Values)
        {
            if (other == quadVisual)
                continue;
            if (other.QuadBounds.Overlaps(bounds))
                return false;
        }

        return true;
    }
}
