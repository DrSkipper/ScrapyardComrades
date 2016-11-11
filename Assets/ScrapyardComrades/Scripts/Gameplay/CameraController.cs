using UnityEngine;

public class CameraController : MonoBehaviour
{
    public IntegerRectCollider BoundsChecker;
    public WorldLoadingManager WorldManager;

    void Awake()
    {
        _halfBoundsWidth = this.BoundsChecker.Size.X / 2;
        _halfBoundsHeight = this.BoundsChecker.Size.Y / 2;
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }

    void Update()
    {
        if (_tracker != null)
        {
            // Find closest position to centering on tracker that doesn't result in our bounds leaving the current quad
            IntegerVector target = (Vector2)_tracker.transform.position;
            IntegerRect quad = this.WorldManager.CurrentQuadBoundsCheck.Bounds;
            IntegerVector destination = target;

            if (target.X > 0)
            {
                if (quad.Max.X - target.X < _halfBoundsWidth)
                    destination.X = quad.Max.X - _halfBoundsWidth;
            }
            else
            {
                if (target.X - quad.Min.X < _halfBoundsWidth)
                    destination.X = quad.Min.X + _halfBoundsWidth;
            }

            if (target.Y > 0)
            {
                if (quad.Max.Y - target.Y < _halfBoundsHeight)
                    destination.Y = quad.Max.Y - _halfBoundsHeight;
            }
            else
            {
                if (target.Y - quad.Min.Y < _halfBoundsHeight)
                    destination.Y = quad.Min.Y + _halfBoundsHeight;
            }

            this.transform.position = new Vector3(destination.X, destination.Y, this.transform.position.z);
        }
    }

    /**
     * Private
     */
    private Transform _tracker;
    private int _halfBoundsWidth;
    private int _halfBoundsHeight;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _tracker = (e as PlayerSpawnedEvent).PlayerObject.transform;
    }
}
