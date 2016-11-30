using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : MonoBehaviour
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
    public ParallaxLayerController[] LayerControllers;

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

        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
    }

    public void Start()
    {
        loadParallaxForCurrentQuad();
    }

    /**
     * Private
     */
    private Dictionary<string, SCParallaxLayer[]> _parallaxData;

    private void onPause(LocalEventNotifier.Event e)
    {
        PauseEvent pauseEvent = e as PauseEvent;
        if (pauseEvent.PauseGroup == PauseController.PauseGroup.SequencedPause && pauseEvent.Tag == WorldLoadingManager.ROOM_TRANSITION_SEQUENCE)
        {
            loadParallaxForCurrentQuad();
        }
    }

    private void loadParallaxForCurrentQuad()
    {
        if (_parallaxData.ContainsKey(this.WorldManager.CurrentQuadName))
        {
            SCParallaxLayer[] layers = _parallaxData[this.WorldManager.CurrentQuadName];

            for (int i = 0; i < SCParallaxLayer.NUM_RENDER_LAYERS; ++i)
            {
                if (this.LayerControllers[i] != null)
                    this.LayerControllers[i].TransitionToNewLayer(layers[i]);
            }
        }

        //TODO: Fade previous material out and current material in, with timing that matches camera transition
    }
}
