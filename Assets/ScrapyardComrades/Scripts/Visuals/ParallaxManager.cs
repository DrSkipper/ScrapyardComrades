using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : VoBehavior
{
    public WorldLoadingManager WorldManager;
    public ParallaxLayerController[] CurrentLayerControllers;
    public Transform PreviousParallaxRoot;
    public Texture2D ParallaxAtlas;
    public CameraController CameraController;

    public static ParallaxManager Instance { get { return _instance; } }

    public void Awake()
    {
        _instance = this;
        _objects = new Dictionary<int, List<ObjectEntry>>();
        _origins = new Dictionary<int, IntegerVector>();
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onRecenter);
    }

    void Start()
    {
        loadParallaxForCurrentQuad(false);
    }

    void Update()
    {
        if (_inTransition)
        {
            _transitionTime += Time.deltaTime;

            float currentAlpha = 1.0f;
            float previousAlpha = 0.0f;

            if (_transitionTime >= this.CameraController.TransitionDuration)
            {
                _inTransition = false;
                this.PreviousParallaxRoot.position = this.transform.position;
            }
            else
            {
                currentAlpha = Easing.QuadEaseInOut(_transitionTime, 0.0f, 1.0f, this.CameraController.TransitionDuration);
                previousAlpha = Easing.QuadEaseInOut(_transitionTime, 1.0f, -1.0f, this.CameraController.TransitionDuration);
            }

            setTransitionAlphas(currentAlpha, previousAlpha);
        }
    }

    void FixedUpdate()
    {
        foreach (int key in _objects.Keys)
        {
            float ratio = parallaxKeyToRatio(key);
            List<ObjectEntry> objects = _objects[key];
            IntegerVector origin = _origins[key];

            IntegerVector trackerPos = ((IntegerVector)(Vector2)this.CameraController.transform.position) - origin;
            Vector2 trackerNormalized = ((Vector2)trackerPos).normalized;
            float magnitude = ((Vector2)trackerPos).magnitude * ratio;
            IntegerVector final = ((IntegerVector)(trackerNormalized * magnitude)) + origin;
            
            float t = _inTransition ? Easing.QuadEaseInOut(_transitionTime, 0.0f, 1.0f, this.CameraController.TransitionDuration) : 0.0f;

            for (int i = 0; i < objects.Count; ++i)
            {
                IntegerVector target = final + objects[i].RelativeOrigin;

                if (_inTransition)
                {
                    target = Vector2.Lerp(objects[i].Transform.position, target, t);
                }

                objects[i].Transform.SetLocalPosition2D(target.X, target.Y);
            }
        }
    }

    public void AddObject(SCParallaxObject parallaxObject)
    {
        int parallaxKey = parallaxRatioToKey(parallaxObject.ParallaxRatio);
        if (!_objects.ContainsKey(parallaxKey))
            addParallaxKey(parallaxKey);
        _objects[parallaxKey].Add(new ObjectEntry(parallaxObject));
    }

    public void RemoveObject(SCParallaxObject parallaxObject)
    {
        int parallaxKey = parallaxRatioToKey(parallaxObject.ParallaxRatio);
        if (_objects.ContainsKey(parallaxKey))
        {
            List<ObjectEntry> objects = _objects[parallaxKey];
            for (int i = 0; i < objects.Count; ++i)
            {
                if (objects[i].Transform == parallaxObject.transform)
                {
                    objects.RemoveAt(i);
                    break;
                }
            }
        }
    }

    /**
     * Private
     */
    private static ParallaxManager _instance;
    private bool _inTransition;
    private float _transitionTime;
    private Dictionary<int, List<ObjectEntry>> _objects;
    private Dictionary<int, IntegerVector> _origins;

    private struct ObjectEntry
    {
        public Transform Transform;
        public IntegerVector RelativeOrigin;

        public ObjectEntry(SCParallaxObject parallaxObject)
        {
            this.Transform = parallaxObject.transform;
            this.RelativeOrigin = (Vector2)this.Transform.position;
        }
    }

    private static int parallaxRatioToKey(float parallaxRatio)
    {
        return Mathf.RoundToInt(parallaxRatio * 100);
    }

    private static float parallaxKeyToRatio(int parallaxKey)
    {
        return parallaxKey / 100.0f;
    }

    private void addParallaxKey(int key)
    {
        _objects.Add(key, new List<ObjectEntry>());
        _origins.Add(key, IntegerVector.Zero);
    }

    private void onPause(LocalEventNotifier.Event e)
    {
        PauseEvent pauseEvent = e as PauseEvent;
        if (pauseEvent.PauseGroup == PauseController.PauseGroup.SequencedPause && pauseEvent.Tag == WorldLoadingManager.ROOM_TRANSITION_SEQUENCE)
        {
            loadParallaxForCurrentQuad(true);
        }
    }

    private void onRecenter(LocalEventNotifier.Event e)
    {
        IntegerVector offset = (e as WorldRecenterEvent).RecenterOffset;
        foreach (int key in _objects.Keys)
        {
            List<ObjectEntry> objects = _objects[key];
            for (int i = 0; i < objects.Count; ++i)
            {
                ObjectEntry entry = objects[i];
                entry.RelativeOrigin += offset;
                IntegerVector newPos = (IntegerVector)(Vector2)entry.Transform.position + offset;
                entry.Transform.SetLocalPosition2D(newPos.X, newPos.Y);
                objects[i] = entry;
            }
        }
    }

    private void loadParallaxForCurrentQuad(bool transition)
    {
        NewMapInfo currentQuadInfo = this.WorldManager.CurrentMapInfo;
        for (int i = 0; i < this.CurrentLayerControllers.Length; ++i)
        {
            if (i < currentQuadInfo.parallax_layers.Count)
                this.CurrentLayerControllers[i].TransitionToNewLayer(currentQuadInfo.parallax_layers[i], this.WorldManager.CurrentQuadWidth);
            else
                this.CurrentLayerControllers[i].TransitionToNewLayer(null, 0);
        }

        if (transition)
        {
            // Fade previous material out and current material in, with timing that matches camera transition
            _inTransition = true;
            _transitionTime = 0.0f;
            setTransitionAlphas(0.0f, 1.0f);
        }
    }

    private void setTransitionAlphas(float currentAlpha, float previousAlpha)
    {
        for (int i = 0; i < this.CurrentLayerControllers.Length; ++i)
        {
            ParallaxLayerController current = this.CurrentLayerControllers[i];
            Material currentMat = current.CurrentLayerVisual.MeshRenderer.material;
            Material previousMat = current.PreviousLayerVisual.MeshRenderer.material;

            Color color = currentMat.color;
            color.a = currentAlpha;
            currentMat.color = color;
            color = previousMat.color;
            color.a = previousAlpha;
            previousMat.color = color;
        }
    }
}
