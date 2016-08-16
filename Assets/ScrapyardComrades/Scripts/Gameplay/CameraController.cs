using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    void Awake()
    {
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }

    void Update()
    {
        if (_tracker != null)
        {
            this.transform.position = new Vector3(_tracker.transform.position.x, _tracker.transform.position.y, this.transform.position.z);
        }
    }

    /**
     * Private
     */
    private Transform _tracker;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _tracker = (e as PlayerSpawnedEvent).PlayerObject.transform;
    }
}
