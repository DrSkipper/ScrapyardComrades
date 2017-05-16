using UnityEngine;

public class TrainSpawner : VoBehavior, IPausable
{
    public Transform SpawnLocation;
    public IntegerVector SpawnDelayRange;
    public PooledObject TrainPrefab;

    void OnSpawn()
    {
        if (this.spriteRenderer != null)
            this.spriteRenderer.enabled = false;

        if (_spawnTimer == null)
        {
            _spawnTimer = new Timer(this.SpawnDelayRange.Y, false, true, spawnTrain);
        }
        else
        {
            _spawnTimer.reset(this.SpawnDelayRange.Y);
            _spawnTimer.start();
        }
    }

    void FixedUpdate()
    {
        _spawnTimer.update();
    }

    /**
     * Private
     */
    private Timer _spawnTimer;

    private void spawnTrain()
    {
        PooledObject train = this.TrainPrefab.Retain();
        train.transform.position = this.SpawnLocation.position;
        train.GetComponent<SubwayTrain>().OnReachedEndCallback = trainReachedEnd;
        train.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }

    private void trainReachedEnd()
    {
        _spawnTimer.reset(Random.Range(this.SpawnDelayRange.X, this.SpawnDelayRange.Y));
        _spawnTimer.start();
    }
}
