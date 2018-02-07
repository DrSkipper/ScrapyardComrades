using UnityEngine;
using UnityEngine.UI;

public class CameraController : VoBehavior, IPausable
{
    public Camera Camera;
    public IntegerRectCollider BoundsChecker;
    public GameObject WorldBoundsHandler;
    public Transform InitialTracker;
    public int RoomBorder = 10; // Modifies BoundsChecker size to give small visual overlap between rooms
    public Easing.Function TransitionEasingFunction;
    public Easing.Flow TransitionEasingFlow;
    public float TransitionDuration = 1.0f;
    public int CameraViewWidth { get; private set; }
    public int CameraViewHeight { get; private set; }
    public int ResolutionDoublingThreshold = 270;

    public RawImage UpscaleImage;

    void Awake()
    {
        _tracker = this.InitialTracker;
        _boundsHandler = this.WorldBoundsHandler.GetComponent<CameraBoundsHandler>();
        recalculateRendering();

        GlobalEvents.Notifier.Listen(OptionsValueChangedEvent.NAME, this, onOptionChange);

        _easingDelegate = Easing.GetFunction(this.TransitionEasingFunction, this.TransitionEasingFlow);
        this.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }

    void Start()
    {
        this.BoundsChecker.Size.Y = _attemptedHeight;
        _attemptedHeight -= this.RoomBorder * 2;
        _attemptedWidth -= this.RoomBorder * 2;
        this.CalculateBounds();

        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
    }

    public void CalculateBounds()
    {
        int width = _attemptedWidth;
        int height = _attemptedHeight;

        // Make sure our camera target bounds don't go outside the quad bounds
        if (width > _boundsHandler.GetBounds().Size.X)
            width = _boundsHandler.GetBounds().Size.X;
        if (height > _boundsHandler.GetBounds().Size.Y)
            height = _boundsHandler.GetBounds().Size.Y;

        this.BoundsChecker.Size = new IntegerVector(width, height);
        _halfBoundsWidth = width / 2;
        _halfBoundsHeight = height / 2;
    }

    void Update()
    {
        if (_tracker != null)
        {
            if (!_inTransition)
            {
                IntegerVector destination = getDestination();
                this.transform.position = new Vector3(destination.X, destination.Y, this.transform.position.z);
            }
            else
            {
                if (_transitionTime == 0.0f)
                {
                    _transitionOrigin = this.transform.position;
                    _transitionDestination = getDestination();
                }

                _transitionTime = Mathf.Min(_transitionTime + Time.deltaTime, this.TransitionDuration);
                Vector2 target = _transitionOrigin.EaseTowards(_transitionDestination, _transitionTime, this.TransitionDuration, _easingDelegate);
                this.transform.position = new Vector3(target.x, target.y, this.transform.position.z);

                if (Vector2.Distance(this.transform.position, _transitionDestination) < TRANSITION_END_BUFFER)
                {
                    this.transform.position = new Vector3(_transitionDestination.X, _transitionDestination.Y, this.transform.position.z);
                    _inTransition = false;
                    _transitionTime = 0.0f;
                    PauseController.EndSequence(WorldLoadingManager.ROOM_TRANSITION_SEQUENCE);
                }
            }
        }
    }

    public void SetTracker(Transform newTracker)
    {
        _tracker = newTracker;
    }

    /**
     * Private
     */
    private CameraBoundsHandler _boundsHandler;
    private Transform _tracker;
    private int _halfBoundsWidth;
    private int _halfBoundsHeight;
    private int _attemptedWidth;
    private int _attemptedHeight;
    private bool _inTransition;
    private float _transitionTime;
    private IntegerVector _transitionDestination;
    private Vector2 _transitionOrigin;
    private Easing.EasingDelegate _easingDelegate;
    private const int PIXELS_TO_UNITS = 2;
    private const float TRANSITION_END_BUFFER = 0.04f;

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
            _transitionTime = 0.0f;
            this.CalculateBounds();
        }
    }

    private void onOptionChange(LocalEventNotifier.Event e)
    {
        string optionName = (e as OptionsValueChangedEvent).OptionName;
        if (optionName == OptionsValues.FULLSCREEN_KEY || 
            optionName == OptionsValues.RESOLUTION_KEY)
            recalculateRendering();
    }

    private void recalculateRendering()
    {
        int cameraHeight = Screen.height;
        while (cameraHeight > this.ResolutionDoublingThreshold)
            cameraHeight /= 2;
        Camera.orthographicSize = cameraHeight;
        _attemptedHeight = cameraHeight * PIXELS_TO_UNITS;
        _attemptedWidth = Mathf.RoundToInt((float)_attemptedHeight * (float)Screen.width / (float)Screen.height);
        this.CameraViewWidth = _attemptedWidth;
        this.CameraViewHeight = _attemptedHeight;

        // Set up a render texture for upscaling if render resolution is lower than the screen's resolution
#if !UNITY_EDITOR
        if (_attemptedHeight < Screen.height)
        {
            bool needRefresh = true;

            if (this.Camera.targetTexture != null)
            {
                if (this.Camera.targetTexture.width == _attemptedWidth && this.Camera.targetTexture.height == _attemptedHeight)
                    needRefresh = false;
                else
                    this.Camera.targetTexture.Release();
            }

            if (needRefresh)
            {
                RenderTexture rt = new RenderTexture(_attemptedWidth, _attemptedHeight, 16);
                this.Camera.targetTexture = rt;
                this.UpscaleImage.gameObject.SetActive(true);
                this.UpscaleImage.texture = rt;
            }
        }
        else if (this.Camera.targetTexture != null)
        {
            this.Camera.targetTexture.Release();
            this.Camera.targetTexture = null;
        }
#endif
    }

    private IntegerVector getDestination()
    {
        // Find closest position to centering on tracker that doesn't result in our bounds leaving the current quad
        IntegerVector target = (Vector2)_tracker.transform.position;
        IntegerRect quad = _boundsHandler.GetBounds().Bounds;
        IntegerVector destination = target;

        if (target.X > quad.Center.X)
        {
            if (quad.Max.X - target.X < _halfBoundsWidth)
                destination.X = quad.Max.X - _halfBoundsWidth;
        }
        else
        {
            if (target.X - quad.Min.X < _halfBoundsWidth)
                destination.X = quad.Min.X + _halfBoundsWidth;
        }

        if (target.Y > quad.Center.Y)
        {
            if (quad.Max.Y - target.Y < _halfBoundsHeight)
                destination.Y = quad.Max.Y - _halfBoundsHeight;
        }
        else
        {
            if (target.Y - quad.Min.Y < _halfBoundsHeight)
                destination.Y = quad.Min.Y + _halfBoundsHeight;
        }
        return destination;
    }
}
