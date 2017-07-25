using UnityEngine;

public class DoorConfigurer : ObjectConfigurer
{
    public const string NAME = "Door";
    public const string DOOR_TYPE = "type";
    public const string RED_DOOR = "red";
    public const string PURP_DOOR = "purp";
    public const string BLUE_DOOR = "blue";

    public SCSpriteAnimation RedOpenAnimation;
    public SCSpriteAnimation RedCloseAnimation;
    public SCSpriteAnimation PurpOpenAnimation;
    public SCSpriteAnimation PurpCloseAnimation;
    public SCSpriteAnimation BlueOpenAnimation;
    public SCSpriteAnimation BlueCloseAnimation;
    public Door DoorScript;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            return new ObjectParamType[] {
                new ObjectParamType(DOOR_TYPE, new string[] {
                    RED_DOOR,
                    PURP_DOOR,
                    BLUE_DOOR
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
            case DOOR_TYPE:
                configureDoorType(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureDoorType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, DOOR_TYPE, option);
                this.DoorScript.LockType = SCPickup.KeyType.None;
                break;
            case RED_DOOR:
                this.DoorScript.OpenAnimation = this.RedOpenAnimation;
                this.DoorScript.CloseAnimation = this.RedCloseAnimation;
                this.DoorScript.LockType = SCPickup.KeyType.Red;
                break;
            case PURP_DOOR:
                this.DoorScript.OpenAnimation = this.PurpOpenAnimation;
                this.DoorScript.CloseAnimation = this.PurpCloseAnimation;
                this.DoorScript.LockType = SCPickup.KeyType.Purple;
                break;
            case BLUE_DOOR:
                this.DoorScript.OpenAnimation = this.BlueOpenAnimation;
                this.DoorScript.CloseAnimation = this.BlueCloseAnimation;
                this.DoorScript.LockType = SCPickup.KeyType.Blue;
                break;
        }
    }
}
