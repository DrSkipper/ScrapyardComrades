
[System.Serializable]
public class WorldInfo
{
    public int width;
    public int height;
    public int tile_size;
    public LevelQuad[] level_quads;

    [System.Serializable]
    public class LevelQuad
    {
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
