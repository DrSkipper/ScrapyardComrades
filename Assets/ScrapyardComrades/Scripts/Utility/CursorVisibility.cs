using UnityEngine;

public class CursorVisibility : MonoBehaviour
{
    public bool Visible;

    void Start()
    {
        Cursor.visible = this.Visible;
        this.enabled = false;
    }
}
