using UnityEngine;
using UnityEngine.UI;

public class ObjectParamEditPanel : MonoBehaviour
{
    public GameObject SelectionIcon;
    public Text ParameterText;

    public void ShowForParam(NewMapInfo.ObjectParam parameter)
    {
        this.ParameterText.text = parameter.Name + SEPARATOR_TEXT + parameter.CurrentOption;
    }

    /**
     * Private
     */
    private const string SEPARATOR_TEXT = ": ";
}
