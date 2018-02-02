using UnityEngine;

public class TriggerBoxConfigurer : ObjectConfigurer
{
    private const string NAME = "TriggerBox";
    public const string TRIGGER_NAME = "trigger_name";
    public const string TRIGGER_NONE = "trigger_none";
    public const string TRIGGER_A = "trigger_a";
    public const string TRIGGER_B = "trigger_b";
    public const string TRIGGER_C = "trigger_c";
    public const string TRIGGER_D = "trigger_d";
    public const string TRIGGER_E = "trigger_e";
    public const string TRIGGER_F = "trigger_f";
    public const string TRIGGER_G = "trigger_g";
    public const string TRIGGER_H = "trigger_h";
    public const string TRIGGER_I = "trigger_i";
    public const string TRIGGER_J = "trigger_j";
    public const string TRIGGER_K = "trigger_k";

    public const string POWERUP_A = "powerup_a";
    public const string POWERUP_B = "powerup_b";
    public const string POWERUP_C = "powerup_c";
    public const string POWERUP_D = "powerup_d";
    public const string POWERUP_E = "powerup_e";
    public const string POWERUP_F = "powerup_f";
    public const string POWERUP_G = "powerup_g";
    public const string POWERUP_H = "powerup_h";
    public const string POWERUP_I = "powerup_i";
    public const string POWERUP_J = "powerup_j";
    public const string POWERUP_K = "powerup_k";

    public TriggerBox TriggerScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(TRIGGER_NAME, new string[] {
                    TRIGGER_NONE,
                    TRIGGER_A,
                    TRIGGER_B,
                    TRIGGER_C,
                    TRIGGER_D,
                    TRIGGER_E,
                    TRIGGER_F,
                    TRIGGER_G,
                    TRIGGER_H,
                    TRIGGER_I,
                    TRIGGER_J,
                    TRIGGER_K,
                    POWERUP_A,
                    POWERUP_B,
                    POWERUP_C,
                    POWERUP_D,
                    POWERUP_E,
                    POWERUP_F,
                    POWERUP_G,
                    POWERUP_H,
                    POWERUP_I,
                    POWERUP_J,
                    POWERUP_K
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
            case TRIGGER_NAME:
                configureTriggerName(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureTriggerName(string option)
    {
        this.TriggerScript.StateKeyToSetOn = option;
    }
}
