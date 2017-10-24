using UnityEngine;

public class SpriteConfigurer : ObjectConfigurer
{
    public const string NAME = "Sprite";
    private const string FLIP_TYPE = "flip";

    public SpriteRenderer Renderer;
    
    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(FLIP_TYPE, new string[] {
                    SwitchConfigurer.FALSE,
                    SwitchConfigurer.TRUE
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
            case FLIP_TYPE:
                configureFlipType(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureFlipType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, FLIP_TYPE, option);
                this.Renderer.flipX = false;
                break;
            case SwitchConfigurer.FALSE:
                this.Renderer.flipX = false;
                break;
            case SwitchConfigurer.TRUE:
                this.Renderer.flipX = true;
                break;
        }
    }
}
