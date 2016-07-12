using UnityEngine;

[System.Serializable]
public class MapInfo
{
    public int width;
    public int height;
    public MapLayer[] layers;
    public int tilewidth;
    public int tileheight;

    [System.Serializable]
    public class MapObject
    {
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;
    }

    [System.Serializable]
    public class MapLayer
    {
        public string name;
        public int width;
        public int height;
        public int[] data;
        public MapObject[] objects;
        
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
}
