
public class PlayerController : SCCharacterController
{
    public struct PlayerInput : InputWrapper
    {
        public int MovementAxis { get { return GameplayInput.MovementAxis; } }
        public bool JumpBegin { get { return GameplayInput.JumpBegin; } }
        public bool JumpHeld { get { return GameplayInput.JumpHeld; } }
        public bool DodgeBegin { get { return GameplayInput.DodgeBegin; } }
        public bool DodgeHeld { get { return GameplayInput.DodgeHeld; } }
        public bool Duck { get { return GameplayInput.Duck; } }
        public bool AttackLightBegin { get { return GameplayInput.AttackLightBegin; } }
        public bool AttackLightHeld { get { return GameplayInput.AttackLightHeld; } }
        public bool AttackStrongBegin { get { return GameplayInput.AttackStrongBegin; } }
        public bool AttackStrongHeld { get { return GameplayInput.AttackStrongHeld; } }
        public bool UseItem { get { return GameplayInput.UseItem; } }
        public bool Interact { get { return GameplayInput.Interact; } }
        public bool PausePressed { get { return GameplayInput.PausePressed; } }

        public static PlayerInput Reference = new PlayerInput();
    }

    public override InputWrapper GatherInput()
    {
        return PlayerInput.Reference;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        GlobalEvents.Notifier.SendEvent(new PlayerSpawnedEvent(this.gameObject));
        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onWorldRecenter);
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, WorldRecenterEvent.NAME);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!_died && this.Damagable.Dead)
        {
            _died = true;
            GlobalEvents.Notifier.SendEvent(new PlayerDiedEvent());
        }
    }

    /**
     * Private
     */
    private bool _died;
    private const float SCREEN_TRANSITION_UP_BOOST = 3.0f;

    private void onWorldRecenter(LocalEventNotifier.Event e)
    {
        if (this.Velocity.y > 0 && (e as WorldRecenterEvent).RecenterOffset.Y < 0)
            this.Velocity.y += SCREEN_TRANSITION_UP_BOOST;
    }
}
