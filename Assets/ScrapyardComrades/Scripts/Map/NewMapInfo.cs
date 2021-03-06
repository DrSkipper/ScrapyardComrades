﻿using System.Collections.Generic;

[System.Serializable]
public class NewMapInfo
{
    public string name;
    public int width; // Dimensions specified in tiles
    public int height;
    public int tile_size;
    public List<MapLayer> tile_layers; // Use accessors to add/remove
    public List<ParallaxLayer> parallax_layers; // Use accessors to add/remove
    public List<MapObject> props; // Access directly
    public List<MapObject> props_background; // Access directly
    public List<MapObject> objects; // Access directly
    public int next_object_id;
    public int next_prop_id;
    public int next_prop_bg_id;
    public int next_light_id;

    public List<MapLight> lights;

    public NewMapInfo(string n, int w, int h, int tileSize)
    {
        name = n;
        width = w;
        height = h;
        tile_size = tileSize;
        tile_layers = new List<MapLayer>();
        parallax_layers = new List<ParallaxLayer>();
        props = new List<MapObject>();
        props_background = new List<MapObject>();
        objects = new List<MapObject>();
        next_object_id = 0;
        next_prop_id = 0;
        next_prop_bg_id = 0;
        next_light_id = 0;
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

    public void AddParallaxLayer(int depth)
    {
        parallax_layers.Add(new ParallaxLayer(depth));
    }

    public void RemoveParallaxLayer(int depth)
    {
        parallax_layers.RemoveAll(l => l.depth == depth);
    }

    public ParallaxLayer GetParallaxLayer(int depth)
    {
        return parallax_layers.Find(l => l.depth == depth);
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
        public ObjectParam[] parameters;
        public int secondary_x;
        public int secondary_y;
        //public int width;
        //public int height;
    }

    [System.Serializable]
    public struct ObjectParam
    {
        public string Name;
        public string CurrentOption;
    }

    [System.Serializable]
    public class MapLight
    {
        public string name;
        public int x;
        public int y;
        public int light_type;
        public int intensity;
        public int range;
        public int distance;
        public int spot_angle;
        public float rot_x;
        public float rot_y;
        public float r;
        public float g;
        public float b;
        public bool affects_foreground;
        public bool[] affects_parallax;
        public float parallax_ratio;

        public bool AffectsParallax(int parallaxLayerIndex)
        {
            if (affects_parallax == null || affects_parallax.Length <= parallaxLayerIndex)
                return false;
            return affects_parallax[parallaxLayerIndex];
        }

        public void SetAffectsParallax(int parallaxLayerIndex, bool value, int numParallaxLayers)
        {
            if (affects_parallax == null)
                affects_parallax = new bool[numParallaxLayers];
            else if (affects_parallax.Length < numParallaxLayers)
            {
                bool[] newArray = new bool[numParallaxLayers];
                for (int i = 0; i < numParallaxLayers; ++i)
                {
                    newArray[i] = i < affects_parallax.Length ? affects_parallax[i] : false;
                }
                affects_parallax = newArray;
            }

            affects_parallax[parallaxLayerIndex] = value;
        }
    }

    [System.Serializable]
    public class ParallaxLayer
    {
        public string sprite_name;
        public int depth;
        public bool loops;
        public bool lit;
        public float height;
        public float x_position;
        public float parallax_ratio;
        public string layer_name;
        public const string DEFAULT_LAYER = "ParallaxFront";

        public ParallaxLayer(int d)
        {
            sprite_name = null;
            depth = d;
            loops = false;
            lit = false;
            height = 0.5f;
            x_position = 0.5f;
            parallax_ratio = 0.2f;
            layer_name = DEFAULT_LAYER;
        }

        public string GetLayerName()
        {
            if (layer_name == null || layer_name == "")
                return DEFAULT_LAYER;
            return layer_name;
        }
    }
}
