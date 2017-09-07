using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIMenuElement : MonoBehaviour
{
    public Text Text;
    public NicerOutline Outline;
    public Color UnselectedTextColor;
    public Color SelectedTextColor;
    public Color UnselectedOutlineColor;
    public Color SelectedOutlineColor;
    public int AnimDuration = 20;

    public void Configure(Menu.MenuElement element)
    {
        this.Text.text = element.Text;
        _t = 0;
        _selected = false;
        updateColor();
    }

    public void Select()
    {
        _selected = true;
    }

    public void Deselect()
    {
        _selected = false;
    }

    void FixedUpdate()
    {
        if (_selected)
        {
            if (_t < this.AnimDuration)
            {
                ++_t;
                updateColor();
            }
        }
        else
        {
            if (_t > 0)
            {
                --_t;
                updateColor();
            }
        }
    }

    /**
     * Private
     */
    private int _t;
    private bool _selected;

    private void updateColor()
    {
        this.Text.color = Color.Lerp(this.UnselectedTextColor, this.SelectedTextColor, _t);
        this.Outline.effectColor = Color.Lerp(this.UnselectedOutlineColor, this.SelectedOutlineColor, _t);
    }
}
