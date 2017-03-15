using System.Collections.Generic;

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

    public void AddLevelQuad(string name, int x, int y)
    {
        LevelQuad newQuad = new LevelQuad();
        newQuad.name = name;
        newQuad.x = x;
        newQuad.y = y;
        List<LevelQuad> quadList = new List<LevelQuad>(level_quads);
        quadList.Add(newQuad);
        level_quads = quadList.ToArray();
    }
}
