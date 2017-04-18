using UnityEngine;
using System.Collections.Generic;

public class IndexedSpriteManager : MonoBehaviour
{
    public static IndexedSpriteManager Instance()
    {
        return _instance;
    }

    public static Texture2D GetAtlas(string path, string name)
    {
        return _instance._atlases[path + name];
    }

    public PackedSpriteGroup[] SpriteGroups;

    void Awake()
    {
        _instance = this;
        aggregateInformation();
    }

    /**
     * Private
     */
    private static IndexedSpriteManager _instance;
    private Dictionary<string, Texture2D> _atlases;

    private void aggregateInformation()
    {
        _atlases = new Dictionary<string, Texture2D>();

        for (int i = 0; i < this.SpriteGroups.Length; ++i)
        {
            for (int j = 0; j < this.SpriteGroups[i].Atlases.Length; ++j)
            {
                _atlases[this.SpriteGroups[i].AtlasNames[j]] = this.SpriteGroups[i].Atlases[j];
            }
        }
    }
}
