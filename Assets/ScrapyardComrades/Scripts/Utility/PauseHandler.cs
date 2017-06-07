using UnityEngine;
using System.Collections.Generic;

//TODO: OnSpawn should be replaced with method called by object pools, and remove listeners on OnReturnToPool. Apply this change to BatBarian as well.
public class PauseHandler : VoBehavior
{
    public PauseController.PauseGroup PauseGroup;
    public bool ListenOnAwake = false;

    void Awake()
    {
        _pausables = new List<Pausable>();

        IPausable[] components = this.GetComponents<IPausable>();
        for (int i = 0; i < components.Length; ++i)
        {
            _pausables.Add(new Pausable(components[i] as MonoBehaviour));
        }

        if (this.ListenOnAwake)
            this.OnSpawn();
    }

    void OnSpawn()
    {
        _currentPausedLayers = 0;
        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, pause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, resume);
    }

    void OnReturnToPool()
    {
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, PauseEvent.NAME);
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, ResumeEvent.NAME);
    }

    public void ForceEnabledStateIgnoringPauseState(bool enabled)
    {
        for (int i = 0; i < _pausables.Count; ++i)
        {
            _pausables[i].Force(enabled);
        }
    }

    /**
     * Private
     */
    private List<Pausable> _pausables;
    private uint _currentPausedLayers;

    private struct Pausable
    {
        public MonoBehaviour Behavior;
        public bool PreviouslyEnabled;

        public Pausable(MonoBehaviour behavior)
        {
            this.Behavior = behavior;
            this.PreviouslyEnabled = behavior.enabled;
        }

        public bool Pause()
        {
            if (this.Behavior == null)
                return false;
            this.PreviouslyEnabled = this.Behavior.enabled;
            this.Behavior.enabled = false;
            return true;
        }

        public bool Resume()
        {
            if (this.Behavior == null)
                return false;
            this.Behavior.enabled = this.PreviouslyEnabled;
            return true;
        }

        public void Force(bool enabled)
        {
            this.PreviouslyEnabled = enabled;
            this.Behavior.enabled = enabled;
        }
    }
    
    private void pause(LocalEventNotifier.Event e)
    {
        PauseController.PauseGroup group = (e as PauseEvent).PauseGroup;
        if (isAffected(group) && (_currentPausedLayers & (uint)group) != (uint)group)
        {
            bool prevPaused = _currentPausedLayers != 0;
            _currentPausedLayers += (uint)group;

            if (!prevPaused)
            {
                for (int i = 0; i < _pausables.Count;)
                {
                    if (_pausables[i].Pause())
                        ++i;
                    else
                        _pausables.RemoveAt(i);
                }
            }
        }
    }

    private void resume(LocalEventNotifier.Event e)
    {
        PauseController.PauseGroup group = (e as ResumeEvent).PauseGroup;
        if (_currentPausedLayers != 0 && isAffected(group) && (_currentPausedLayers & (uint)group) != 0)
        {
            _currentPausedLayers = _currentPausedLayers.Approach(0, (uint)group);
            if (_currentPausedLayers == 0)
            {
                for (int i = 0; i < _pausables.Count;)
                {
                    if (_pausables[i].Resume())
                        ++i;
                    else
                        _pausables.RemoveAt(i);
                }
            }
        }
    }

    private bool isAffected(PauseController.PauseGroup group)
    {
        return (uint)group >= (uint)this.PauseGroup;
    }
}
