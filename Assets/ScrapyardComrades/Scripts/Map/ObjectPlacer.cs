﻿using UnityEngine;

public class ObjectPlacer : VoBehavior
{
    public GameObject PlayerSpawnerPrefab;
    public GameObject EnemySpawnerPrefab;
    public GameObject HeartSpawnerPrefab;
    public int TileRenderSize = 20;
    public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(MapInfo.MapObject[] mapObjects, int gridWidth, int gridHeight, bool loadPlayer)
    {
        int positionCorrection = this.TileRenderSize / this.TileTextureSize;

        for (int i = 0; i < mapObjects.Length; ++i)
        {
            MapInfo.MapObject mapObject = mapObjects[i];
            GameObject spawner = null;

            if (mapObject.name == PLAYER)
            {
                if (loadPlayer)
                    spawner = Instantiate<GameObject>(this.PlayerSpawnerPrefab);
            }
            else if (mapObject.name.Contains(ENEMY))
            {
                spawner = Instantiate<GameObject>(this.EnemySpawnerPrefab);
            }
            else if (mapObject.name.Contains(HEART))
            {
                spawner = Instantiate<GameObject>(this.HeartSpawnerPrefab);
            }

            if (spawner != null)
            {
                spawner.transform.parent = this.transform;
                int x = (mapObject.x + mapObject.width / 2) * positionCorrection;
                int y = !this.FlipVertical ?
                    (mapObject.y + mapObject.height) * positionCorrection :
                    gridHeight * this.TileRenderSize - ((mapObject.y) * positionCorrection) + (1 * positionCorrection);
                spawner.transform.localPosition = new Vector3(x, y);
            }
        }
    }

    private const string PLAYER = "player";
    private const string ENEMY = "enemy";
    private const string HEART = "heart";
}
