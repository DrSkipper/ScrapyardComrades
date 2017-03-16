using System.Collections.Generic;

[System.Serializable]
public class NewMapInfo
{
    public string name;
    public int width; // Dimensions specified in tiles
    public int height;
    public int tile_size;
    public List<MapLayer> tile_layers; // Use accessors to add/remove
    public List<ParallaxLayer> parallax_layers; // Access directly
    public List<MapObject> props; // Access directly
    public List<MapObject> objects; // Access directly
    public int next_object_id;
    public int next_prop_id;

    public NewMapInfo(string n, int w, int h, int tileSize)
    {
        name = n;
        width = w;
        height = h;
        tile_size = tileSize;
        tile_layers = new List<MapLayer>();
        parallax_layers = new List<ParallaxLayer>();
        props = new List<MapObject>();
        objects = new List<MapObject>();
        next_object_id = 0;
        next_prop_id = 0;
    }

    public void AddTileLayer(string layerName)
    {
        tile_layers.Add(new MapLayer(layerName, width, height));
    }

    public void RemoveTileLayer(string layerName)
    {
        tile_layers.RemoveAll(l => l.name == layerName);
    }

    public MapLayer GetMapLayer(string layerName)
    {
        return tile_layers.Find(l => l.name == layerName);
    }

    public void ResizeWidth(int newWidth)
    {
        tile_layers.ForEach(x => x.ResizeWidth(newWidth));
        if (newWidth < width)
        {
            props.RemoveAll(p => p.x >= newWidth);
            objects.RemoveAll(o => o.x >= newWidth);
        }
        width = newWidth;
    }

    public void ResizeHeight(int newHeight)
    {
        tile_layers.ForEach(x => x.ResizeHeight(newHeight));
        if (newHeight < height)
        {
            props.RemoveAll(p => p.y >= newHeight);
            objects.RemoveAll(o => o.y >= newHeight);
        }
        height = newHeight;
    }

    [System.Serializable]
    public class MapLayer
    {
        public string name;
        public int width; // Dimensions specified in tiles
        public int height;
        public string tileset_name;
        public MapTile[] data;

        public MapLayer(string n, int w, int h)
        {
            name = n;
            width = w;
            height = h;
            data = new MapTile[w * h];
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    data[y * width + x] = new MapTile();
                }
            }
        }

        public MapTile[,] GetDataGrid()
        {
            MapTile[,] grid = new MapTile[width, height];
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    grid[x, y] = data[y * width + x];
                }
            }
            return grid;
        }

        public void SetDataGrid(MapTile[,] grid)
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    data[y * width + x] = grid[x, y];
                }
            }
        }

        public void SetTile(int x, int y, string spriteName)
        {
            data[y * width + x].sprite_name = spriteName;
        }

        public void ResizeWidth(int newWidth)
        {
            MapTile[] newData = new MapTile[newWidth * height];
            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    newData[y * newWidth + x] = x >= width ? new MapTile() : data[y * width + x];
                }
            }
            width = newWidth;
            data = newData;
        }

        public void ResizeHeight(int newHeight)
        {
            MapTile[] newData = new MapTile[width * newHeight];
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {
                    newData[y * width + x] = y >= height ? new MapTile() : data[y * width + x];
                }
            }
            height = newHeight;
            data = newData;
        }
    }

    [System.Serializable]
    public class MapTile
    {
        public bool is_filled;
        public string sprite_name;
        public const string EMPTY_TILE_SPRITE_NAME = "empty";

        public MapTile()
        {
            is_filled = false;
            sprite_name = EMPTY_TILE_SPRITE_NAME;
        }
    }

    [System.Serializable]
    public class MapObject
    {
        public string name;
        public string prefab_name;
        public int x; // Position specified in pixel coordinates
        public int y;
        public int z;
        //public int width;
        //public int height;
    }

    [System.Serializable]
    public class ParallaxLayer
    {
        public string sprite_name;
        public int depth;
        public bool loops;
        public float height;
    }
}
