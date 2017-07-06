using UnityEngine;

public class ThrowableConfigurer : ObjectConfigurer
{
    private const string NAME = "Key";
    private const string PICKUP_TYPE = "type";
    private const string ROCK = "rock";
    private const string RED_KEY = "red";

    public SCPickup RockPickup;
    public SCPickup RedKeyPickup;
    public Pickup PickupScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(PICKUP_TYPE, new string[] {
                    ROCK,
                    RED_KEY
                })
            };
        }
    }

    public override void ConfigureParameter(string parameterName, string option)
    {
        switch (parameterName)
        {
            default:
                LogInvalidParameter(NAME, parameterName, option);
                break;
            case PICKUP_TYPE:
                configurePickupType(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configurePickupType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, PICKUP_TYPE, option);
                break;
            case ROCK:
                this.PickupScript.Data = this.RockPickup;
                break;
            case RED_KEY:
                this.PickupScript.Data = this.RedKeyPickup;
                break;
        }
    }
}
