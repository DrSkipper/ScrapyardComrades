using UnityEngine;

public class ContextMenuElement : VoBehavior
{
    public string[] ValidStates;

    public bool IsValidForState(string state)
    {
        if (this.ValidStates == null)
            return false;
        for (int i = 0; i < this.ValidStates.Length; ++i)
        {
            if (this.ValidStates[i] == state)
                return true;
        }
        return false;
    }
}
