﻿using UnityEngine;
using System;

public class PlayerVisualizer : VoBehavior
{
    public GameObject PlayerObject = null;
    public Sprite IdleSprite;
    public Sprite RunningSprite;
    public Sprite JumpingSprite;
    public Sprite FallingSprite;

    private const string IDLE_STATE = "idle";
    private const string RUNNING_STATE = "run";
    private const string JUMPING_STATE = "jump";
    private const string FALLING_STATE = "fall";

    void Awake()
    {
        _playerController = this.PlayerObject != null ? this.PlayerObject.GetComponent<PlayerController>() : this.GetComponent<PlayerController>();
        _stateMachine = new FSMStateMachine();
        _stateMachine.AddState(IDLE_STATE, this.updateGeneric, this.enterIdle);
        _stateMachine.AddState(RUNNING_STATE, this.updateGeneric, this.enterRunning);
        _stateMachine.AddState(JUMPING_STATE, this.updateGeneric, this.enterJumping);
        _stateMachine.AddState(FALLING_STATE, this.updateGeneric, this.enterFalling);
        _stateMachine.BeginWithInitialState(IDLE_STATE);

        this.localNotifier.Listen(PlayerUpdateFinishedEvent.NAME, this, this.UpdateVisual);
    }

    public void UpdateVisual(LocalEventNotifier.Event localEvent)
    {
        _stateMachine.Update();
        this.transform.localScale = new Vector3((_playerController.CurrentFacing == PlayerController.Facing.Left ? -1 : 1) * Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
    }

    /**
     * Private
     */
    private PlayerController _playerController;
    private FSMStateMachine _stateMachine;

    private string updateGeneric()
    {
        if (!_playerController.OnGround)
        {
            if (GameplayInput.Jump() && _playerController.Velocity.y > 0.0f)
                return JUMPING_STATE;
            return FALLING_STATE;
        }
        if (_playerController.MoveAxis.X != 0 && _playerController.Velocity.x != 0)
            return RUNNING_STATE;
        return IDLE_STATE;
    }

    private void enterIdle()
    {
        //TODO - Handle transitions from Previous State
        this.spriteRenderer.sprite = this.IdleSprite;
    }

    private void exitIdle()
    {
    }

    private void enterRunning()
    {
        this.spriteRenderer.sprite = this.RunningSprite;
    }

    private void exitRunning()
    {
    }

    private void enterJumping()
    {
        this.spriteRenderer.sprite = this.JumpingSprite;
    }

    private void exitJumping()
    {
    }

    private void enterFalling()
    {
        this.spriteRenderer.sprite = this.FallingSprite;
    }

    private void exitFalling()
    {
    }
}
