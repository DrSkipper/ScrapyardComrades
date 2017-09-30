using UnityEngine;

public class SwitchListener : MonoBehaviour
{
    public Switch.SwitchState DefaultState = Switch.SwitchState.OFF;
    public bool InversedSwitch = false;
    public bool IgnoreEvents = false;
    public string SwitchName = "switch";
    public Switch.StateChangeDelegate StateChangeCallback;
    
    public Switch.SwitchState CurrentState {
        get {
            return _currentState;
        }
        private set {
            _currentState = value;
            if (this.StateChangeCallback != null)
                this.StateChangeCallback(_currentState);
        }
    }

    void OnSpawn()
    {
        if (!this.IgnoreEvents)
        {
            GlobalEvents.Notifier.Listen(SwitchStateChangedEvent.NAME, this, onSwitchStateChange);
            if (SaveData.DataLoaded)
                configureForState(readState());
        }
        else
        {
            configureForState(this.DefaultState);
        }
    }

    void OnReturnToPool()
    {
        if (!this.IgnoreEvents)
            GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, SwitchStateChangedEvent.NAME);
    }

    /**
     * Private
     */
    private Switch.SwitchState _currentState;

    private void configureForState(Switch.SwitchState state)
    {
        if (this.InversedSwitch)
        {
            state = (state == Switch.SwitchState.OFF) ? Switch.SwitchState.ON : Switch.SwitchState.OFF;
        }
        this.CurrentState = state;
    }

    private void onSwitchStateChange(LocalEventNotifier.Event e)
    {
        SwitchStateChangedEvent switchEvent = e as SwitchStateChangedEvent;
        if (switchEvent.SwitchName == this.SwitchName)
            configureForState(switchEvent.State);
    }
    
    private Switch.SwitchState readState()
    {
        return Switch.SwitchStateFromKey(SaveData.GetGlobalState(this.SwitchName, Switch.SwitchKeyFromState(this.DefaultState)));
    }
}
