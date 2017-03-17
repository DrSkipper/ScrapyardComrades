using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : VoBehavior
{
    public WorldLoadingManager WorldManager;
    public ParallaxLayerController[] CurrentLayerControllers;
    public Transform PreviousParallaxRoot;
    public Material MatForCurrentParallax;
    public Material MatForPreviousParallax;
    public Texture2D ParallaxAtlas;
    public CameraController CameraController;

    public void Awake()
    {
        this.MatForCurrentParallax.mainTexture = this.ParallaxAtlas;
        this.MatForPreviousParallax.mainTexture = this.ParallaxAtlas;

        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
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

            Color color = this.MatForCurrentParallax.color;
            color.a = currentAlpha;
            this.MatForCurrentParallax.color = color;
            color = this.MatForPreviousParallax.color;
            color.a = previousAlpha;
            this.MatForPreviousParallax.color = color;
        }
    }

    /**
     * Private
     */
    private bool _inTransition;
    private float _transitionTime;

    private void onPause(LocalEventNotifier.Event e)
    {
        PauseEvent pauseEvent = e as PauseEvent;
        if (pauseEvent.PauseGroup == PauseController.PauseGroup.SequencedPause && pauseEvent.Tag == WorldLoadingManager.ROOM_TRANSITION_SEQUENCE)
        {
            loadParallaxForCurrentQuad(true);
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
            Color color = this.MatForCurrentParallax.color;
            color.a = 0.0f;
            this.MatForCurrentParallax.color = color;
            color = this.MatForPreviousParallax.color;
            color.a = 1.0f;
            this.MatForPreviousParallax.color = color;
        }
    }
}
