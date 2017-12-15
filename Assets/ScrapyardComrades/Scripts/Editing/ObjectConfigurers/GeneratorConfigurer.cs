using UnityEngine;

public class GeneratorConfigurer : ObjectConfigurer
{
    private const string NAME = "Generator";

    public SpriteRenderer Renderer;
    public Sprite RedSprite;
    public Sprite PurpSprite;
    public Sprite BlueSprite;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(SwitchConfigurer.COLOR, new string[] {
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
            case SwitchConfigurer.COLOR:
                configureColor(option);
                break;
        }
    }

    private void configureColor(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, SwitchConfigurer.COLOR, option);
                this.Renderer.sprite = this.RedSprite;
                break;
            case DoorConfigurer.RED_DOOR:
                this.Renderer.sprite = this.RedSprite;
                break;
            case DoorConfigurer.PURP_DOOR:
                this.Renderer.sprite = this.PurpSprite;
                break;
            case DoorConfigurer.BLUE_DOOR:
                this.Renderer.sprite = this.BlueSprite;
                break;
        }
    }
}
