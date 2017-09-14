using System;
using UnityEngine;

public class PosterConfigurer : ObjectConfigurer
{
    public SpriteRenderer BGRenderer;
    public Sprite BGSpriteA;
    public Sprite BGSpriteB;
    public IntegerVector BGOffsetA;
    public IntegerVector BGOffsetB;
    public ControlSchemeImage ButtonRenderer;
    public Sprite ControllerJumpButton;
    public Sprite KeyboardJumpButton;
    public Sprite ControllerSlideButton;
    public Sprite KeyboardSlideButton;
    public Sprite ControllerLightAttackButton;
    public Sprite KeyboardLightAttackButton;
    public Sprite ControllerStrongAttackButton;
    public Sprite KeyboardStrongAttackButton;
    public float KeyboardJumpScale = 0.25f;
    public float KeyboardSlideScale = 0.5f;
    public float KeyboardLightAttackScale = 1.0f;
    public float KeyboardStrongAttackScale = 1.0f;

    private const string NAME = "Poster";
    private const string BG_TYPE = "bg";
    private const string BG_A = "A";
    private const string BG_B = "B";
    private const string INPUT_TYPE = "input";
    private const string INPUT_JUMP = "jump";
    private const string INPUT_SLIDE = "slide";
    private const string INPUT_LIGHTATTACK = "l_attack";
    private const string INPUT_STRONGATTACK = "s_attack";

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(BG_TYPE, new string[] {
                    BG_A,
                    BG_B
                }),
                new ObjectParamType(INPUT_TYPE, new string[] {
                    INPUT_JUMP,
                    INPUT_SLIDE,
                    INPUT_LIGHTATTACK,
                    INPUT_STRONGATTACK
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
            case BG_TYPE:
                configureBgType(option);
                break;
            case INPUT_TYPE:
                configureInputType(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureBgType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, BG_TYPE, option);
                this.BGRenderer.sprite = this.BGSpriteA;
                this.ButtonRenderer.transform.SetLocalPosition2D(this.BGOffsetA.X, this.BGOffsetA.Y);
                break;
            case BG_A:
                this.BGRenderer.sprite = this.BGSpriteA;
                this.ButtonRenderer.transform.SetLocalPosition2D(this.BGOffsetA.X, this.BGOffsetA.Y);
                break;
            case BG_B:
                this.BGRenderer.sprite = this.BGSpriteB;
                this.ButtonRenderer.transform.SetLocalPosition2D(this.BGOffsetB.X, this.BGOffsetB.Y);
                break;
        }
    }

    private void configureInputType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, INPUT_TYPE, option);
                this.ButtonRenderer.ControllerSprite = this.ControllerJumpButton;
                this.ButtonRenderer.KeyboardSprite = this.KeyboardJumpButton;
                this.ButtonRenderer.KeyboardScale = this.KeyboardJumpScale;
                break;
            case INPUT_JUMP:
                this.ButtonRenderer.ControllerSprite = this.ControllerJumpButton;
                this.ButtonRenderer.KeyboardSprite = this.KeyboardJumpButton;
                this.ButtonRenderer.KeyboardScale = this.KeyboardJumpScale;
                break;
            case INPUT_SLIDE:
                this.ButtonRenderer.ControllerSprite = this.ControllerSlideButton;
                this.ButtonRenderer.KeyboardSprite = this.KeyboardSlideButton;
                this.ButtonRenderer.KeyboardScale = this.KeyboardSlideScale;
                break;
            case INPUT_LIGHTATTACK:
                this.ButtonRenderer.ControllerSprite = this.ControllerLightAttackButton;
                this.ButtonRenderer.KeyboardSprite = this.KeyboardLightAttackButton;
                this.ButtonRenderer.KeyboardScale = this.KeyboardLightAttackScale;
                break;
            case INPUT_STRONGATTACK:
                this.ButtonRenderer.ControllerSprite = this.ControllerStrongAttackButton;
                this.ButtonRenderer.KeyboardSprite = this.KeyboardStrongAttackButton;
                this.ButtonRenderer.KeyboardScale = this.KeyboardStrongAttackScale;
                break;
        }

        this.ButtonRenderer.Configure();
    }
}
