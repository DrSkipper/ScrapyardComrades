using UnityEngine;

public class CameraController : MonoBehaviour, IPausable
{
    public IntegerRectCollider BoundsChecker; // Y value should be set to match vision of screen height in world at start. X will be driven by screen width to height ratio.
    public WorldLoadingManager WorldManager;
    public int RoomBorder = 10; // Modifies BoundsChecker size to give small visual overlap between rooms
    public float TransitionSpeed = 1.0f;

    void Awake()
    {
        int height = this.BoundsChecker.Size.Y;
        int width = Mathf.RoundToInt((float)height * (float)Screen.width / (float)Screen.height);
        height -= this.RoomBorder;
        width -= this.RoomBorder;

        this.BoundsChecker.Size = new IntegerVector(width, height);
        _halfBoundsWidth = width / 2;
        _halfBoundsHeight = height / 2;

        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
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

            if (!_inTransition)
            {
                this.transform.position = new Vector3(destination.X, destination.Y, this.transform.position.z);
            }
            else
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(destination.X, destination.Y, this.transform.position.z), this.TransitionSpeed);
                if (Vector2.Distance(this.transform.position, destination) < 0.1f)
                {
                    this.transform.position = new Vector3(destination.X, destination.Y, this.transform.position.z);
                    _inTransition = false;
                    PauseController.EndSequence(WorldLoadingManager.ROOM_TRANSITION_SEQUENCE);
                }
            }
        }
    }

    /**
     * Private
     */
    private Transform _tracker;
    private int _halfBoundsWidth;
    private int _halfBoundsHeight;
    private bool _inTransition;

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _tracker = (e as PlayerSpawnedEvent).PlayerObject.transform;
    }

    private void onPause(LocalEventNotifier.Event e)
    {
        PauseEvent pauseEvent = e as PauseEvent;
        if (pauseEvent.PauseGroup == PauseController.PauseGroup.SequencedPause &&
            pauseEvent.Tag == WorldLoadingManager.ROOM_TRANSITION_SEQUENCE)
        {
            _inTransition = true;
        }
    }
}
