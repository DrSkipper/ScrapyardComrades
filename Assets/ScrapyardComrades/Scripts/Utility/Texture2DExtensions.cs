﻿using UnityEngine;
using System.Collections.Generic;

public static class Texture2DExtensions
{
    public static Dictionary<string, Sprite> GetSprites(string path, string atlasName)
    {
        Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
        Sprite[] spriteArray = GetSpritesArray(path, atlasName);

        foreach (Sprite sprite in spriteArray)
        {
            spriteDictionary[sprite.name] = sprite;
        }

        return spriteDictionary;
    }

    public static Sprite[] GetSpritesArray(string path, string atlasName)
    {
        //return Resources.LoadAll<Sprite>(path + self.name);
        return IndexedSpriteManager.GetSprites(path, atlasName);
    }

    public static Vector2[] GetUVs(this Sprite self)
    {
        float minX = (self.rect.xMin + self.textureRectOffset.x) / self.texture.width;
        float minY = (self.rect.yMin + self.textureRectOffset.y) / self.texture.height;
        float maxX = (self.rect.xMax + self.textureRectOffset.x) / self.texture.width;
        float maxY = (self.rect.yMax + self.textureRectOffset.y) / self.texture.height;
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(minX, minY);
        uvs[1] = new Vector2(maxX, minY);
        uvs[2] = new Vector2(minX, maxY);
        uvs[3] = new Vector2(maxX, maxY);
        return uvs;
    }

    public static IntegerRect GetIntegerBounds(this Sprite self)
    {
        Rect bounds = self.rect;
        return new IntegerRect((Vector2)bounds.center, (Vector2)bounds.size);
    }
}
