using UnityEngine;
using UnityEngine.UI;

public class MapEditorMenu : MonoBehaviour
{
    public MapEditorManager Manager;
    public GameObject TilesPanel;
    public GameObject ObjectsPanel;
    public MapEditorParallaxPanel ParallaxPanel;

    void Awake()
    {
        this.ParallaxPanel.ValidSprites = this.Manager.ParallaxSprites;
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, onResume);
    }

    /**
     * Private
     */
    private void onPause(LocalEventNotifier.Event e)
    {
        MapEditorLayer layer = this.Manager.Layers[this.Manager.CurrentLayer];
        this.TilesPanel.SetActive(layer.Type == MapEditorLayer.LayerType.Tiles);
        this.ObjectsPanel.SetActive(layer.Type == MapEditorLayer.LayerType.Objects);
        this.ParallaxPanel.gameObject.SetActive(layer.Type == MapEditorLayer.LayerType.Parallax);

        switch (layer.Type)
        {
            default:
            case MapEditorLayer.LayerType.Tiles:
                break;
            case MapEditorLayer.LayerType.Objects:
                break;
            case MapEditorLayer.LayerType.Parallax:
                this.ParallaxPanel.ShowForLayer(layer);
                break;
        }
    }

    private void onResume(LocalEventNotifier.Event e)
    {
        this.Manager.HandleReturnFromMenu();
    }
}
