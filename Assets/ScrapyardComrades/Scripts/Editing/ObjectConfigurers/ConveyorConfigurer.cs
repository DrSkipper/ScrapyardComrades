using UnityEngine;

public class ConveyorConfigurer : ObjectConfigurer
{
    public const string NAME = "Conveyor";
    public const string DIRECTION = "direction";
    public const string LEFT = "left";
    public const string RIGHT = "right";
    private const string POSITION = "position";
    private const string MID = "mid";
    private const string SWITCH_MODE = "switch_mode";
    private const string ENABLE_DISABLE = "enable_disable";
    private const string TOGGLE_DIRECTION = "toggle_direction";

    public ConveyorBelt ConveyorScript;
    public SwitchListener SwitchListenerScript;
    public PositionType Position;

    public SCSpriteAnimation LeftMidAnimation;
    public SCSpriteAnimation LeftLeftAnimation;
    public SCSpriteAnimation LeftRightAnimation;

    public SCSpriteAnimation RightMidAnimation;
    public SCSpriteAnimation RightLeftAnimation;
    public SCSpriteAnimation RightRightAnimation;

    [System.Serializable]
    public enum PositionType
    {
        Mid,
        Left,
        Right
    }

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(DIRECTION, new string[] {
                    LEFT,
                    RIGHT
                }),
                new ObjectParamType(POSITION, new string[] {
                    MID,
                    RIGHT,
                    LEFT
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
                }),
                new ObjectParamType(SWITCH_MODE, new string[] {
                    ENABLE_DISABLE,
                    TOGGLE_DIRECTION
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
            case DIRECTION:
                configureDirection(option);
                break;
            case POSITION:
                configurePosition(option);
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
            case SWITCH_MODE:
                configureSwitchMode(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureDirection(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, DIRECTION, option);
                break;
            case LEFT:
                this.ConveyorScript.DefaultDirection = SCCharacterController.Facing.Left;
                break;
            case RIGHT:
                this.ConveyorScript.DefaultDirection = SCCharacterController.Facing.Right;
                break;
        }
    }

    private void configurePosition(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, POSITION, option);
                this.ConveyorScript.LeftAnimation = LeftMidAnimation;
                this.ConveyorScript.RightAnimation = RightMidAnimation;
                break;
            case MID:
                this.ConveyorScript.LeftAnimation = LeftMidAnimation;
                this.ConveyorScript.RightAnimation = RightMidAnimation;
                break;
            case LEFT:
                this.ConveyorScript.LeftAnimation = LeftLeftAnimation;
                this.ConveyorScript.RightAnimation = RightLeftAnimation;
                break;
            case RIGHT:
                this.ConveyorScript.LeftAnimation = LeftRightAnimation;
                this.ConveyorScript.RightAnimation = RightRightAnimation;
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

    private void configureSwitchMode(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, SWITCH_MODE, option);
                this.ConveyorScript.OnSwitchAction = ConveyorBelt.SwitchBehavior.EnableDisable;
                break;
            case ENABLE_DISABLE:
                this.ConveyorScript.OnSwitchAction = ConveyorBelt.SwitchBehavior.EnableDisable;
                break;
            case TOGGLE_DIRECTION:
                this.ConveyorScript.OnSwitchAction = ConveyorBelt.SwitchBehavior.DirectionToggle;
                break;
        }
    }
}
