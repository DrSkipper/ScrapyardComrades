﻿using UnityEngine;

public class ObjectPlacer : VoBehavior
{
    public GameObject PlayerSpawnerPrefab;
    public int TileRenderSize = 20;
    public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(MapInfo.MapObject[] mapObjects, int gridWidth, int gridHeight)
    {
        int positionCorrection = this.TileRenderSize / this.TileTextureSize;

        for (int i = 0; i < mapObjects.Length; ++i)
        {
            MapInfo.MapObject mapObject = mapObjects[i];
            if (mapObject.name == PLAYER)
            {
                GameObject spawner = Instantiate<GameObject>(this.PlayerSpawnerPrefab);
                spawner.transform.parent = this.transform;
                int x = (mapObject.x + mapObject.width / 2) * positionCorrection;
                int y = !this.FlipVertical ? 
                    (mapObject.y + mapObject.height / 2) * positionCorrection :
                    gridHeight * this.TileRenderSize - ((mapObject.y - mapObject.height / 2) * positionCorrection) + (1 * positionCorrection);
                spawner.transform.localPosition = new Vector3(x, y);
            }
        }
    }

    private const string PLAYER = "player";
}
