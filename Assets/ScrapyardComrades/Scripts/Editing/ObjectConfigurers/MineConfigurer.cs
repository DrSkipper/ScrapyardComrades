using UnityEngine;

public class MineConfigurer : ObjectConfigurer
{
    private const string NAME = "Mine";
    public const string ATTACH_DIR_TYPE = "attach";
    public const string ATTACH_DOWN = "d";
    public const string ATTACH_UP = "u";
    public const string ATTACH_LEFT = "l";
    public const string ATTACH_RIGHT = "r";

    public Mine MineScript;

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
                this.MineScript.AttachedAt = TurretController.AttachDir.Down;
                break;
            case ATTACH_DOWN:
                this.MineScript.AttachedAt = TurretController.AttachDir.Down;
                break;
            case ATTACH_UP:
                this.MineScript.AttachedAt = TurretController.AttachDir.Up;
                break;
            case ATTACH_LEFT:
                this.MineScript.AttachedAt = TurretController.AttachDir.Left;
                break;
            case ATTACH_RIGHT:
                this.MineScript.AttachedAt = TurretController.AttachDir.Right;
                break;
        }
    }
}
