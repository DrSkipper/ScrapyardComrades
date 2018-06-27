using UnityEngine;

public class NameEntryMenu : MonoBehaviour, MenuController
{
    public NameEntryElement[] Elements;

    public void Show()
    {
        _currentElement = 0;
        for (int i = 0; i < this.Elements.Length; ++i)
        {
            this.Elements[i].RemoveEntry();
        }
    }

    public void Hide()
    {
    }

    void FixedUpdate()
    {
        if (MenuInput.NavDown)
            cycleChar(-1);
        else if (MenuInput.NavUp)
            cycleChar(1);
        else if(MenuInput.NavLeft)
            cycleChar(-1);
        else if (MenuInput.NavRight)
            cycleChar(1);
        else if (MenuInput.Confirm)
            nextElement();
        else if (MenuInput.Cancel)
            prevElement();
    }

    /**
     * Private
     */
    private int _currentElement;

    private void cycleChar(int dir)
    {
        this.Elements[_currentElement].CycleEntry(dir);
    }

    private void nextElement()
    {
        if (_currentElement < this.Elements.Length - 1)
        {
            ++_currentElement;
            this.Elements[_currentElement].ShowEntry();
        }
        else
        {
            string name = StringExtensions.EMPTY;
            for (int i = 0; i < this.Elements.Length; ++i)
            {
                name += this.Elements[i].GetCurrent();
            }

            //TODO: pop up confirmation menu? or just go straight into game
        }

    }

    private void prevElement()
    {
        if (_currentElement > 0)
        {
            this.Elements[_currentElement].RemoveEntry();
            --_currentElement;
        }
        //TODO: else return to previous menu
    }
}
