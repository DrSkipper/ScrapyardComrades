﻿using UnityEngine;

public class EnemyController : SCCharacterController
{
    public AIType AI = AIType.Simple;

    [System.Serializable]
    public enum AIType
    {
        Simple,
        Guard,
        Office
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        //TODO: Data-drive health
        this.Damagable.Health = this.Damagable.MaxHealth;

        //TODO: Data-drive which AI to use, as well as parameters
        switch (this.AI)
        {
            default:
            case AIType.Simple:
                _ai = new SimpleAI(250, 425, 75, 48, 4);
                break;
            case AIType.Guard:
                _ai = new GuardAI(275, 400, 156, 114, 128, 8);
                break;
            case AIType.Office:
                _ai = new OfficeAI(280, 425, 200, 145, 180, 100, 60, 4);
                break;
        }
    }

    public override InputWrapper GatherInput()
    {
        _input.ApplyValues(_ai.RunAI(gatherAiInput(PlayerReference.Collider, PlayerReference.IsAlive)));
        return _input;
    }

    /**
     * Private
     */
    private AI _ai;
    private EnemyInput _input;

    private AIInput gatherAiInput(IntegerCollider target, bool targetAlive)
    {
        AIInput aiInput = new AIInput();
        aiInput.HasTarget = target != null;
        aiInput.TargetAlive = targetAlive;
        aiInput.OurPosition = (Vector2)this.transform.position;
        aiInput.TargetPosition = target != null ? (Vector2)target.transform.position : Vector2.zero;
        aiInput.OurCollider = this.Hurtbox;
        aiInput.TargetCollider = target;
        aiInput.OnGround = this.OnGround;
        aiInput.ExecutingMove = this.ExecutingMove;
        aiInput.InMoveCooldown = this.InMoveCooldown;
        aiInput.HitStunned = this.HitStunned;

        if (targetAlive)
        {
            SCCharacterController targetController = target.GetComponent<SCCharacterController>();
            aiInput.TargetOnGround = targetController.OnGround && Mathf.RoundToInt(targetController.Velocity.y) <= 0;
        }
        else
        {
            aiInput.TargetOnGround = false;
        }
        return aiInput;
    }
    
    private struct EnemyInput : InputWrapper
    {
        public int MovementAxis { get; private set; }
        public bool JumpBegin { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DodgeBegin { get; private set; }
        public bool DodgeHeld { get; private set; }
        public bool Duck { get; private set; }
        public bool LookUp { get; private set; }
        public bool AttackLightBegin { get; private set; }
        public bool AttackLightHeld { get; private set; }
        public bool AttackStrongBegin { get; private set; }
        public bool AttackStrongHeld { get; private set; }
        public bool UseItem { get; private set; }
        public bool Interact { get; private set; }
        public bool PausePressed { get; private set; }

        public void ApplyValues(AIOutput output)
        {
            this.MovementAxis = output.MovementDirection;
            this.JumpBegin = output.Jump;
            this.JumpHeld = output.Jump;
            this.DodgeBegin = output.Dodge;
            this.DodgeHeld = output.Dodge;
            this.Duck = false;
            this.AttackLightBegin = output.Attack;
            this.AttackLightHeld = output.Attack;
            this.AttackStrongBegin = output.AttackStrong;
            this.AttackStrongHeld = output.AttackStrong;
            this.UseItem = false;
            this.Interact = output.Interact;
            this.PausePressed = false;
        }
    }
}
