﻿using UnityEngine;
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
    public RegionDataCollection RegionData;
    public PixelizedFade FadeEffect;
    public float FadeTransitionGapDuration = 0.5f;
    public Easing.Function FadeTransitionFunction;
    public Easing.Flow FadeTransitionFlow;

    public bool CanBeginParallaxTransition { get { return _transitionType == TransitionType.Lerp ? true : _transitionTime >= _fadeEffectDuration; } }
    public float TotalTransitionDuration { get { return _transitionType == TransitionType.Lerp ? this.TransitionDuration : _fadeEffectDuration + this.FadeTransitionGapDuration; } }
    public TransitionType CurrentTransitionType { get {
            if (!_hasCalculatedTransitionType)
                _transitionType = calculateTransitionType();
            return _transitionType;
        } }

    public enum TransitionType
    {
        Lerp,
        Fade
    }

    void Awake()
    {
        _tracker = this.InitialTracker;
        _boundsHandler = this.WorldBoundsHandler.GetComponent<CameraBoundsHandler>();
        recalculateRendering();

        _easingDelegate = Easing.GetFunction(this.TransitionEasingFunction, this.TransitionEasingFlow);
        _fadeDelegate = Easing.GetFunction(this.FadeTransitionFunction, this.FadeTransitionFlow);
        this.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
        
        GlobalEvents.Notifier.Listen(OptionsValueChangedEvent.NAME, this, onOptionChange);
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);

        if (this.FadeEffect != null)
            _fadeEffectDuration = this.FadeEffect.Duration * Time.fixedDeltaTime;
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
                updateTowardDestination();
            }
            else
            {
                if (_transitionTime == 0.0f)
                {
                    _transitionOrigin = this.transform.position;
                    _transitionDestination = getDestination();
                }

                switch (_transitionType)
                {
                    default:
                    case TransitionType.Lerp:
                        updateLerpTransition();
                        break;
                    case TransitionType.Fade:
                        updateFadeTransition();
                        break;
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
    private Easing.EasingDelegate _fadeDelegate;
    private TransitionType _transitionType;
    private bool _hasCalculatedTransitionType;
    private float _fadeEffectDuration;

    private const int PIXELS_TO_UNITS = 2;
    private const float TRANSITION_END_BUFFER = 0.04f;
    private const float FADE_TRANSITION_BUFFER = 0.07f;
    private const float FADE_TRANSITION_MIN = 0.0001f;

    private void updateLerpTransition()
    {
        _transitionTime = Mathf.Min(_transitionTime + Time.deltaTime, this.TransitionDuration);
        lerp(_transitionTime, this.TransitionDuration, _easingDelegate);

        if (Vector2.Distance(this.transform.position, _transitionDestination) < TRANSITION_END_BUFFER)
        {
            this.transform.position = new Vector3(_transitionDestination.X, _transitionDestination.Y, this.transform.position.z);
            endTransition();
        }
    }

    private void updateFadeTransition()
    {
        _transitionTime += Time.deltaTime;

        if (this.FadeEffect.Completed)
        {
            if (_transitionTime >= _fadeEffectDuration + this.FadeTransitionGapDuration)
            {
                if (_transitionTime < _fadeEffectDuration * 1.5f + this.FadeTransitionGapDuration)
                {
                    this.transform.position = new Vector3(_transitionDestination.X, _transitionDestination.Y, this.transform.position.z);
                    this.FadeEffect.Reverse();
                }
                else
                {
                    endTransition();
                }
            }
        }

        float t = Mathf.Clamp(_transitionTime - (_fadeEffectDuration - FADE_TRANSITION_BUFFER), 0.0f, this.FadeTransitionGapDuration + FADE_TRANSITION_BUFFER * 2);
        if (t > FADE_TRANSITION_MIN)
            lerp(t, this.FadeTransitionGapDuration + FADE_TRANSITION_BUFFER * 2, _fadeDelegate);
    }

    private void lerp(float t, float d, Easing.EasingDelegate easingDelegate)
    {
        Vector2 target = _transitionOrigin.EaseTowards(_transitionDestination, t, d, easingDelegate);
        this.transform.position = new Vector3(target.x, target.y, this.transform.position.z);
    }

    private void endTransition()
    {
        _hasCalculatedTransitionType = false;
        _inTransition = false;
        _transitionTime = 0.0f;
        PauseController.EndSequence(WorldLoadingManager.ROOM_TRANSITION_SEQUENCE);
    }

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
            _transitionType = this.CurrentTransitionType;

            // Check if we're in a new region, and if so, use different transition approach
            if (_transitionType == TransitionType.Fade)
                this.FadeEffect.Reverse();

            this.CalculateBounds();
        }
    }

    private TransitionType calculateTransitionType()
    {
        _hasCalculatedTransitionType = true;
        if (this.RegionData != null && !StringExtensions.IsEmpty(_boundsHandler.PrevQuadName))
        {
            int current = this.RegionData.RegionIndexForRoom(_boundsHandler.PrevQuadName);
            if (!this.RegionData.RegionContainsRoom(current, _boundsHandler.CurrentQuadName))
            {
                return TransitionType.Fade;
            }
        }
        return TransitionType.Lerp;
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
        int screenWidth = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_WIDTH_KEY, Screen.width);
        int screenHeight = PlayerPrefs.GetInt(OptionsValues.RESOLUTION_HEIGHT_KEY, Screen.height);

        int cameraHeight = screenHeight;
        while (cameraHeight > this.ResolutionDoublingThreshold)
            cameraHeight /= 2;
        this.Camera.orthographicSize = cameraHeight;
        _attemptedHeight = cameraHeight * PIXELS_TO_UNITS;
        _attemptedWidth = Mathf.RoundToInt((float)_attemptedHeight * (float)screenWidth / (float)screenHeight);
        this.CameraViewWidth = _attemptedWidth;
        this.CameraViewHeight = _attemptedHeight;
        
        // Bounds
        this.BoundsChecker.Size.Y = _attemptedHeight;
        _attemptedHeight -= this.RoomBorder * 2;
        _attemptedWidth -= this.RoomBorder * 2;
        this.CalculateBounds();

        if (_tracker != null)
            updateTowardDestination();

        // Set up a render texture for upscaling if render resolution is lower than the screen's resolution
#if !UNITY_EDITOR
        if (this.UpscaleImage != null &&  _attemptedHeight < screenHeight)
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
                RenderTexture rt = new RenderTexture(_attemptedWidth, _attemptedHeight, 0);
                rt.filterMode = FilterMode.Point;
                this.Camera.targetTexture = rt;
                this.UpscaleImage.gameObject.SetActive(true);
                this.UpscaleImage.texture = rt;
            }
        }
        else if (this.Camera.targetTexture != null)
        {
            this.Camera.targetTexture.Release();
            this.Camera.targetTexture = null;
            this.UpscaleImage.gameObject.SetActive(false);
        }
#endif
    }

    private void updateTowardDestination()
    {
        IntegerVector destination = getDestination();
        this.transform.position = new Vector3(destination.X, destination.Y, this.transform.position.z);
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
