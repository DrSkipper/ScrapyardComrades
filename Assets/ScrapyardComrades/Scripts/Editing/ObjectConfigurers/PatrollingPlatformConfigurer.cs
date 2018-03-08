using UnityEngine;
using System.Linq;

public class PatrollingPlatformConfigurer : ObjectConfigurer
{
    private const string NAME = "PatrollingPlatform";
    private const string CONFIGURATION = "config";
    public SpriteRenderer Renderer;
    public IntegerRectCollider Collider;
    public Transform DestinationLocation;
    public Sprite[] Configurations;
    public IntegerVector[] ColliderSizes;

    public override Transform GetSecondaryTransform()
    {
        return this.DestinationLocation;
    }

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(CONFIGURATION, this.Configurations.Select(x => x.name).ToArray())
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
            case CONFIGURATION:
                configure(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configure(string option)
    {
        for (int i = 0; i < this.Configurations.Length; ++i)
        {
            if (this.Configurations[i].name == option)
            {
                useConfiguration(i);
                return;
            }
        }

        if (this.Configurations.Length > 0)
            useConfiguration(0);
        LogInvalidParameter(NAME, CONFIGURATION, option);
    }

    private void useConfiguration(int config)
    {
        this.Renderer.sprite = this.Configurations[config];
        this.Collider.Size = this.ColliderSizes[config];
    }
}
