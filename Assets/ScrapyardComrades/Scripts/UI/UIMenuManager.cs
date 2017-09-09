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
            if (MenuInput.NavDown)
            {
                _currentMenu.HighlightNext();
                highlight();
            }
            else if (MenuInput.NavUp)
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

    private void configureForEmptyState()
    {
        this.MenuBounds.Destination = this.EmptyState;
        this.MenuBounds.BeginLerp();
    }

    private void handleAction(Menu.Action action)
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
}
