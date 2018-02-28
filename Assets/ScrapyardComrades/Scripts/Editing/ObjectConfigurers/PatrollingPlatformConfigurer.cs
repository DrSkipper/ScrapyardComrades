using UnityEngine;

public class PatrollingPlatformConfigurer : ObjectConfigurer
{
    private const string NAME = "PatrollingPlatform";
    public Transform DestinationLocation;

    public override Transform GetSecondaryTransform()
    {
        return this.DestinationLocation;
    }

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] { };
        }
    }

    protected override void ConfigureParameter(string parameterName, string option)
    {
        LogInvalidParameter(NAME, parameterName, option);
    }
}
