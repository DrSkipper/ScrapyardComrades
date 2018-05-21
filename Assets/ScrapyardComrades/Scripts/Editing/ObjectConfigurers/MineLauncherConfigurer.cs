using UnityEngine;

public class MineLauncherConfigurer : ObjectConfigurer
{
    private const string NAME = "MineLauncher";
    private const string IS_LAUNCHING = "is_launching";
    private const string MINE_ATTACH_DIR_TYPE = "mine_dir";
    private const string LAUNCH_SPEED = "launch_speed";
    private const string SLOW = "1";
    private const string NORMAL = "2";
    private const string FAST = "3";
    private const string LAUNCH_INTERVAL = "interval";
    private const string SMALL = "30";
    private const string MEDIUM = "50";
    private const string LONG = "100";
    private const string VERY_LONG = "150";


    public MineLauncher MineScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(IS_LAUNCHING, new string[] {
                    SwitchConfigurer.TRUE,
                    SwitchConfigurer.FALSE
                }),
                new ObjectParamType(TurretConfigurer.ATTACH_DIR_TYPE, new string[] {
                    TurretConfigurer.ATTACH_LEFT,
                    TurretConfigurer.ATTACH_RIGHT,
                    TurretConfigurer.ATTACH_DOWN,
                    TurretConfigurer.ATTACH_UP
                }),
                new ObjectParamType(MINE_ATTACH_DIR_TYPE, new string[] {
                    TurretConfigurer.ATTACH_DOWN,
                    TurretConfigurer.ATTACH_UP,
                    TurretConfigurer.ATTACH_LEFT,
                    TurretConfigurer.ATTACH_RIGHT
                }),
                new ObjectParamType(LAUNCH_SPEED, new string[] {
                    NORMAL,
                    FAST,
                    SLOW
                }),
                new ObjectParamType(LAUNCH_INTERVAL, new string[] {
                    MEDIUM,
                    LONG,
                    VERY_LONG,
                    SMALL
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
            case IS_LAUNCHING:
                configureIsLaunching(option);
                break;
            case TurretConfigurer.ATTACH_DIR_TYPE:
                configureAttachType(option);
                break;
            case MINE_ATTACH_DIR_TYPE:
                configureMineDir(option);
                break;
            case LAUNCH_SPEED:
                configureLaunchSpeed(option);
                break;
            case LAUNCH_INTERVAL:
                configureLaunchInterval(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureIsLaunching(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, IS_LAUNCHING, option);
                this.MineScript.IsLaunching = true;
                break;
            case SwitchConfigurer.TRUE:
                this.MineScript.IsLaunching = true;
                break;
            case SwitchConfigurer.FALSE:
                this.MineScript.IsLaunching = false;
                break;
        }
    }

    private void configureAttachType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, TurretConfigurer.ATTACH_DIR_TYPE, option);
                this.MineScript.AttachedAt = TurretController.AttachDir.Left;
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

    private void configureMineDir(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, MINE_ATTACH_DIR_TYPE, option);
                this.MineScript.MineAttachDir = TurretController.AttachDir.Down;
                break;
            case TurretConfigurer.ATTACH_DOWN:
                this.MineScript.MineAttachDir = TurretController.AttachDir.Down;
                break;
            case TurretConfigurer.ATTACH_UP:
                this.MineScript.MineAttachDir = TurretController.AttachDir.Up;
                break;
            case TurretConfigurer.ATTACH_LEFT:
                this.MineScript.MineAttachDir = TurretController.AttachDir.Left;
                break;
            case TurretConfigurer.ATTACH_RIGHT:
                this.MineScript.MineAttachDir = TurretController.AttachDir.Right;
                break;
        }
    }

    private void configureLaunchSpeed(string option)
    {
        int result;
        bool wasInt = int.TryParse(option, out result);

        if (wasInt)
            this.MineScript.LaunchSpeed = result;
        else
            LogInvalidParameter(NAME, LAUNCH_SPEED, option);
    }


    private void configureLaunchInterval(string option)
    {
        int result;
        bool wasInt = int.TryParse(option, out result);

        if (wasInt)
            this.MineScript.LaunchInterval = result;
        else
            LogInvalidParameter(NAME, LAUNCH_INTERVAL, option);
    }
}
