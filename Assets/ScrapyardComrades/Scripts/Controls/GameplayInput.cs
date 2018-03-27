using UnityEngine;
using Rewired;
using System;

public static class GameplayInput
{
    public enum ControllerLayout
    {
        Xbox360,
        XboxOne,
        Playstation
    }

    private const int PLAYER_ID = 0;
    private const int MOVE_HORIZONTAL = 0;
    private const int DUCK = 32;
    private const int LOOK_UP = 33;
    private const int JUMP = 2;
    private const int DODGE = 7;
    private const int ATTACK_LIGHT = 3;
    private const int ATTACK_STRONG = 4;
    private const int USE_ITEM = 5;
    private const int INTERACT = 6;
    private const int PAUSE = 15;
    private const float AXIS_DEADZONE = 0.15f;
    private const string XBOX_ONE = "Xbox One";
    private const string PLAYSTATION_1 = "Dualshock";
    private const string PLAYSTATION_2 = "PS3";
    private const string PLAYSTATION_3 = "PS4";
    private const string PLAYSTATION_4 = "Playstation";

    public static int MovementAxis
    {
        get
        {
            float axis = ReInput.players.GetPlayer(PLAYER_ID).GetAxis(MOVE_HORIZONTAL);
            if (Mathf.Abs(axis) < AXIS_DEADZONE)
                return 0;
            return Math.Sign(axis);
        }
    }

    public static bool JumpBegin
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(JUMP);
        }
    }

    public static bool JumpHeld
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(JUMP);
        }
    }

    public static bool DodgeBegin
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(DODGE);
        }
    }

    public static bool DodgeHeld
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(DODGE);
        }
    }

    public static bool Duck
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(DUCK);
        }
    }

    public static bool LookUp
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(LOOK_UP);
        }
    }

    public static bool AttackLightBegin
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(ATTACK_LIGHT);
        }
    }

    public static bool AttackLightHeld
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(ATTACK_LIGHT);
        }
    }

    public static bool AttackStrongBegin
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(ATTACK_STRONG);
        }
    }

    public static bool AttackStrongHeld
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(ATTACK_STRONG);
        }
    }

    public static bool UseItem
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButton(USE_ITEM);
        }
    }

    public static bool Interact
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(INTERACT);
        }
    }

    public static bool PausePressed
    {
        get
        {
            return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(PAUSE);
        }
    }

    public static bool ButtonPressed(string buttonName)
    {
        return ReInput.players.GetPlayer(PLAYER_ID).GetButtonDown(buttonName);
    }

    public static bool ButtonLongHeld(string buttonName)
    {
        return ReInput.players.GetPlayer(PLAYER_ID).GetButtonLongPress(buttonName);
    }

    public static bool UsingController()
    {
        Controller c = ReInput.players.GetPlayer(PLAYER_ID).controllers.GetLastActiveController();
        if (c != null)
            return c.type == ControllerType.Joystick;
        return ReInput.players.GetPlayer(PLAYER_ID).controllers.joystickCount > 0;
    }

    public static ControllerLayout GetControllerLayout()
    {
        Controller c = ReInput.players.GetPlayer(PLAYER_ID).controllers.GetLastActiveController();
        //c.GetExtension<Rewired.ControllerExtensions.DualShock4Extension>();
        if (c != null)
        {
            if (c.name.Contains(XBOX_ONE))
                return ControllerLayout.XboxOne;
            else if (c.name.Contains(PLAYSTATION_1) ||
                c.name.Contains(PLAYSTATION_2) ||
                c.name.Contains(PLAYSTATION_3) ||
                c.name.Contains(PLAYSTATION_4))
                return ControllerLayout.Playstation;
        }
        return ControllerLayout.Xbox360;
    }
}
