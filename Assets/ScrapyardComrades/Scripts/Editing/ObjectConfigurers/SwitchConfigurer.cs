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

    public Switch SwitchScript;

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
                    SWITCH_F
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
        }
    }

    /**
     * Private
     */
    private void configureSwitchName(string option)
    {
        this.SwitchScript.SwitchName = option;
    }
}
