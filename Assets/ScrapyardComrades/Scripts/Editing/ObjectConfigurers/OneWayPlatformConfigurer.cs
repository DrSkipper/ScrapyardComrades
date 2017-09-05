using System;
using UnityEngine;

public class OneWayPlatformConfigurer : ObjectConfigurer
{
    public SpriteRenderer SpriteRenderer;
    public Sprite SpriteA;
    public Sprite SpriteB;
    public Sprite SpriteC;
    public Sprite SpriteD;

    private const string NAME = "OneWayPlatform";
    private const string SPRITE_TYPE = "type";
    private const string SPRITE_A = "A";
    private const string SPRITE_B = "B";
    private const string SPRITE_C = "C";
    private const string SPRITE_D = "D";

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(SPRITE_TYPE, new string[] {
                    SPRITE_A,
                    SPRITE_B,
                    SPRITE_C,
                    SPRITE_D
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
            case SPRITE_TYPE:
                configureSpriteType(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureSpriteType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, SPRITE_TYPE, option);
                this.SpriteRenderer.sprite = this.SpriteA;
                break;
            case SPRITE_A:
                this.SpriteRenderer.sprite = this.SpriteA;
                break;
            case SPRITE_B:
                this.SpriteRenderer.sprite = this.SpriteB;
                break;
            case SPRITE_C:
                this.SpriteRenderer.sprite = this.SpriteC;
                break;
            case SPRITE_D:
                this.SpriteRenderer.sprite = this.SpriteD;
                break;
        }
    }
}
