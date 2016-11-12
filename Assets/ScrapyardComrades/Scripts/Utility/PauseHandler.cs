using UnityEngine;
using System.Collections.Generic;

public class PauseHandler : VoBehavior
{
    public PauseController.PauseGroup PauseGroup;

    void Awake()
    {
        _pausables = new List<Pausable>();
        //_animator = this.GetComponent<Animator>();

        IPausable[] components = this.GetComponents<IPausable>();
        for (int i = 0; i < components.Length; ++i)
        {
            _pausables.Add(new Pausable(components[i] as MonoBehaviour));
        }

        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, pause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, resume);
    }

    /**
     * Private
     */
    private List<Pausable> _pausables;

    //TODO: Will require state saving and continuing code to get Animator class to work properly with this pausing system. As is the animator state will be restarted when returning from pause. GetCurrentAnimatorStateInfo() and GetNextAnimatorStateInfo() may need to be used, and then data from those passed int Play();
    //private Animator _animator;

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
    }

    private void pause(LocalEventNotifier.Event e)
    {
        if (isAffected((e as PauseEvent).PauseGroup))
        {
            for (int i = 0; i < _pausables.Count;)
            {
                if (_pausables[i].Pause())
                    ++i;
                else
                    _pausables.RemoveAt(i);
            }

            /*
            if (_animator != null)
                _animator.GetCurrentAnimatorStateInfo();
                */
        }
    }

    private void resume(LocalEventNotifier.Event e)
    {
        if (isAffected((e as ResumeEvent).PauseGroup))
        {
            for (int i = 0; i < _pausables.Count;)
            {
                if (_pausables[i].Resume())
                    ++i;
                else
                    _pausables.RemoveAt(i);
            }

            /*
            if (_animator != null)
                _animator.
                */
        }
    }

    private bool isAffected(PauseController.PauseGroup group)
    {
        return ((uint)group & (uint)this.PauseGroup) == (uint)this.PauseGroup;
    }
}
