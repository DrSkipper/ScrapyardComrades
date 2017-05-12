using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    public static bool IsAlive { get { return _instance._playerAlive; } }
    public static GameObject GameObject { get { return _instance._playerObject; } }
    public static Transform Transform { get { return _instance._playerTransform; } }
    public static IntegerCollider Collider { get { return _instance._playerCollider; } }

    void Awake()
    {
        _instance = this;
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
        GlobalEvents.Notifier.Listen(PlayerDiedEvent.NAME, this, playerDied);
    }

    private static PlayerReference _instance;
    private bool _playerAlive;
    private GameObject _playerObject;
    private Transform _playerTransform;
    private IntegerCollider _playerCollider;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _playerAlive = true;
        _playerObject = (e as PlayerSpawnedEvent).PlayerObject;
        _playerTransform = _playerObject.transform;
        _playerCollider = _playerObject.GetComponent<IntegerCollider>();
    }

    private void playerDied(LocalEventNotifier.Event e)
    {
        _playerAlive = false;
    }
}
