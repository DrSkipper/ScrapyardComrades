using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class WorldMapEditorManager : MonoBehaviour, CameraBoundsHandler
{
    public string WorldMapName;
    public int WorldGridSpaceSize = 16;
    public int GridSpaceRenderSize = 20;
    public bool FlipVertical = true;
    public RectTransform WorldPanel;
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
        this.Grid.InitializeGridForSize(_mapInfo.width, _mapInfo.height);
        MapInfo.MapLayer layer = _mapInfo.GetLayerWithName(WorldLoadingManager.LAYER);
        _quadVisuals = new Dictionary<string, WorldEditorQuadVisual>();
        this.WorldPanel.sizeDelta = new Vector2(_mapInfo.width * this.GridSpaceRenderSize, _mapInfo.height * this.GridSpaceRenderSize);
        this.WorldBounds.Size = this.WorldPanel.sizeDelta;
        this.WorldBounds.Offset = this.WorldPanel.sizeDelta / 2;

        for (int i = 0; i < layer.objects.Length; ++i)
        {
            MapInfo.MapObject mapObject = layer.objects[i];
            PooledObject quadVisualObject = this.QuadPrefab.Retain();
            ((RectTransform)quadVisualObject.transform).SetParent(this.WorldPanel, false);
            IntegerVector pos = new IntegerVector(mapObject.x / this.WorldGridSpaceSize, this.FlipVertical ? _mapInfo.height - ((mapObject.height + mapObject.y) / this.WorldGridSpaceSize) : mapObject.y / this.WorldGridSpaceSize);
            IntegerVector size = new IntegerVector(mapObject.width / this.WorldGridSpaceSize, mapObject.height / this.WorldGridSpaceSize);

            quadVisualObject.transform.SetLocalPosition2D(pos.X * this.GridSpaceRenderSize, pos.Y * this.GridSpaceRenderSize);
            ((RectTransform)quadVisualObject.transform).sizeDelta = new Vector2((size.X) * this.GridSpaceRenderSize, (size.Y) * this.GridSpaceRenderSize);

            WorldEditorQuadVisual quadVisual = quadVisualObject.GetComponent<WorldEditorQuadVisual>();
            quadVisual.ConfigureForQuad(mapObject.name, MapLoader.GatherMapInfo(mapObject.name), this.WorldGridSpaceSize, this.GridSpaceRenderSize, pos);
            //TODO: Change map object's size if necessary based on loaded quad data
            _quadVisuals.Add(mapObject.name, quadVisual);
        }
    }

    void Update()
    {
        if (MapEditorInput.Confirm)
        {
            if (_selectedQuad == null)
            {
                //TODO: Iterating through all quads here probably not great - should probably have a grid system of storage;
                foreach (WorldEditorQuadVisual quadVisual in _quadVisuals.Values)
                {
                    //TODO: Not sure why I have to check against the outside border of the bounds here
                    if (quadVisual.QuadBounds.Contains(this.Cursor.GridPos) && this.Cursor.GridPos.X != quadVisual.QuadBounds.Max.X && this.Cursor.GridPos.Y != quadVisual.QuadBounds.Max.Y)
                    {
                        _selectedQuad = quadVisual;
                        break;
                    }
                }

                if (_selectedQuad != null)
                {
                    _selectedQuad.Select();
                    this.Cursor.Hide();
                }
            }
            else
            {
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
        else if (MapEditorInput.Exit) //TODO: Save option should be in UI, not exit key
        {
            this.Save();
        }
    }

    public void Save()
    {
        MapInfo.MapLayer layer = _mapInfo.GetLayerWithName(WorldLoadingManager.LAYER);

        for (int i = 0; i < layer.objects.Length; ++i)
        {
            MapInfo.MapObject mapObject = layer.objects[i];
            WorldEditorQuadVisual quadVisual = _quadVisuals[mapObject.name];
            mapObject.x = quadVisual.QuadBounds.Min.X * this.WorldGridSpaceSize;
            mapObject.y = quadVisual.QuadBounds.Min.Y * this.WorldGridSpaceSize;
            mapObject.width = quadVisual.QuadBounds.Size.X * this.WorldGridSpaceSize;
            mapObject.height = quadVisual.QuadBounds.Size.Y * this.WorldGridSpaceSize;
        }

        string path = Application.streamingAssetsPath + "/" + this.WorldMapFilePath;
        File.WriteAllText(path, JsonConvert.SerializeObject(_mapInfo));
    }

    public void Load()
    {
        string path = Application.streamingAssetsPath + "/" + this.WorldMapFilePath;
        if (File.Exists(path))
        {
            this.FlipVertical = false;
            _mapInfo = JsonConvert.DeserializeObject<MapInfo>(File.ReadAllText(path));
        }
        else
        {
            this.FlipVertical = true;
            _mapInfo = WorldLoadingManager.ReadWorldMapInfo(this.WorldMapName);
        }
    }

    /**
     * Private
     */
    private Dictionary<string, WorldEditorQuadVisual> _quadVisuals;
    private WorldEditorQuadVisual _selectedQuad;
    private MapInfo _mapInfo;

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
