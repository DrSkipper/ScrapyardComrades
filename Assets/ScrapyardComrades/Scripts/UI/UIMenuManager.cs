using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIMenuManager : MonoBehaviour
{
    public Menu[] MenuStates;
    public Transform EmptyState;
    public PositionLerper MenuBounds;
    public UIMenuElementSpec[] ElementVisuals;
    public PositionLerper HighlightIndicator;
    public string InitialStateName;
    public int SceneTransitionDelay = 50;
    public int EventSendDelay = 25;
    public string EndSceneEvent = "SCENE_END";
    public Menu.Action CancelAction;
    public MenuOrientation Orientation = MenuOrientation.Vertical;

    [System.Serializable]
    public enum MenuOrientation
    {
        Vertical,
        Horizontal
    }

    public void Initialize()
    {
        MenuInput.EnableMenuInput();
        if (_pastMenuStack == null)
            _pastMenuStack = new List<string>();
        else
            _pastMenuStack.Clear();
        configureForState(this.InitialStateName);
        _initialized = true;
    }

    public void Hide()
    {
        MenuInput.DisableMenuInput();
        _initialized = false;
        configureForEmptyState();
    }

    void FixedUpdate()
    {
        if (_actionTimer != null)
        {
            _actionTimer.update();
        }
        else if (_initialized && ! this.MenuBounds.Running)
        {
            if (next())
            {
                _currentMenu.HighlightNext();
                highlight();
            }
            else if (prev())
            {
                _currentMenu.HighlightPrev();
                highlight();
            }
            else if (MenuInput.Confirm)
            {
                Menu.Action action = _currentMenu.SelectCurrent();
                if (action.Type == Menu.ActionType.Custom)
                    handleAction(this.ElementVisuals[_currentHighlight].HandleCustomAction(action));
                else
                    handleAction(action);
            }
            else if (MenuInput.Cancel)
            {
                if (_pastMenuStack.Count > 0)
                    configureForState(_pastMenuStack.Pop());
                else
                    handleAction(this.CancelAction);
            }
            else if (_currentMenu.CurrentElementAllowsCycling())
            {
                if (cycleNext())
                    cycleCurrent(1);
                else if (cyclePrev())
                    cycleCurrent(-1);
            }
        }
    }

    /**
     * Private
     */
    private bool _initialized;
    private Menu _currentMenu;
    private List<string> _pastMenuStack;
    private int _currentHighlight;
    private Timer _actionTimer;
    private string _timerParam;
    private bool _changingScene;

    private void configureForState(string state)
    {
        Menu menu = menuByStateName(state);

        for (int i = 0; i < this.ElementVisuals.Length; ++i)
        {
            if (i < menu.NumElements)
            {
                this.ElementVisuals[i].gameObject.SetActive(true);
                this.ElementVisuals[i].Configure(menu, menu.Elements[i]);
            }
            else
            {
                this.ElementVisuals[i].gameObject.SetActive(false);
            }
        }

        this.MenuBounds.Destination = menu.transform;
        this.MenuBounds.BeginLerp();
        _currentMenu = menu;
        highlight();
    }

    private void refreshState()
    {
        this.ElementVisuals[_currentMenu.HighlightedElement].UnHighlight();
        for (int i = 0; i < this.ElementVisuals.Length && i < _currentMenu.NumElements; ++i)
        {
            this.ElementVisuals[i].Configure(_currentMenu, _currentMenu.Elements[i]);
        }
        this.ElementVisuals[_currentMenu.HighlightedElement].Highlight();
    }

    private void configureForEmptyState()
    {
        this.MenuBounds.Destination = this.EmptyState;
        this.MenuBounds.BeginLerp();
    }

    private void cycleCurrent(int dir)
    {
        Menu.Action action = _currentMenu.SelectCurrent();
        if (action.Type == Menu.ActionType.Custom)
            handleAction(this.ElementVisuals[_currentHighlight].HandleCustomAction(action, dir), dir);
        else
            handleAction(action, dir);
    }

    private void handleAction(Menu.Action action, int dir = 1)
    {
        switch (action.Type)
        {
            default:
            case Menu.ActionType.None:
                break;
            case Menu.ActionType.OpenMenu:
                _pastMenuStack.Add(_currentMenu.Name);
                configureForState(action.Param);
                break;
            case Menu.ActionType.SceneTransition:
                this.Hide();
                _actionTimer = new Timer(this.SceneTransitionDelay, false, true, onDelayedAction);
                _timerParam = action.Param;
                _changingScene = true;

                LocalEventNotifier.Event e = new LocalEventNotifier.Event();
                e.Name = this.EndSceneEvent;
                GlobalEvents.Notifier.SendEvent(e);
                break;
            case Menu.ActionType.CloseMenuWithEvent:
                this.Hide();
                _actionTimer = new Timer(this.EventSendDelay, false, true, onDelayedAction);
                _timerParam = action.Param;
                break;
            case Menu.ActionType.ChangeValue:
                OptionsValues.ChangeValue(action.Param, 1);
                refreshState();
                break;
            case Menu.ActionType.Reload:
                for (int i = 0; i < _pastMenuStack.Count; ++i)
                {
                    menuByStateName(_pastMenuStack[i]).Reload();
                }
                _currentMenu.Reload();
                refreshState();
                break;
        }
    }

    private void onDelayedAction()
    {
        _actionTimer = null;

        if (_changingScene)
        {
            if (_timerParam.IsEmpty())
                Application.Quit();
            else
                SceneManager.LoadScene(_timerParam, LoadSceneMode.Single);
        }
        else
        {
            LocalEventNotifier.Event e = new LocalEventNotifier.Event();
            e.Name = _timerParam;
            GlobalEvents.Notifier.SendEvent(e);
            _timerParam = null;
        }
    }

    private Menu menuByStateName(string stateName)
    {
        for (int i = 0; i < this.MenuStates.Length; ++i)
        {
            if (this.MenuStates[i].Name == stateName)
                return this.MenuStates[i];
        }
        return null;
    }

    private void highlight()
    {
        // Move highlight to correct element
        this.ElementVisuals[_currentHighlight].UnHighlight();
        _currentHighlight = _currentMenu.HighlightedElement;
        this.ElementVisuals[_currentHighlight].Highlight();

        this.HighlightIndicator.Destination = this.ElementVisuals[_currentMenu.HighlightedElement].transform;
        this.HighlightIndicator.BeginLerp();
    }

    private bool prev()
    {
        switch (this.Orientation)
        {
            default:
            case MenuOrientation.Vertical:
                return MenuInput.NavUp;
            case MenuOrientation.Horizontal:
                return MenuInput.NavLeft;
        }
    }

    private bool next()
    {
        switch (this.Orientation)
        {
            default:
            case MenuOrientation.Vertical:
                return MenuInput.NavDown;
            case MenuOrientation.Horizontal:
                return MenuInput.NavRight;
        }
    }

    private bool cyclePrev()
    {
        switch (this.Orientation)
        {
            default:
            case MenuOrientation.Vertical:
                return MenuInput.NavLeft;
            case MenuOrientation.Horizontal:
                return MenuInput.NavDown;
        }
    }

    private bool cycleNext()
    {
        switch (this.Orientation)
        {
            default:
            case MenuOrientation.Vertical:
                return MenuInput.NavRight;
            case MenuOrientation.Horizontal:
                return MenuInput.NavUp;
        }
    }
}
