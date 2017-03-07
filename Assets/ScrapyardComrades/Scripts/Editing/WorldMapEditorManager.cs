using UnityEngine;
using System.Collections.Generic;

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

    public IntegerRectCollider GetBounds()
    {
        return this.WorldBounds;
    }

    void Start()
    {
        MapInfo mapInfo = WorldLoadingManager.ReadWorldMapInfo(this.WorldMapName);
        this.Grid.InitializeGridForSize(mapInfo.width, mapInfo.height);
        MapInfo.MapLayer layer = mapInfo.GetLayerWithName(WorldLoadingManager.LAYER);
        _quadVisuals = new Dictionary<string, WorldEditorQuadVisual>();
        this.WorldPanel.sizeDelta = new Vector2((mapInfo.width - 1) * this.GridSpaceRenderSize, (mapInfo.height - 1) * this.GridSpaceRenderSize);
        this.WorldBounds.Size = this.WorldPanel.sizeDelta;
        this.WorldBounds.Offset = this.WorldPanel.sizeDelta / 2;

        for (int i = 0; i < layer.objects.Length; ++i)
        {
            MapInfo.MapObject mapObject = layer.objects[i];
            PooledObject quadVisualObject = this.QuadPrefab.Retain();
            ((RectTransform)quadVisualObject.transform).SetParent(this.WorldPanel, false);
            IntegerVector pos = new IntegerVector(mapObject.x / this.WorldGridSpaceSize, this.FlipVertical ? mapInfo.height - ((mapObject.height + mapObject.y) / this.WorldGridSpaceSize) : mapObject.y / this.WorldGridSpaceSize);
            IntegerVector size = new IntegerVector(mapObject.width / this.WorldGridSpaceSize, mapObject.height / this.WorldGridSpaceSize);

            quadVisualObject.transform.SetLocalPosition2D(pos.X * this.GridSpaceRenderSize, pos.Y * this.GridSpaceRenderSize);
            ((RectTransform)quadVisualObject.transform).sizeDelta = new Vector2((size.X) * this.GridSpaceRenderSize, (size.Y) * this.GridSpaceRenderSize);

            WorldEditorQuadVisual quadVisual = quadVisualObject.GetComponent<WorldEditorQuadVisual>();
            quadVisual.Text.text = mapObject.name;
            IntegerRect bounds = new IntegerRect(IntegerVector.Zero, size);
            bounds.Min = pos;
            bounds.Max = pos + size;
            quadVisual.QuadBounds = bounds;
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
                    if (quadVisual.QuadBounds.Contains(this.Cursor.GridPos))
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
    }

    /**
     * Private
     */
    private Dictionary<string, WorldEditorQuadVisual> _quadVisuals;
    private WorldEditorQuadVisual _selectedQuad;

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
        if (newBounds.Max.X >= this.Grid.Width - 1)
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
        if (newBounds.Max.Y >= this.Grid.Height - 1)
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
