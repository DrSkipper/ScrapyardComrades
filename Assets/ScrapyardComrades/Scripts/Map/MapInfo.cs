using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public int width;
    public int height;
    public MapLayer[] layers;
    public int tilewidth;
    public int tileheight;
    public MapTileSet[] tilesets;

    // unused
#if UNITY_EDITOR
    public int nextobjectid;
    public string orientation;
    public string renderorder;
    public int version;
#endif

    [System.Serializable]
    public class MapObject
    {
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;

        // unused
#if UNITY_EDITOR
        public int gid;
        public int id;
        public float rotation;
        public string type;
        public bool visible;
#endif
    }

    [System.Serializable]
    public class MapLayer
    {
        public string name;
        public int width;
        public int height;
        public int[] data;
        public MapObject[] objects;

        // unused
#if UNITY_EDITOR
        public int opacity;
        public string type;
        public bool visible;
        public int x;
        public int y;
        public string draworder;
#endif

        public MapGridSpaceInfo[,] GetGrid(MapTileSet[] tilesets)
        {
            MapGridSpaceInfo[,] grid = new MapGridSpaceInfo[width, height];
            int i = 0;

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    grid[x, y] = new MapGridSpaceInfo(data[i], tilesets);
                    ++i;
                }
            }

            return grid;
        }

        public void SetGrid(MapGridSpaceInfo[,] grid)
        {
            int i = 0;
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                for (int x = 0; x < grid.GetLength(0); ++x)
                {
                    data[i] = grid[x, y].FullTileGid;
                    ++i;
                }
            }
        }
    }

    public class MapTileSet
    {
        public string name;
        public int firstgid;
        public int tilecount;
        public int tileheight;
        public int tilewidth;

        // unused
#if UNITY_EDITOR
        public int columns;
        public string image;
        public int imageheight;
        public int imagewidth;
        public int margin;
        public int spacing;
#endif
    }

    public MapLayer GetLayerWithName(string layerName)
    {
        for (int l = 0; l < layers.Length; ++l)
        {
            if (layers[l].name == layerName)
                return layers[l];
        }
        return null;
    }

    public void SetLayerWithName(string layerName, MapLayer layer)
    {
        for (int l = 0; l < layers.Length; ++l)
        {
            if (layers[l].name == layerName)
                layers[l] = layer;
        }
    }
}

public struct MapGridSpaceInfo
{
    public string ImageName;
    public int TileId;
    public int TileSetGid;
    public int FullTileGid { get { return this.TileSetGid + this.TileId - 1; } }

    public MapGridSpaceInfo(int fullTileGid, MapInfo.MapTileSet[] tilesets)
    {
        MapInfo.MapTileSet tileset = null;
        for (int i = 0; i < tilesets.Length; ++i)
        {
            if (fullTileGid >= tilesets[i].firstgid && fullTileGid < tilesets[i].firstgid + tilesets[i].tilecount)
            {
                tileset = tilesets[i];
            }
        }

        if (tileset != null)
        {
            this.ImageName = tileset.name.Replace("_tiled", "");
            this.TileId = fullTileGid - tileset.firstgid + 1;
            this.TileSetGid = tileset.firstgid;
        }
        else
        {
            this.ImageName = "invalid";
            this.TileId = 0;
            this.TileSetGid = -1;
        }
    }
}
