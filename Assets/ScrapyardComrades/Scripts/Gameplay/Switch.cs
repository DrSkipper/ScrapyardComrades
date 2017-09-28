﻿using UnityEngine;

public class Switch : MonoBehaviour
{
    public const string OFF = "OFF";
    public const string ON = "ON";

    public SwitchState DefaultState = SwitchState.OFF;
    public string SwitchName = "switch";
    public StateChangeDelegate StateChangeCallback;
    public delegate void StateChangeDelegate(SwitchState state);

    public SwitchState CurrentState {
        get {
            return _currentState;
        }
        private set {
            _currentState = value;
            if (this.StateChangeCallback != null)
                this.StateChangeCallback(_currentState);
        }
    }

    [System.Serializable]
    public enum SwitchState
    {
        OFF,
        ON
    }

    void Awake()
    {
        _switchStateEvent = new SwitchStateChangedEvent(this.SwitchName, this.DefaultState);
    }

    void OnSpawn()
    {
        if (SaveData.DataLoaded)
            this.CurrentState = readState();
    }

    public void ToggleSwitch()
    {
        this.CurrentState = this.CurrentState == SwitchState.OFF ? SwitchState.ON : SwitchState.OFF;
        saveState();
    }
    
    public static string SwitchKeyFromState(SwitchState state)
    {
        switch (state)
        {
            default:
            case SwitchState.OFF:
                return OFF;
            case SwitchState.ON:
                return ON;
        }
    }

    public static SwitchState SwitchStateFromKey(string key)
    {
        switch (key)
        {
            default:
            case OFF:
                return SwitchState.OFF;
            case ON:
                return SwitchState.ON;
        }
    }

    /**
     * Private
     */
    private SwitchState _currentState;
    private SwitchStateChangedEvent _switchStateEvent;
    private bool _sentEvent;

    private SwitchState readState()
    {
        return SwitchStateFromKey(SaveData.GetGlobalState(this.SwitchName, SwitchKeyFromState(this.DefaultState)));
    }

    private void saveState()
    {
        _sentEvent = true;
        _switchStateEvent.State = this.CurrentState;
        GlobalEvents.Notifier.SendEvent(_switchStateEvent);
        SaveData.SetGlobalState(this.SwitchName, SwitchKeyFromState(this.CurrentState));
        _sentEvent = false;
    }

    private void onSwitchStateChange(LocalEventNotifier.Event e)
    {
        if (_sentEvent)
            return;

        SwitchStateChangedEvent switchEvent = e as SwitchStateChangedEvent;
        if (switchEvent.SwitchName == this.SwitchName)
        {
            this.CurrentState = switchEvent.State;
        }
    }
}
