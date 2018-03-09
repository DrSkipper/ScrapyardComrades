using UnityEngine;
using System.Collections.Generic;

public class MapGeometryCreator : VoBehavior
{
    public PooledObject PlatformGeometryPrefab;
    public int TileRenderSize = 20;
    public bool FlipVertical = true;

    public void CreateGeometryForGrid(NewMapInfo.MapTile[,] grid, Dictionary<string, TilesetData.SpriteData> spriteData, bool editor)
    {
        if (_geometry == null)
            _geometry = new List<IntegerCollider>();

        int halfSize = this.TileRenderSize / 2;
        for (int x = 0; x < grid.GetLength(0); ++x)
        {
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                if (!shouldIgnore(grid[x, y], spriteData))
                {
                    IntegerRectCollider geom = aquireGeometry(editor);
                    //TODO: Base size of collider on TilesetData
                    geom.Size = new IntegerVector(this.TileRenderSize, this.TileRenderSize);
                    int posY = this.FlipVertical ? (grid.GetLength(1) - y - 1) * this.TileRenderSize : y * this.TileRenderSize;
                    geom.transform.localPosition = new Vector3(x * this.TileRenderSize + halfSize, posY + halfSize, 0);
                    _geometry.Add(geom);
                }
            }
        }
    }

    public void Clear(bool editor = false)
    {
        if (editor)
        {
            while (this.transform.childCount > 0)
            {
                DestroyImmediate(this.transform.GetChild(0).gameObject);
            }

            if (_geometry != null)
                _geometry.Clear();
        }
        else if (_geometry != null)
        {
            while (_geometry.Count > 0)
            {
                //NOTE: No need to remove from collision pool here, as we're now just calling "CollisionManager.RemoveAllSolids()"
                IntegerCollider geo = _geometry.Pop();
                geo.transform.SetParent(null);
                ObjectPools.Release(geo.gameObject);
            }
        }
    }

    public void AddColliders()
    {
        if (_geometry != null)
        {
            for (int i = 0; i < _geometry.Count; ++i)
            {
                _geometry[i].AddToCollisionPool();
            }
        }
    }

    /**
     * Private
     */
    private List<IntegerCollider> _geometry;

    private bool shouldIgnore(NewMapInfo.MapTile tile, Dictionary<string, TilesetData.SpriteData> spriteData)
    {
        if (!tile.is_filled || !spriteData.ContainsKey(tile.sprite_name))
            return true;
        TilesetData.TileType type = spriteData[tile.sprite_name].Type;
        if (type == TilesetData.TileType.Surrounded || type == TilesetData.TileType.Empty)
            return true;
        return false;
    }

    private IntegerRectCollider aquireGeometry(bool editor)
    {
        PooledObject obj = !editor ? this.PlatformGeometryPrefab.Retain() : Instantiate<PooledObject>(this.PlatformGeometryPrefab);
        IntegerRectCollider geom = obj.GetComponent<IntegerRectCollider>();
        geom.transform.parent = this.transform;
        return geom;
    }
}
