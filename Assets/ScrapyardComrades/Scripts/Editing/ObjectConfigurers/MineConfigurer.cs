using UnityEngine;

public class MineConfigurer : ObjectConfigurer
{
    private const string NAME = "Mine";

    public Mine MineScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(TurretConfigurer.ATTACH_DIR_TYPE, new string[] {
                    TurretConfigurer.ATTACH_DOWN,
                    TurretConfigurer.ATTACH_UP,
                    TurretConfigurer.ATTACH_LEFT,
                    TurretConfigurer.ATTACH_RIGHT
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
            case TurretConfigurer.ATTACH_DIR_TYPE:
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
                LogInvalidParameter(NAME, TurretConfigurer.ATTACH_DIR_TYPE, option);
                this.MineScript.AttachedAt = TurretController.AttachDir.Down;
                break;
            case TurretConfigurer.ATTACH_DOWN:
                this.MineScript.AttachedAt = TurretController.AttachDir.Down;
                break;
            case TurretConfigurer.ATTACH_UP:
                this.MineScript.AttachedAt = TurretController.AttachDir.Up;
                break;
            case TurretConfigurer.ATTACH_LEFT:
                this.MineScript.AttachedAt = TurretController.AttachDir.Left;
                break;
            case TurretConfigurer.ATTACH_RIGHT:
                this.MineScript.AttachedAt = TurretController.AttachDir.Right;
                break;
        }
    }
}
