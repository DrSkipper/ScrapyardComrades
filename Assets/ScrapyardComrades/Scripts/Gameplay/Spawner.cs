using UnityEngine;

[RequireComponent(typeof(TimedCallbacks))]
public class Spawner : VoBehavior
{
    public GameObject ObjectToSpawn;
    public Transform SpawnLocation;
    public float Delay = 0.5f;
    public bool SpawnOnStart = true;

    void Start()
    {
        if (this.SpawnOnStart)
            this.BeginSpawn();
    }

    public void BeginSpawn()
    {
        this.GetComponent<TimedCallbacks>().AddCallback(this, spawn, this.Delay);
    }

    private void spawn()
    {
        GameObject spawn = Instantiate<GameObject>(this.ObjectToSpawn);
        spawn.transform.position = this.SpawnLocation.position;
    }
}
