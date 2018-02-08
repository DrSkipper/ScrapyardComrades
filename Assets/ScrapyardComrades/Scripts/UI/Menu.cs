using UnityEngine;

public class Menu : MonoBehaviour
{
    public string Name;
    public MenuElement[] Elements;
    public int NumElements { get { return this.Elements.Length; } }
    public int HighlightedElement { get; private set; }

    [System.Serializable]
    public enum ActionType
    {
        None,
        OpenMenu,
        SceneTransition,
        UnPause,
        ChangeValue,
        CloseMenuWithEvent,
        Custom,
        Reload
    }

    [System.Serializable]
    public struct Action
    {
        public ActionType Type;
        public string Param;
    }

    public virtual void Reload()
    {
    }

    public int HighlightNext()
    {
        this.HighlightedElement = this.HighlightedElement >= this.NumElements - 1 ? 0 : this.HighlightedElement + 1;
        return this.HighlightedElement;
    }

    public int HighlightPrev()
    {
        this.HighlightedElement = this.HighlightedElement <= 0 ? this.NumElements - 1 : this.HighlightedElement - 1;
        return this.HighlightedElement;
    }

    public Action SelectCurrent()
    {
        return this.HighlightedElement < this.NumElements && this.HighlightedElement >= 0 ? this.Elements[this.HighlightedElement].Action : new Action();
    }

    public bool CurrentElementAllowsCycling()
    {
        return this.Elements[this.HighlightedElement].AllowCycling;
    }

    [System.Serializable]
    public struct MenuElement
    {
        public string Text;
        public Action Action;
        public bool AllowCycling;
    }
}
