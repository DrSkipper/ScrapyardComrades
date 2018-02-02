using UnityEngine;

public class VatConfigurer : ObjectConfigurer
{
    private const string NAME = "Vat";

    public MutantVat VatScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(TriggerBoxConfigurer.TRIGGER_NAME, new string[] {
                    TriggerBoxConfigurer.TRIGGER_NONE,
                    TriggerBoxConfigurer.TRIGGER_A,
                    TriggerBoxConfigurer.TRIGGER_B,
                    TriggerBoxConfigurer.TRIGGER_C,
                    TriggerBoxConfigurer.TRIGGER_D,
                    TriggerBoxConfigurer.TRIGGER_E,
                    TriggerBoxConfigurer.TRIGGER_F,
                    TriggerBoxConfigurer.TRIGGER_G,
                    TriggerBoxConfigurer.TRIGGER_H,
                    TriggerBoxConfigurer.TRIGGER_I,
                    TriggerBoxConfigurer.TRIGGER_J,
                    TriggerBoxConfigurer.TRIGGER_K
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
        }
    }

    /**
     * Private
     */
    private void configureTriggerName(string option)
    {
        this.VatScript.StateKeyForBreak = option;
    }
}
