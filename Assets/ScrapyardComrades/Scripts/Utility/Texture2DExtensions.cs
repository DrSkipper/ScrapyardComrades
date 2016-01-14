using UnityEngine;
using System.Collections.Generic;

public static class Texture2DExtensions
{
    public static Dictionary<string, Sprite> GetSprites(this Texture2D self)
    {
        Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
        Sprite[] spriteArray = Resources.LoadAll<Sprite>(self.name);

        foreach (Sprite sprite in spriteArray)
        {
            spriteDictionary[sprite.name] = sprite;
        }

        return spriteDictionary;
    }
}
