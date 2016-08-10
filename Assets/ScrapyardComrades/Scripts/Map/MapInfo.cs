using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public int width;
    public int height;
    public MapLayer[] layers;
    public int tilewidth;
    public int tileheight;

    // unused
    public int nextobjectid;
    public string orientation;
    public string renderorder;
    public object tilesets;
    public int version;

    [System.Serializable]
    public class MapObject
    {
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;

        // unused
        public int gid;
        public int id;
        public float rotation;
        public string type;
        public bool visible;
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
        public int opacity;
        public string type;
        public bool visible;
        public int x;
        public int y;
        public string draworder;
        
        public int[,] Grid
        {
            get
            {
                int[,] grid = new int[width, height];
                int i = 0;

                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        grid[x, y] = data[i];
                        ++i;
                    }
                }

                return grid;
            }

            set
            {
                int i = 0;
                for (int y = 0; y < value.GetLength(1); ++y)
                {
                    for (int x = 0; x < value.GetLength(0); ++x)
                    {
                        data[i] = value[x, y];
                        ++i;
                    }
                }
            }
        }
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
