using UnityEngine;

[RequireComponent(typeof(TimedCallbacks))]
public class Spawner : VoBehavior, IPausable
{
    public GameObject ObjectToSpawn;
    public Transform SpawnLocation;
    public float Delay = 0.5f;
    public bool SpawnOnStart = true;
    public bool DestroyAfterSpawn = true;

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
        IntegerCollider collider = spawn.GetComponent<IntegerCollider>();
        int yOffset = collider != null ? collider.Bounds.Size.Y / 2 : 0;

        spawn.transform.position = new Vector3(this.SpawnLocation.position.x, this.SpawnLocation.position.y + yOffset, this.SpawnLocation.position.z);
        if (this.DestroyAfterSpawn)
            Destroy(this.gameObject);
    }
}
