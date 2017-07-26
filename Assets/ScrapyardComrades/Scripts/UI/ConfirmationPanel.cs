using UnityEngine;

public class ConfirmationPanel : MonoBehaviour, MenuController
{
    public delegate void ActionDelegate(bool confirmed);
    public ActionDelegate ActionCallback;
    public MenuSelectionType SelectionType;
    public GameObject[] NavigationElements;

    [System.Serializable]
    public enum MenuSelectionType
    {
        ButtonPress,
        Navigational
    }

    void Start()
    {
        switch (this.SelectionType)
        {
            default:
            case MenuSelectionType.ButtonPress:
                _update = updateButtonPress;
                break;
            case MenuSelectionType.Navigational:
                _update = updateNavigational;
                break;
        }
    }
	
    public void Show()
    {
        if (this.SelectionType == MenuSelectionType.Navigational)
        {
            _navigationIndex = 0;
            this.NavigationElements[_navigationIndex].GetComponent<SelectionIndicator>().Run();
        }
    }

    public void Hide()
    {
        if (this.SelectionType == MenuSelectionType.Navigational)
        {
            this.NavigationElements[_navigationIndex].GetComponent<SelectionIndicator>().Stop();
        }
    }

    void Update()
    {
        _update();
    }

    /**
     * Private
     */
    private delegate void UpdateDelegate();
    private UpdateDelegate _update;
    private int _navigationIndex;

    private void updateButtonPress()
    {
        if (this.ActionCallback != null)
        {
            if (MapEditorInput.Cancel || MapEditorInput.Exit)
                this.ActionCallback(false);
            else if (MapEditorInput.Confirm)
                this.ActionCallback(true);
        }
    }

    private void updateNavigational()
    {
        if (this.ActionCallback != null && MapEditorInput.Confirm)
        {
            this.ActionCallback(true);
        }
        else
        {
            bool up = MapEditorInput.NavUp || MapEditorInput.ResizeUp;
            bool down = MapEditorInput.NavDown || MapEditorInput.ResizeDown;
            if (up || down)
            {
                this.NavigationElements[_navigationIndex].GetComponent<SelectionIndicator>().Stop();
                _navigationIndex = Mathf.Clamp(up ? _navigationIndex - 1 : _navigationIndex + 1, 0, this.NavigationElements.Length - 1);
                this.NavigationElements[_navigationIndex].GetComponent<SelectionIndicator>().Run();
            }
        }
    }
}

public interface SelectionIndicator
{
    void Run();
    void Stop();
}
