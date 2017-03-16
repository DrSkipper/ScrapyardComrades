using System.Collections.Generic;

[System.Serializable]
public class WorldInfo
{
    public int width; // Dimensions specified in world grid spaces
    public int height;
    public int tile_size;
    public LevelQuad[] level_quads;

    [System.Serializable]
    public class LevelQuad
    {
        public string name;
        public int x;
        public int y;
        public int width; // Dimensions specified in world grid spaces
        public int height;
    }

    public void AddLevelQuad(string name, int x, int y, int w, int h)
    {
        LevelQuad newQuad = new LevelQuad();
        newQuad.name = name;
        newQuad.x = x;
        newQuad.y = y;
        newQuad.width = w;
        newQuad.height = h;
        List<LevelQuad> quadList = new List<LevelQuad>(level_quads);
        quadList.Add(newQuad);
        level_quads = quadList.ToArray();
    }

    public void RemoveLevelQuad(string name)
    {
        List<LevelQuad> quadList = new List<LevelQuad>(level_quads);
        quadList.RemoveAll(q => q.name == name);
        level_quads = quadList.ToArray();
    }

    public LevelQuad GetLevelQuad(string name)
    {
        for (int i = 0; i < level_quads.Length; ++i)
        {
            if (level_quads[i].name == name)
                return level_quads[i];
        }
        return null;
    }
}
