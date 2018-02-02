using UnityEngine;

public class PowerupConfigurer : ObjectConfigurer
{
    private const string NAME = "Powerup";
    private const string THROW_ANIM = "ThrowAnimation";

    public PowerupObject PowerupScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(TriggerBoxConfigurer.TRIGGER_NAME, new string[] {
                    TriggerBoxConfigurer.TRIGGER_NONE,
                    TriggerBoxConfigurer.POWERUP_A,
                    TriggerBoxConfigurer.POWERUP_B,
                    TriggerBoxConfigurer.POWERUP_C,
                    TriggerBoxConfigurer.POWERUP_D,
                    TriggerBoxConfigurer.POWERUP_E,
                    TriggerBoxConfigurer.POWERUP_F,
                    TriggerBoxConfigurer.POWERUP_G,
                    TriggerBoxConfigurer.POWERUP_H,
                    TriggerBoxConfigurer.POWERUP_I,
                    TriggerBoxConfigurer.POWERUP_J,
                    TriggerBoxConfigurer.POWERUP_K
                }),
                new ObjectParamType(THROW_ANIM, new string[] {
                    Switch.ON,
                    Switch.OFF
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
            case TriggerBoxConfigurer.TRIGGER_NAME:
                configureTriggerName(option);
                break;
            case THROW_ANIM:
                configureThrowAnim(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureTriggerName(string option)
    {
        this.PowerupScript.ThrowOnStateKey = option;
    }

    private void configureThrowAnim(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, THROW_ANIM, option);
                this.PowerupScript.ThrowSpawnAnimation = true;
                break;
            case Switch.ON:
                this.PowerupScript.ThrowSpawnAnimation = true;
                break;
            case Switch.OFF:
                this.PowerupScript.ThrowSpawnAnimation = false;
                break;
        }
    }
}
