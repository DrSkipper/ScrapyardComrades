using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class WorldMapEditorManager : MonoBehaviour, CameraBoundsHandler
{
    public string WorldMapName = "World";
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
    public string LevelEditorSceneName = "LevelEditing";
    public string TestSceneName = "MapLoadingTest";
    public float LevelLoadTime = 1.0f;

    public IntegerRectCollider GetBounds()
    {
        return this.WorldBounds;
    }

    void Awake()
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

        if (MenuInput.Confirm)
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
            if (MenuInput.NavLeft && canMoveLeft(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                --newPos.X;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X - 1, this.Cursor.GridPos.Y);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MenuInput.NavRight && canMoveRight(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                ++newPos.X;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X + 1, this.Cursor.GridPos.Y);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MenuInput.NavDown && canMoveDown(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                --newPos.Y;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X, this.Cursor.GridPos.Y - 1);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MenuInput.NavUp && canMoveUp(_selectedQuad))
            {
                IntegerVector newPos = _selectedQuad.QuadBounds.Center;
                ++newPos.Y;
                this.Cursor.GridPos = new IntegerVector(this.Cursor.GridPos.X, this.Cursor.GridPos.Y + 1);
                _selectedQuad.QuadBounds = new IntegerRect(newPos, _selectedQuad.QuadBounds.Size);
                this.Cursor.MoveToGridPos();
                _selectedQuad.MoveToGridPos(this.Grid);
            }
            else if (MenuInput.Exit)
            {
                _worldInfo.RemoveLevelQuad(_selectedQuad.QuadName);
                _quadVisuals.Remove(_selectedQuad.QuadName);
                File.Delete(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + _selectedQuad.QuadName + StringExtensions.JSON_SUFFIX);
                _selectedQuad.GetComponent<PooledObject>().Release();
                _selectedQuad = null;
                this.ContextMenu.EnterState(NO_SELECTION_STATE);
                this.Cursor.MoveToGridPos();
                this.Cursor.UnHide();
                this.Save();
            }
            else if (MenuInput.ResizeLeft && _selectedQuad.QuadBounds.Size.X > 1)
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                applyQuadVisualToLevelQuad(_selectedQuad, levelQuad);
                --levelQuad.width;
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                quadInfo.ResizeWidth(levelQuad.width * this.WorldGridSpaceSize);
                saveQuadResize(levelQuad, quadInfo);
            }
            else if (MenuInput.ResizeRight && canMoveRight(_selectedQuad))
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                applyQuadVisualToLevelQuad(_selectedQuad, levelQuad);
                if ((levelQuad.width + 1) * levelQuad.height * this.WorldGridSpaceSize <= MAX_TILES_IN_QUAD)
                {
                    ++levelQuad.width;
                    NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                    quadInfo.ResizeWidth(levelQuad.width * this.WorldGridSpaceSize);
                    saveQuadResize(levelQuad, quadInfo);
                }
            }
            else if (MenuInput.ResizeDown && _selectedQuad.QuadBounds.Size.Y > 1)
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                applyQuadVisualToLevelQuad(_selectedQuad, levelQuad);
                --levelQuad.height;
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                quadInfo.ResizeHeight(levelQuad.height * this.WorldGridSpaceSize);
                saveQuadResize(levelQuad, quadInfo);
            }
            else if (MenuInput.ResizeUp && canMoveUp(_selectedQuad))
            {
                WorldInfo.LevelQuad levelQuad = _worldInfo.GetLevelQuad(_selectedQuad.QuadName);
                applyQuadVisualToLevelQuad(_selectedQuad, levelQuad);
                if (levelQuad.width * (levelQuad.height + 1) * this.WorldGridSpaceSize <= MAX_TILES_IN_QUAD)
                {
                    ++levelQuad.height;
                    NewMapInfo quadInfo = MapLoader.GatherMapInfo(levelQuad.name);
                    quadInfo.ResizeHeight(levelQuad.height * this.WorldGridSpaceSize);
                    saveQuadResize(levelQuad, quadInfo);
                }
            }
        }
        else if (MenuInput.Start)
        {
            this.Save();
        }
        else if (MenuInput.Action)
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
                GlobalEvents.Notifier.SendEvent(new BeginSceneTransitionEvent(this.LevelEditorSceneName));
            }
            else
            {
                pause();
                this.NewLevelPanel.Show();
            }
        }
        else if (MenuInput.SwapModes)
        {
            WorldEditorQuadVisual hoveredQuad = hoveredQuadVisual();
            if (hoveredQuad != null)
            {
                this.Save();
                this.Cursor.Hide();
                ScenePersistentLoading.BeginLoading(hoveredQuad.QuadName, true);
                _exiting = true;
                this.TimedCallbacks.AddCallback(this, loadTestScene, this.LevelLoadTime);
                this.FadeOut.Run();
                GlobalEvents.Notifier.SendEvent(new BeginSceneTransitionEvent(this.TestSceneName));
            }
        }
        else if (MenuInput.Cancel)
        {
            WorldEditorQuadVisual hoveredQuad = hoveredQuadVisual();
            if (hoveredQuad != null)
            {
                NewMapInfo quadInfo = MapLoader.GatherMapInfo(hoveredQuad.QuadName);
                IntegerVector pos = findBestOpenSpotForNewLevel(hoveredQuad.QuadBounds.Size.X, hoveredQuad.QuadBounds.Size.Y);

                string levelName = findUsableName(quadInfo.name);
                string oldName = quadInfo.name;
                quadInfo.name = levelName;
                File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + levelName + StringExtensions.JSON_SUFFIX, JsonConvert.SerializeObject(quadInfo, Formatting.Indented));
                _worldInfo.AddLevelQuad(levelName, pos.X, pos.Y, hoveredQuad.QuadBounds.Size.X, hoveredQuad.QuadBounds.Size.Y);
                _quadVisuals.Add(levelName, loadQuad(_worldInfo.GetLevelQuad(levelName)));
                quadInfo.name = oldName;
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
            applyQuadVisualToLevelQuad(quadVisual, levelQuad);
        }
        
        File.WriteAllText(Application.streamingAssetsPath + WorldLoadingManager.WORLD_INFO_PATH + this.WorldMapName + StringExtensions.JSON_SUFFIX, JsonConvert.SerializeObject(_worldInfo, Formatting.Indented));
    }

    public void Load()
    {
        _worldInfo = WorldLoadingManager.ReadWorldMapInfo(this.WorldMapName);
    }

    /**
     * Private
     */
    private const string HOVER_STATE = "Hover";
    private const string SELECTION_STATE = "Selection";
    private const string NO_SELECTION_STATE = "NoSelection";
    private const int MAX_TILES_IN_QUAD = 15000; //8192;
    private Dictionary<string, WorldEditorQuadVisual> _quadVisuals;
    private WorldEditorQuadVisual _selectedQuad;
    private WorldInfo _worldInfo;
    private IntegerVector _cursorPrev;
    private bool _exiting;
    private bool _paused;

    private IntegerVector findBestOpenSpotForNewLevel(int w, int h)
    {
        int searchSize = Mathf.Min(_worldInfo.width, _worldInfo.height);
        for (int r = 0; r < searchSize; ++r)
        {
            for (int x = 0; x <= r; ++x)
            {
                if (canCreateLevel(x, r, w, h))
                    return new IntegerVector(x, r);
            }

            for (int y = 0; y < r; ++y)
            {
                if (canCreateLevel(r, y, w, h))
                    return new IntegerVector(r, y);
            }
        }
        return this.Cursor.GridPos;
    }

    private bool canCreateLevel(int x, int y, int w, int h)
    {
        IntegerRect bounds = IntegerRect.CreateFromMinMax(new IntegerVector(x, y), new IntegerVector(x + w, y + h));
        foreach (WorldEditorQuadVisual quad in _quadVisuals.Values)
        {
            if (bounds.Overlaps(quad.QuadBounds))
                return false;
        }
        return true;
    }

    private void applyQuadVisualToLevelQuad(WorldEditorQuadVisual quadVisual, WorldInfo.LevelQuad levelQuad)
    {
        levelQuad.x = quadVisual.QuadBounds.Min.X;
        levelQuad.y = quadVisual.QuadBounds.Min.Y;
        levelQuad.width = quadVisual.QuadBounds.Size.X;
        levelQuad.height = quadVisual.QuadBounds.Size.Y;
    }

    private void saveQuadResize(WorldInfo.LevelQuad levelQuad, NewMapInfo quadInfo)
    {
        _selectedQuad.ConfigureForQuad(levelQuad.name, quadInfo, this.WorldGridSpaceSize, this.GridSpaceRenderSize, new IntegerVector(levelQuad.x, levelQuad.y));
        _selectedQuad.MoveToGridPos(this.Grid);
        File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + levelQuad.name + StringExtensions.JSON_SUFFIX, JsonConvert.SerializeObject(quadInfo, Formatting.Indented));
        this.Save();
    }

    private WorldEditorQuadVisual loadQuad(WorldInfo.LevelQuad levelQuad)
    {
        PooledObject quadVisualObject = this.QuadPrefab.Retain();
        ((RectTransform)quadVisualObject.transform).SetParent(this.WorldPanel, false);
        IntegerVector pos = new IntegerVector(levelQuad.x, levelQuad.y);
        WorldEditorQuadVisual quadVisual = quadVisualObject.GetComponent<WorldEditorQuadVisual>();
        quadVisual.ConfigureForQuad(levelQuad.name, MapLoader.GatherMapInfo(levelQuad.name), this.WorldGridSpaceSize, this.GridSpaceRenderSize, pos);
        quadVisual.MoveToGridPos(this.Grid);
        //TODO: Change level quad's size if necessary based on loaded quad data
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

        File.WriteAllText(Application.streamingAssetsPath + MapLoader.LEVELS_PATH + levelName + StringExtensions.JSON_SUFFIX, JsonConvert.SerializeObject(newMapInfo, Formatting.Indented));
        _worldInfo.AddLevelQuad(levelName, this.Cursor.GridPos.X, this.Cursor.GridPos.Y, 1, 1);
        _quadVisuals.Add(levelName, loadQuad(_worldInfo.GetLevelQuad(levelName)));
        this.Save();
    }

    private string findUsableName(string startingName, int minDigits = 2)
    {
        bool done = false;
        while (!done)
        {
            // Keep incrementing name's number until we have something that doesn't exist yet
            done = true;
            foreach (string name in _quadVisuals.Keys)
            {
                // If this name already exists
                if (name == startingName)
                {
                    // Get the number at the end of the name (if there is one)
                    string digits = name;
                    int index = name.LastIndexOf(StringExtensions.SPACE) + 1;
                    if (index > 0)
                    {
                        digits = index < name.Length ? name.Substring(index, name.Length - index) : StringExtensions.EMPTY;

                        // Get the name before the number
                        startingName = name.Substring(0, index);
                    }
                    else
                    {
                        startingName = StringExtensions.EMPTY;
                    }

                    // Evaluate the int value of the number at the end of the name
                    int number;
                    if (int.TryParse(digits, out number))
                    {
                        // Increment the number and guarantee the min number of digits desired
                        ++number;
                        digits = StringExtensions.EMPTY + number;
                        while (digits.Length < minDigits)
                            digits = "0" + digits;
                        startingName += digits;
                    }
                    else
                    {
                        startingName += "0";
                    }
                    
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

    private void loadTestScene()
    {
        SceneManager.LoadScene(this.TestSceneName);
    }

    private WorldEditorQuadVisual hoveredQuadVisual()
    {
        //TODO: Iterating through all quads here probably not great - should probably have a grid system of storage
        foreach (WorldEditorQuadVisual quadVisual in _quadVisuals.Values)
        {
            if (quadVisual.QuadBounds.Contains(this.Cursor.GridPos) && this.Cursor.GridPos.X != quadVisual.QuadBounds.Max.X && this.Cursor.GridPos.Y != quadVisual.QuadBounds.Max.Y)
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
