﻿using UnityEngine;

public class MutantVat : VoBehavior, IPausable
{
    public PooledObject SpawnPrefab;
    public Transform SpawnPosition;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation IdleAnimation;
    public SCSpriteAnimation BreakAnimation;
    public SCSpriteAnimation BrokenAnimation;
    public SoundData.Key BreakSfxKey;
    public SoundData.Key CrackSfxKey;
    public int CrackFrame = 0;
    public string StateKeyForBreak;

    public string SpawnEntityKey { get { return _entity.EntityName + StringExtensions.UNDERSCORE + this.SpawnPrefab.name; } }

    void Awake()
    {
        _entity = this.GetComponent<WorldEntity>();
    }

    void OnSpawn()
    {
        GlobalEvents.Notifier.Listen(SwitchStateChangedEvent.NAME, this, onSwitchStateChange);
        _conditionsMet = false;
        _state = State.Unbroken;

        switch(_entity.StateTag)
        {
            default:
                this.Animator.PlayAnimation(this.IdleAnimation);
                if (SaveData.DataLoaded && SaveData.GetGlobalState(this.StateKeyForBreak, Switch.OFF) != Switch.OFF)
                    _conditionsMet = true;
                break;
            case VAT_BROKEN:
                if (canSpawn())
                    initiateSpawn();
                else
                    this.Animator.PlayAnimation(this.BrokenAnimation);
                break;
        }
    }

    void FixedUpdate()
    {
        switch (_state)
        {
            default:
            case State.Unbroken:
                if (_conditionsMet && canSpawn())
                    initiateSpawn();
                break;
            case State.Breaking:
                if (this.Animator.Elapsed >= this.Animator.CurrentAnimation.LengthInFrames)
                    spawn();
                else if (this.Animator.Elapsed == this.CrackFrame * this.Animator.GetFrameDuration())
                    SoundManager.Play(this.CrackSfxKey, this.transform);
                break;
            case State.Broken:
                break;
        }
    }

    void OnReturnToPool()
    {
        if (_state == State.Unbroken)
            GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, SwitchStateChangedEvent.NAME);
    }

    /**
     * Private
     */
    private const string VAT_BROKEN = "BROKEN";
    private WorldEntity _entity;
    private State _state;
    private bool _conditionsMet;

    private enum State
    {
        Unbroken,
        Breaking,
        Broken
    }

    private bool canSpawn()
    {
        return EntityTracker.Instance.GetEntity(_entity.QuadName, this.SpawnEntityKey, this.SpawnPrefab.name).CanLoad;
    }

    private void initiateSpawn()
    {
        _state = State.Breaking;
        this.Animator.PlayAnimation(this.BreakAnimation);

        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, SwitchStateChangedEvent.NAME);
    }

    private void spawn()
    {
        _state = State.Broken;
        this.Animator.PlayAnimation(this.BrokenAnimation);
        SoundManager.Play(SoundData.Key.Stasis_GlassShatter);

        PooledObject spawnObj = this.SpawnPrefab.Retain();
        WorldEntity otherEntity = spawnObj.GetComponent<WorldEntity>();
        otherEntity.QuadName = _entity.QuadName;
        otherEntity.EntityName = this.SpawnEntityKey;
        spawnObj.GetComponent<Renderer>().sortingOrder = this.spriteRenderer.sortingOrder + 1;

        EntityTracker.Instance.TrackLoadedEntity(otherEntity);
        spawnObj.transform.SetPosition2D(this.SpawnPosition.position);
        spawnObj.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD);
    }

    private void onSwitchStateChange(LocalEventNotifier.Event e)
    {
        SwitchStateChangedEvent switchEvent = e as SwitchStateChangedEvent;
        if (switchEvent.SwitchName == this.StateKeyForBreak)
            _conditionsMet = switchEvent.State == Switch.SwitchState.ON;
    }
}
