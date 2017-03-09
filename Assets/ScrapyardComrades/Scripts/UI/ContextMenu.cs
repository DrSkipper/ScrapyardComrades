using UnityEngine;
using System.Collections.Generic;

public class ContextMenu : MonoBehaviour
{
    public RectTransform Panel;
    public ContextMenuElement[] Elements;
    public int Border = 2;
    public int ElementSize = 100;
    public bool Horizontal = true;

    public void EnterState(string state)
    {
        if (state == _currentState)
            return;

        if (_currentValidElements == null)
            _currentValidElements = new List<ContextMenuElement>();
        else
            _currentValidElements.Clear();

        for (int i = 0; i < this.Elements.Length; ++i)
        {
            if (this.Elements[i].IsValidForState(state))
            {
                this.Elements[i].gameObject.SetActive(true);
                _currentValidElements.Add(this.Elements[i]);
            }
            else
            {
                this.Elements[i].gameObject.SetActive(false);
            }
        }

        int size = this.Border + _currentValidElements.Count * (this.ElementSize + this.Border);
        this.Panel.sizeDelta = this.Horizontal ? new Vector2(size, this.Panel.sizeDelta.y) : new Vector2(this.Panel.sizeDelta.x, size);

        if (this.Horizontal)
        {
            for (int i = 0; i < _currentValidElements.Count; ++i)
            {
                _currentValidElements[i].transform.SetLocalX(this.Border + i * (this.ElementSize + this.Border));
            }
        }
        else
        {
            for (int i = 0; i < _currentValidElements.Count; ++i)
            {
                _currentValidElements[i].transform.SetLocalY(this.Border + i * (this.ElementSize + this.Border));
            }
        }
    }

    /**
     * Private
     */
    private List<ContextMenuElement> _currentValidElements;
    private string _currentState;
}
