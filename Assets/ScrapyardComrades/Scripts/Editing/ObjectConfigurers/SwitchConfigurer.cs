using UnityEngine;

public class SwitchConfigurer : ObjectConfigurer
{
    public const string NAME = "Switch";
    public const string SWITCH_NAME = "switch_name";
    public const string SWITCH_A = "A_switch";
    public const string SWITCH_B = "B_switch";
    public const string SWITCH_C = "C_switch";
    public const string SWITCH_D = "D_switch";
    public const string SWITCH_E = "E_switch";
    public const string SWITCH_F = "F_switch";
    public const string SWITCH_G = "G_switch";
    public const string SWITCH_H = "H_switch";
    public const string SWITCH_I = "I_switch";
    public const string SWITCH_J = "J_switch";
    public const string SWITCH_K = "K_switch";
    public const string INVERSE_SWITCH = "inverse";
    public const string TRUE = "true";
    public const string FALSE = "false";
    public const string ONE_WAY = "one_way";
    public const string DEFAULT_STATE = "default_state";
    public const string COLOR = "color";
    public const string YELLOW = "yellow";

    public Switch SwitchScript;
    public AreaSwitch SwitchDetectionScript; //TODO: Interface that AreaSwitch implements
    public SpriteRenderer ButtonRenderer;
    public Sprite YellowButtonSprite;
    public Sprite RedButtonSprite;
    public Sprite PurpButtonSprite;
    public Sprite BlueButtonSprite;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(SWITCH_NAME, new string[] {
                    SWITCH_A,
                    SWITCH_B,
                    SWITCH_C,
                    SWITCH_D,
                    SWITCH_E,
                    SWITCH_F,
                    SWITCH_G,
                    SWITCH_H,
                    SWITCH_I,
                    SWITCH_J,
                    SWITCH_K
                }),
                new ObjectParamType(INVERSE_SWITCH, new string[] {
                    FALSE,
                    TRUE
                }),
                new ObjectParamType(ONE_WAY, new string[] {
                    FALSE,
                    TRUE
                }),
                new ObjectParamType(DEFAULT_STATE, new string[] {
                    Switch.OFF,
                    Switch.ON
                }),
                new ObjectParamType(COLOR, new string[] {
                    YELLOW,
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
            case SWITCH_NAME:
                configureSwitchName(option);
                break;
            case INVERSE_SWITCH:
                configureInverseSwitch(option);
                break;
            case ONE_WAY:
                configureOneWay(option);
                break;
            case DEFAULT_STATE:
                configureDefaultState(option);
                break;
            case COLOR:
                configureColor(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureSwitchName(string option)
    {
        this.SwitchScript.SwitchName = option;
    }

    private void configureInverseSwitch(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, INVERSE_SWITCH, option);
                this.SwitchScript.InversedSwitch = false;
                break;
            case TRUE:
                this.SwitchScript.InversedSwitch = true;
                break;
            case FALSE:
                this.SwitchScript.InversedSwitch = false;
                break;
        }
    }

    private void configureOneWay(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, ONE_WAY, option);
                this.SwitchDetectionScript.OneWay = false;
                break;
            case TRUE:
                this.SwitchDetectionScript.OneWay = true;
                break;
            case FALSE:
                this.SwitchDetectionScript.OneWay = false;
                break;
        }
    }

    private void configureDefaultState(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, DEFAULT_STATE, option);
                this.SwitchScript.DefaultState = Switch.SwitchState.OFF;
                break;
            case Switch.ON:
                this.SwitchScript.DefaultState = Switch.SwitchState.ON;
                break;
            case Switch.OFF:
                this.SwitchScript.DefaultState = Switch.SwitchState.OFF;
                break;
        }
    }

    private void configureColor(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, COLOR, option);
                this.ButtonRenderer.sprite = this.YellowButtonSprite;
                break;
            case YELLOW:
                this.ButtonRenderer.sprite = this.YellowButtonSprite;
                break;
            case DoorConfigurer.RED_DOOR:
                this.ButtonRenderer.sprite = this.RedButtonSprite;
                break;
            case DoorConfigurer.PURP_DOOR:
                this.ButtonRenderer.sprite = this.PurpButtonSprite;
                break;
            case DoorConfigurer.BLUE_DOOR:
                this.ButtonRenderer.sprite = this.BlueButtonSprite;
                break;
        }
    }
}
