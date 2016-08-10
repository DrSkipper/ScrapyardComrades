using UnityEngine;

public class MapGeometryCreator : MonoBehaviour
{
    public GameObject PlatformGeometryPrefab;
    public int[] TileTypesToIgnore;
    public int TileRenderSize = 20;
    public bool FlipVertical = true;

    public void CreateGeometryForGrid(int[,] grid)
    {
        int halfSize = this.TileRenderSize / 2;
        for (int x = 0; x < grid.GetLength(0); ++x)
        {
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                if (!shouldIgnore(grid[x, y]))
                {
                    GameObject go = Instantiate<GameObject>(this.PlatformGeometryPrefab);
                    go.transform.parent = this.transform;
                    int posY = this.FlipVertical ? (grid.GetLength(1) - y - 1) * this.TileRenderSize : y * this.TileRenderSize;
                    go.transform.localPosition = new Vector3(x * this.TileRenderSize + halfSize, posY + halfSize, 0);
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
        }
        else
        {
            for (int i = 0; i < this.transform.childCount; ++i)
            {
                Destroy(this.transform.GetChild(i).gameObject);
            }
        }
    }

    private bool shouldIgnore(int tileType)
    {
        if (this.TileTypesToIgnore != null)
        {
            for (int i = 0; i < this.TileTypesToIgnore.Length; ++i)
            {
                if (this.TileTypesToIgnore[i] == tileType)
                    return true;
            }
        }

        return false;
    }
}
