using UnityEngine;
using UnityEngine.UI;

public class UIVersionRenderer : MonoBehaviour
{
    public Text Text;

    void Start()
    {
        this.Text.text = VersionNumber.FullVersionString;
    }
}
