using UnityEngine;

public class ObjectPlacer : VoBehavior
{
    public PooledObject PlayerSpawnerPrefab;
    public PooledObject EnemySpawnerPrefab;
    public PooledObject HeartSpawnerPrefab;
    public EnemyTracker EnemyTracker;
    public int TileRenderSize = 20;
    public int TileTextureSize = 10;
    public bool FlipVertical = true;

    public void PlaceObjects(MapInfo.MapObject[] mapObjects, int gridWidth, int gridHeight, bool loadPlayer, string quadName)
    {
        int positionCorrection = this.TileRenderSize / this.TileTextureSize;

        for (int i = 0; i < mapObjects.Length; ++i)
        {
            MapInfo.MapObject mapObject = mapObjects[i];
            PooledObject spawner = null;

            if (mapObject.name == PLAYER)
            {
                if (loadPlayer)
                    spawner = this.PlayerSpawnerPrefab.Retain();
            }
            else if (mapObject.name.Contains(ENEMY))
            {
                if (this.EnemyTracker.AttemptLoad(quadName, mapObject.name))
                {
                    //TODO - Apply enemy health remaining
                    spawner = this.EnemySpawnerPrefab.Retain();
                    Spawner spawnerComponent = spawner.GetComponent<Spawner>();
                    spawnerComponent.ClearSpawnData();
                    spawnerComponent.AddSpawnData(EnemyController.QUAD_NAME_KEY, quadName);
                    spawnerComponent.AddSpawnData(EnemyController.ENEMY_NAME_KEY, mapObject.name);
                }
            }
            else if (mapObject.name.Contains(HEART))
            {
                spawner = this.HeartSpawnerPrefab.Retain();
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
