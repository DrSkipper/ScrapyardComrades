using UnityEngine;
using UnityEngine.UI;

public class MapEditorMenu : MonoBehaviour
{
    public MapEditorManager Manager;
    public MapEditorTilesPanel TilesPanel;
    public MapEditorObjectsPanel ObjectsPanel;
    public MapEditorParallaxPanel ParallaxPanel;
    public MapEditorLightingPanel LightingPanel;

    void Start()
    {
        this.ParallaxPanel.ValidSprites = this.Manager.ParallaxSprites;
        this.LightingPanel.ValidLightColors = this.Manager.ValidLightColors;
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, onResume);
    }

    /**
     * Private
     */
    private void onPause(LocalEventNotifier.Event e)
    {
        MapEditorLayer layer = this.Manager.Layers[this.Manager.CurrentLayer];
        this.TilesPanel.gameObject.SetActive(layer.Type == MapEditorLayer.LayerType.Tiles);
        this.ObjectsPanel.gameObject.SetActive(layer.Type == MapEditorLayer.LayerType.Objects);
        this.ParallaxPanel.gameObject.SetActive(layer.Type == MapEditorLayer.LayerType.Parallax);
        this.LightingPanel.gameObject.SetActive(layer.Type == MapEditorLayer.LayerType.Lighting);
        switch (layer.Type)
        {
            default:
            case MapEditorLayer.LayerType.Objects:
                this.ObjectsPanel.ShowForLayer(layer);
                break;
            case MapEditorLayer.LayerType.Tiles:
                this.TilesPanel.ShowForLayer(layer);
                break;
            case MapEditorLayer.LayerType.Parallax:
                this.ParallaxPanel.ShowForLayer(layer);
                break;
            case MapEditorLayer.LayerType.Lighting:
                this.LightingPanel.ShowForLayer(layer);
                break;
        }
    }

    private void onResume(LocalEventNotifier.Event e)
    {
        this.Manager.HandleReturnFromMenu();
    }
}
