using UnityEngine;

public class FanConfigurer : ObjectConfigurer
{
    public const string NAME = "Fan";
    private const string ATTACH_DIR_TYPE = "attach";
    private const string ATTACH_DOWN = "d";
    private const string ATTACH_UP = "u";
    private const string ATTACH_LEFT = "l";
    private const string ATTACH_RIGHT = "r";

    public WindFan FanScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(ATTACH_DIR_TYPE, new string[] {
                    ATTACH_DOWN,
                    ATTACH_UP,
                    ATTACH_LEFT,
                    ATTACH_RIGHT
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
            case ATTACH_DIR_TYPE:
                configureAttachType(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureAttachType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, ATTACH_DIR_TYPE, option);
                break;
            case ATTACH_DOWN:
                this.FanScript.AttachedAt = TurretController.AttachDir.Down;
                break;
            case ATTACH_UP:
                this.FanScript.AttachedAt = TurretController.AttachDir.Up;
                break;
            case ATTACH_LEFT:
                this.FanScript.AttachedAt = TurretController.AttachDir.Left;
                break;
            case ATTACH_RIGHT:
                this.FanScript.AttachedAt = TurretController.AttachDir.Right;
                break;
        }
    }
}
