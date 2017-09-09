using UnityEngine;

public abstract class UIMenuElementSpec : MonoBehaviour
{
    public abstract void Configure(Menu menu, Menu.MenuElement element);
    public abstract void Highlight();
    public abstract void UnHighlight();
    public abstract Menu.Action HandleCustomAction(Menu.Action action);
}
