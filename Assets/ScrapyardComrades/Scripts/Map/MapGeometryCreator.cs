using UnityEngine;
using System.Collections.Generic;

public class MapGeometryCreator : VoBehavior
{
    public GameObject PlatformGeometryPrefab;
    public int[] TileTypesToIgnore;
    public int TileRenderSize = 20;
    public int MaxSolidsToStore = 2048;
    public bool FlipVertical = true;

    public void CreateGeometryForGrid(MapGridSpaceInfo[,] grid)
    {
        if (_unusedGeometry == null)
            _unusedGeometry = new List<IntegerCollider>();
        if (_usedGeometry == null)
            _usedGeometry = new List<IntegerCollider>();

        int halfSize = this.TileRenderSize / 2;
        for (int x = 0; x < grid.GetLength(0); ++x)
        {
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                if (!shouldIgnore(grid[x, y]))
                {
                    IntegerCollider geom = findAvailableGeometry();
                    int posY = this.FlipVertical ? (grid.GetLength(1) - y - 1) * this.TileRenderSize : y * this.TileRenderSize;
                    geom.transform.localPosition = new Vector3(x * this.TileRenderSize + halfSize, posY + halfSize, 0);
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

            if (_unusedGeometry != null)
                _unusedGeometry.Clear();
            if (_usedGeometry != null)
                _usedGeometry.Clear();
        }
        else if (_unusedGeometry != null)
        {
            // If not in editor, only destroy down to max storage count, then simply disable the rest
            int numToDestroy = this.transform.childCount - this.MaxSolidsToStore;
            int destroyedCount = 0;

            while (_usedGeometry.Count > 0)
            {
                int index = _usedGeometry.Count - 1;
                IntegerCollider collider = _usedGeometry[index];
                _usedGeometry.RemoveAt(index);

                if (destroyedCount < numToDestroy)
                {
                    Destroy(collider);
                    ++destroyedCount;
                }
                else
                {
                    //collider.RemoveFromCollisionPool(); // Now using CollisionManager.RemoveAllSolids instead
                    collider.enabled = false;
                    _unusedGeometry.Add(collider);
                }
            }
        }
    }

    public void AddColliders()
    {
        if (_usedGeometry != null)
        {
            for (int i = 0; i < _usedGeometry.Count; ++i)
            {
                _usedGeometry[i].AddToCollisionPool();
            }
        }
    }

    /**
     * Private
     */
    private List<IntegerCollider> _unusedGeometry;
    private List<IntegerCollider> _usedGeometry;

    private bool shouldIgnore(MapGridSpaceInfo tile)
    {
        if (this.TileTypesToIgnore != null)
        {
            for (int i = 0; i < this.TileTypesToIgnore.Length; ++i)
            {
                if (this.TileTypesToIgnore[i] == tile.TileId)
                    return true;
            }
        }

        return false;
    }

    private IntegerCollider findAvailableGeometry()
    {
        IntegerCollider geom;

        if (_unusedGeometry.Count > 0)
        {
            geom = _unusedGeometry[_unusedGeometry.Count - 1];
            _unusedGeometry.RemoveAt(_unusedGeometry.Count - 1);
            geom.enabled = true;
        }
        else
        {
            GameObject go = Instantiate<GameObject>(this.PlatformGeometryPrefab);
            geom = go.GetComponent<IntegerCollider>();
            geom.transform.parent = this.transform;
        }

        _usedGeometry.Add(geom);
        return geom;
    }
}
