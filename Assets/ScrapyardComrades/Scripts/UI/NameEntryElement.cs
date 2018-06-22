using UnityEngine;
using UnityEngine.UI;

public class NameEntryElement : MonoBehaviour
{
    public Text TextVisual;

    public void ShowEntry()
    {
        updateVisual(true);
    }

    public void RemoveEntry()
    {
        updateVisual(false);
    }

    public void CycleEntry(int dir)
    {
        _index += dir;
        if (_index >= _choices.Length)
            _index = 0;
        else if (_index < 0)
            _index = _choices.Length - 1;

        updateVisual(true);
    }

    public char GetCurrent()
    {
        return _choices[_index];
    }

    /**
     * Private
     */
    private int _index;
    private char[] _choices = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '.', ':', '\'', '!', '?', '*', '/', '_' };

    private void updateVisual(bool show)
    {
        if (show)
            this.TextVisual.text = StringExtensions.EMPTY + this.GetCurrent();
        else
            this.TextVisual.text = StringExtensions.EMPTY;
    }
}
