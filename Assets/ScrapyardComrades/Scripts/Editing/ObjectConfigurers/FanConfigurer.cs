using UnityEngine;

public class FanConfigurer : ObjectConfigurer
{
    public const string NAME = "Fan";
    private const string ATTACH_DIR_TYPE = "attach";
    private const string ATTACH_DOWN = "d";
    private const string ATTACH_UP = "u";
    private const string ATTACH_LEFT = "l";
    private const string ATTACH_RIGHT = "r";
    public const string IGNORE_EVENTS = "ignore_events";

    public WindFan FanScript;
    public SwitchListener SwitchListenerScript;
    public SCSpriteAnimation YellowStoppedAnim;
    public SCSpriteAnimation YellowRunningAnim;
    public SCSpriteAnimation RedStoppedAnim;
    public SCSpriteAnimation RedRunningAnim;
    public SCSpriteAnimation PurpStoppedAnim;
    public SCSpriteAnimation PurpRunningAnim;
    public SCSpriteAnimation BlueStoppedAnim;
    public SCSpriteAnimation BlueRunningAnim;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(ATTACH_DIR_TYPE, new string[] {
                    ATTACH_DOWN,
                    ATTACH_UP,
                    ATTACH_LEFT,
                    ATTACH_RIGHT
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
                    SwitchConfigurer.SWITCH_F,
                    SwitchConfigurer.SWITCH_G,
                    SwitchConfigurer.SWITCH_H,
                    SwitchConfigurer.SWITCH_I,
                    SwitchConfigurer.SWITCH_J,
                    SwitchConfigurer.SWITCH_K
                }),
                new ObjectParamType(SwitchConfigurer.DEFAULT_STATE, new string[] {
                    Switch.OFF,
                    Switch.ON
                }),
                new ObjectParamType(IGNORE_EVENTS, new string[] {
                    SwitchConfigurer.FALSE,
                    SwitchConfigurer.TRUE
                }),
                new ObjectParamType(SwitchConfigurer.COLOR, new string[]
                {
                    SwitchConfigurer.YELLOW,
                    DoorConfigurer.RED_DOOR,
                    DoorConfigurer.PURP_DOOR,
                    DoorConfigurer.BLUE_DOOR
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
            case ATTACH_DIR_TYPE:
                configureAttachType(option);
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
            case IGNORE_EVENTS:
                configureIgnoreEvents(option);
                break;
            case SwitchConfigurer.COLOR:
                configureColor(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureAttachType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, ATTACH_DIR_TYPE, option);
                break;
            case ATTACH_DOWN:
                this.FanScript.AttachedAt = TurretController.AttachDir.Down;
                break;
            case ATTACH_UP:
                this.FanScript.AttachedAt = TurretController.AttachDir.Up;
                break;
            case ATTACH_LEFT:
                this.FanScript.AttachedAt = TurretController.AttachDir.Left;
                break;
            case ATTACH_RIGHT:
                this.FanScript.AttachedAt = TurretController.AttachDir.Right;
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
                LogInvalidParameter(NAME, IGNORE_EVENTS, option);
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

    private void configureColor(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, SwitchConfigurer.COLOR, option);
                this.FanScript.StoppedAnimation = this.YellowStoppedAnim;
                this.FanScript.RunningAnimation = this.YellowRunningAnim;
                break;
            case SwitchConfigurer.YELLOW:
                this.FanScript.StoppedAnimation = this.YellowStoppedAnim;
                this.FanScript.RunningAnimation = this.YellowRunningAnim;
                break;
            case DoorConfigurer.RED_DOOR:
                this.FanScript.StoppedAnimation = this.RedStoppedAnim;
                this.FanScript.RunningAnimation = this.RedRunningAnim;
                break;
            case DoorConfigurer.PURP_DOOR:
                this.FanScript.StoppedAnimation = this.PurpStoppedAnim;
                this.FanScript.RunningAnimation = this.PurpRunningAnim;
                break;
            case DoorConfigurer.BLUE_DOOR:
                this.FanScript.StoppedAnimation = this.BlueStoppedAnim;
                this.FanScript.RunningAnimation = this.BlueRunningAnim;
                break;
        }
    }
}
