using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : VoBehavior
{
    [System.Serializable]
    public struct QuadParallaxData
    {
        public string QuadName;
        public LayerEntry[] Layers;
    }

    [System.Serializable]
    public struct LayerEntry
    {
        public SCParallaxLayer.RenderLayer RenderLayer;
        public SCParallaxLayer LayerData;
    }

    public WorldLoadingManager WorldManager;
    public QuadParallaxData[] ParallaxData;
    public ParallaxLayerController[] CurrentLayerControllers;
    public Transform PreviousParallaxRoot;
    public Material MatForCurrentParallax;
    public Material MatForPreviousParallax;
    public Texture2D ParallaxAtlas;
    public CameraController CameraController;

    public void Awake()
    {
        _parallaxData = new Dictionary<string, SCParallaxLayer[]>();
        for (int i = 0; i < this.ParallaxData.Length; ++i)
        {
            QuadParallaxData quadData = this.ParallaxData[i];
            SCParallaxLayer[] layers = new SCParallaxLayer[SCParallaxLayer.NUM_RENDER_LAYERS];

            for (int j = 0; j < quadData.Layers.Length; ++j)
            {
                LayerEntry layerEntry = quadData.Layers[j];

                SCParallaxLayer.RenderLayer renderLayer = layerEntry.RenderLayer != SCParallaxLayer.RenderLayer.Default ? layerEntry.RenderLayer : layerEntry.LayerData.DefaultRenderLayer;
                layers[(int)renderLayer] = layerEntry.LayerData;
            }

            _parallaxData.Add(quadData.QuadName, layers);
        }

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
    private Dictionary<string, SCParallaxLayer[]> _parallaxData;
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
        if (_parallaxData.ContainsKey(this.WorldManager.CurrentQuadName))
        {
            SCParallaxLayer[] layers = _parallaxData[this.WorldManager.CurrentQuadName];

            for (int i = 0; i < SCParallaxLayer.NUM_RENDER_LAYERS; ++i)
            {
                if (this.CurrentLayerControllers[i] != null)
                    this.CurrentLayerControllers[i].TransitionToNewLayer(layers[i], this.WorldManager.CurrentQuadWidth);
            }
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
