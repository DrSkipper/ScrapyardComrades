using UnityEngine;
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
    public PooledObject QuadPrefab;
    public string WorldMapFilePath = "Levels/WorldMap/World.json";

    public IntegerRectCollider GetBounds()
    {
        return this.WorldBounds;
    }

    void Start()
    {
        this.Load();
        this.Grid.InitializeGridForSize(_worldInfo.width, _worldInfo.height);
        _quadVisuals = new Dictionary<string, WorldEditorQuadVisual>();
        this.WorldPanel.sizeDelta = new Vector2(_worldInfo.width * this.GridSpaceRenderSize, _worldInfo.height * this.GridSpaceRenderSize);
        this.WorldBounds.Size = this.WorldPanel.sizeDelta;
        this.WorldBounds.Offset = this.WorldPanel.sizeDelta / 2;

        for (int i = 0; i < _worldInfo.level_quads.Length; ++i)
        {
            WorldInfo.LevelQuad levelQuad = _worldInfo.level_quads[i];
            PooledObject quadVisualObject = this.QuadPrefab.Retain();
            ((RectTransform)quadVisualObject.transform).SetParent(this.WorldPanel, false);
            IntegerVector pos = new IntegerVector(levelQuad.x, levelQuad.y);
            IntegerVector size = new IntegerVector(levelQuad.width, levelQuad.height);
            
            ((RectTransform)quadVisualObject.transform).sizeDelta = new Vector2((size.X) * this.GridSpaceRenderSize, (size.Y) * this.GridSpaceRenderSize);

            WorldEditorQuadVisual quadVisual = quadVisualObject.GetComponent<WorldEditorQuadVisual>();
            quadVisual.ConfigureForQuad(levelQuad.name, MapLoader.GatherMapInfo(levelQuad.name), this.WorldGridSpaceSize, this.GridSpaceRenderSize, pos);
            quadVisual.MoveToGridPos(this.Grid);
            //TODO: Change map object's size if necessary based on loaded quad data
            _quadVisuals.Add(levelQuad.name, quadVisual);
        }

        this.ContextMenu.EnterState(hoveredQuadVisual() == null ? NO_SELECTION_STATE : HOVER_STATE);
        _cursorPrev = this.Cursor.GridPos;
    }

    void Update()
    {
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
        }
        else if (MapEditorInput.Exit) //TODO: Don't use exit key
        {
            this.Save();
        }

        if (_selectedQuad == null && _cursorPrev != this.Cursor.GridPos)
        {
            WorldEditorQuadVisual hoveredQuad = hoveredQuadVisual();
            if (hoveredQuad)
                this.ContextMenu.EnterState(HOVER_STATE);
            else
                this.ContextMenu.EnterState(NO_SELECTION_STATE);
        }
        _cursorPrev = this.Cursor.GridPos;
    }

    public void Save()
    {
        for (int i = 0; i < _worldInfo.level_quads.Length; ++i)
        {
            WorldInfo.LevelQuad levelQuad = _worldInfo.level_quads[i];
            WorldEditorQuadVisual quadVisual = _quadVisuals[levelQuad.name];
            levelQuad.x = quadVisual.QuadBounds.Min.X;
            levelQuad.y = quadVisual.QuadBounds.Min.Y;
            levelQuad.width = quadVisual.QuadBounds.Size.X;
            levelQuad.height = quadVisual.QuadBounds.Size.Y;
        }

        string path = Application.streamingAssetsPath + "/" + this.WorldMapFilePath;
        File.WriteAllText(path, JsonConvert.SerializeObject(_worldInfo));
    }

    public void Load()
    {
        string path = Application.streamingAssetsPath + "/" + this.WorldMapFilePath;
        if (File.Exists(path))
        {
            _worldInfo = JsonConvert.DeserializeObject<WorldInfo>(File.ReadAllText(path));
            //_worldInfo = mapInfoToWorldInfo(JsonConvert.DeserializeObject<MapInfo>(File.ReadAllText(path)));
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
            //TODO: Not sure why I have to check against the outside border of the bounds here
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
