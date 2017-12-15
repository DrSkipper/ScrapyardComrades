using UnityEngine;

public class DoorConfigurer : ObjectConfigurer
{
    private const string NAME = "Door";
    private const string DOOR_TYPE = "type";
    public const string RED_DOOR = "red";
    public const string PURP_DOOR = "purp";
    public const string BLUE_DOOR = "blue";

    public SCSpriteAnimation RedOpenAnimation;
    public SCSpriteAnimation RedCloseAnimation;
    public SCSpriteAnimation PurpOpenAnimation;
    public SCSpriteAnimation PurpCloseAnimation;
    public SCSpriteAnimation BlueOpenAnimation;
    public SCSpriteAnimation BlueCloseAnimation;
    public DoorSwitchable DoorScript;
    public SwitchListener SwitchListenerScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(DOOR_TYPE, new string[] {
                    RED_DOOR,
                    PURP_DOOR,
                    BLUE_DOOR
                }),
                new ObjectParamType(SwitchConfigurer.INVERSE_SWITCH, new string[] {
                    SwitchConfigurer.FALSE,
                    SwitchConfigurer.TRUE
                }),
                new ObjectParamType(SwitchConfigurer.SWITCH_NAME, new string[] {
                    SwitchConfigurer.SWITCH_A,
                    SwitchConfigurer.SWITCH_B,
                    SwitchConfigurer.SWITCH_C,
                    SwitchConfigurer.SWITCH_D,
                    SwitchConfigurer.SWITCH_E,
                    SwitchConfigurer.SWITCH_F
                }),
                new ObjectParamType(SwitchConfigurer.DEFAULT_STATE, new string[] {
                    Switch.OFF,
                    Switch.ON
                }),
                new ObjectParamType(FanConfigurer.IGNORE_EVENTS, new string[] {
                    SwitchConfigurer.FALSE,
                    SwitchConfigurer.TRUE
                })
            };
        }
    }

    protected override void ConfigureParameter(string parameterName, string option)
    {
        switch (parameterName)
        {
            default:
                LogInvalidParameter(NAME, parameterName, option);
                break;
            case DOOR_TYPE:
                configureDoorType(option);
                break;
            case SwitchConfigurer.SWITCH_NAME:
                configureSwitchName(option);
                break;
            case SwitchConfigurer.INVERSE_SWITCH:
                configureInverseSwitch(option);
                break;
            case SwitchConfigurer.DEFAULT_STATE:
                configureDefaultState(option);
                break;
            case FanConfigurer.IGNORE_EVENTS:
                configureIgnoreEvents(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureDoorType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, DOOR_TYPE, option);
                //this.DoorScript.LockType = SCPickup.KeyType.None;
                break;
            case RED_DOOR:
                this.DoorScript.OpenAnimation = this.RedOpenAnimation;
                this.DoorScript.CloseAnimation = this.RedCloseAnimation;
                //this.DoorScript.LockType = SCPickup.KeyType.Red;
                break;
            case PURP_DOOR:
                this.DoorScript.OpenAnimation = this.PurpOpenAnimation;
                this.DoorScript.CloseAnimation = this.PurpCloseAnimation;
                //this.DoorScript.LockType = SCPickup.KeyType.Purple;
                break;
            case BLUE_DOOR:
                this.DoorScript.OpenAnimation = this.BlueOpenAnimation;
                this.DoorScript.CloseAnimation = this.BlueCloseAnimation;
                //this.DoorScript.LockType = SCPickup.KeyType.Blue;
                break;
        }
    }

    private void configureSwitchName(string option)
    {
        this.SwitchListenerScript.SwitchName = option;
    }

    private void configureInverseSwitch(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, SwitchConfigurer.INVERSE_SWITCH, option);
                this.SwitchListenerScript.InversedSwitch = false;
                break;
            case SwitchConfigurer.TRUE:
                this.SwitchListenerScript.InversedSwitch = true;
                break;
            case SwitchConfigurer.FALSE:
                this.SwitchListenerScript.InversedSwitch = false;
                break;
        }
    }

    private void configureDefaultState(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, SwitchConfigurer.DEFAULT_STATE, option);
                this.SwitchListenerScript.DefaultState = Switch.SwitchState.OFF;
                break;
            case Switch.ON:
                this.SwitchListenerScript.DefaultState = Switch.SwitchState.ON;
                break;
            case Switch.OFF:
                this.SwitchListenerScript.DefaultState = Switch.SwitchState.OFF;
                break;
        }
    }

    private void configureIgnoreEvents(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, FanConfigurer.IGNORE_EVENTS, option);
                this.SwitchListenerScript.IgnoreEvents = false;
                break;
            case SwitchConfigurer.TRUE:
                this.SwitchListenerScript.IgnoreEvents = true;
                break;
            case SwitchConfigurer.FALSE:
                this.SwitchListenerScript.IgnoreEvents = false;
                break;
        }
    }
}
