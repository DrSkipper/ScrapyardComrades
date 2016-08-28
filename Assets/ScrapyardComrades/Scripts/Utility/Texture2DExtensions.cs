using UnityEngine;
using System.Collections.Generic;

public static class Texture2DExtensions
{
    public static Dictionary<string, Sprite> GetSprites(this Texture2D self)
    {
        Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
        Sprite[] spriteArray = self.GetSpritesArray();

        foreach (Sprite sprite in spriteArray)
        {
            spriteDictionary[sprite.name] = sprite;
        }

        return spriteDictionary;
    }

    public static Sprite[] GetSpritesArray(this Texture2D self)
    {
        return Resources.LoadAll<Sprite>(self.name);
    }
}
