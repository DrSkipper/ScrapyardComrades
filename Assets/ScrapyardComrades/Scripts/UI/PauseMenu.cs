using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject PausePanel;
    public PauseController.PauseGroup PauseGroup;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, onPause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, onResume);
    }

    /**
     * Private
     */
    private void onPause(LocalEventNotifier.Event e)
    {
        if ((e as PauseEvent).PauseGroup == this.PauseGroup)
        {
            this.PausePanel.SetActive(true);
            MenuController controller = this.PausePanel.GetComponent<MenuController>();
            if (controller != null)
                controller.Show();
        }
    }

    public void onResume(LocalEventNotifier.Event e)
    {
        if ((e as ResumeEvent).PauseGroup == this.PauseGroup)
        {
            this.PausePanel.SetActive(false);
            MenuController controller = this.PausePanel.GetComponent<MenuController>();
            if (controller != null)
                controller.Hide();
        }
    }
}

public interface MenuController
{
    void Show();
    void Hide();
}
