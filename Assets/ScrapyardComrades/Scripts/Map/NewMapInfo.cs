using UnityEngine;

[System.Serializable]
public class NewMapInfo
{
    public string name;
    public int width;
    public int height;
    public int tile_size;
    public MapLayer[] tile_layers;
    public ParallaxLayer[] parallax_layers;
    public MapObject[] props;
    public MapObject[] objects;

    [System.Serializable]
    public class MapLayer
    {
        public string name;
        public int width;
        public int height;
        public int[,] data;
        public string tileset_name;
    }

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
    public class ParallaxLayer
    {
        public string sprite_name;
        public int depth;
        public bool loops;
        public float height;
    }
}
