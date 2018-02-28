using UnityEngine;

public abstract class ObjectConfigurer : MonoBehaviour
{
    public struct ObjectParamType
    {
        public string Name;
        public string[] Options;

        public ObjectParamType(string name, string[] options)
        {
            this.Name = name;
            this.Options = options;
        }
    }

    public abstract ObjectParamType[] ParameterTypes { get; }
    public int[] CurrentOptions { get; private set; }
    public virtual Transform GetSecondaryTransform() { return null; }

    protected abstract void ConfigureParameter(string parameterName, string option);

    public void ConfigureForParams(NewMapInfo.ObjectParam[] parameters)
    {
        ObjectParamType[] paramTypes = this.ParameterTypes;
        this.CurrentOptions = new int[paramTypes.Length];

        if (parameters != null)
        {
            for (int i = 0; i < parameters.Length; ++i)
                findCurrentOption(paramTypes, parameters[i]);
        }

        for (int i = 0; i < paramTypes.Length; ++i)
        {
            this.ConfigureParameter(paramTypes[i].Name, paramTypes[i].Options[this.CurrentOptions[i]]);
        }
    }

    public static void LogInvalidParameter(string objectName, string key, string value)
    {
        Debug.LogWarning("Unsupported object parameter: " + objectName + "(" + key + ", " + value + ")");
    }

    /**
     * Private
     */
    private static int CurrentOptionIntFromString(ObjectParamType paramType, string optionName)
    {
        for (int i = 0; i < paramType.Options.Length; ++i)
        {
            if (paramType.Options[i] == optionName)
                return i;
        }
        return 0;
    }

    private void findCurrentOption(ObjectParamType[] paramTypes, NewMapInfo.ObjectParam objectParam)
    {
        string name = objectParam.Name;
        for (int j = 0; j < paramTypes.Length; ++j)
        {
            if (name == paramTypes[j].Name)
            {
                this.CurrentOptions[j] = CurrentOptionIntFromString(paramTypes[j], objectParam.CurrentOption);
                break;
            }
        }
    }
}
