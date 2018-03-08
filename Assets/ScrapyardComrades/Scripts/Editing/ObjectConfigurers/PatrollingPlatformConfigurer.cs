using UnityEngine;
using System.Linq;

public class PatrollingPlatformConfigurer : ObjectConfigurer
{
    private const string NAME = "PatrollingPlatform";
    private const string WIDTH = "width";
    private const string HEIGHT = "height";
    private const string SPEED = "speed";
    private const string ONE = "1";
    private const string TWO = "2";
    private const string THREE = "3";
    private const string FOUR = "4";
    private const string FIVE = "5";
    private const string SIX = "6";
    public const int MAX_SIZE = 6;
    private const string TILESET = "tileset";
    public Transform DestinationLocation;
    public TilesetCollection TilesetCollection;
    public PatrollingPlatform PlatformScript;

    public override Transform GetSecondaryTransform()
    {
        return this.DestinationLocation;
    }

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            string[] sizes = new string[] { ONE, TWO, THREE, FOUR, FIVE, SIX };
            return new ObjectParamType[] {
                new ObjectParamType(WIDTH, sizes),
                new ObjectParamType(HEIGHT, sizes),
                new ObjectParamType(TILESET, this.TilesetCollection.Tilesets.Select(x => x.name).ToArray()),
                new ObjectParamType(SPEED, sizes)
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
            case WIDTH:
                configureWidth(option);
                break;
            case HEIGHT:
                configureHeight(option);
                break;
            case TILESET:
                configureTileset(option);
                break;
            case SPEED:
                configureSpeed(option);
                break;
        }
    }

    /**
     * Private
     */
    private void configureWidth(string option)
    {
        int value;
        if (int.TryParse(option, out value))
        {
            this.PlatformScript.Width = value;
        }
        else
        {
            LogInvalidParameter(NAME, WIDTH, option);
        }
    }

    private void configureHeight(string option)
    {
        int value;
        if (int.TryParse(option, out value))
        {
            this.PlatformScript.Height = value;
        }
        else
        {
            LogInvalidParameter(NAME, HEIGHT, option);
        }
    }

    private void configureTileset(string option)
    {
        for (int i = 0; i < this.TilesetCollection.Tilesets.Length; ++i)
        {
            TilesetData tileset = this.TilesetCollection.Tilesets[i];
            if (tileset.name == option)
            {
                this.PlatformScript.Tileset = tileset;
                return;
            }
        }

        LogInvalidParameter(NAME, TILESET, option);
    }

    private void configureSpeed(string option)
    {
        int value;
        if (int.TryParse(option, out value))
        {
            this.PlatformScript.Speed = value;
        }
        else
        {
            LogInvalidParameter(NAME, SPEED, option);
        }
    }
}
