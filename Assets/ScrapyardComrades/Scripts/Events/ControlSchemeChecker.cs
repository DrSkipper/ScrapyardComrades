using UnityEngine;

public class ControlSchemeChecker : MonoBehaviour
{
    void Start()
    {
        _controlChangeEvent = new ControlSchemeChangeEvent(GameplayInput.UsingController());
        //GlobalEvents.Notifier.SendEvent(_controlChangeEvent);
    }

    void FixedUpdate()
    {
        bool prev = _controlChangeEvent.UsingController;
        _controlChangeEvent.UsingController = GameplayInput.UsingController();
        if (prev != _controlChangeEvent.UsingController)
            GlobalEvents.Notifier.SendEvent(_controlChangeEvent);
    }
    
    /**
     * Private
     */
    private ControlSchemeChangeEvent _controlChangeEvent;
}
