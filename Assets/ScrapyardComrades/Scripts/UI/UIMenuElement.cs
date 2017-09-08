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

    void Awake()
    {
        float h, s, v;
        Color.RGBToHSV(this.UnselectedTextColor, out h, out s, out v);
        _unhighlightedTextV = v;
        Color.RGBToHSV(this.SelectedTextColor, out h, out s, out v);
        _highlightedTextV = v;

        Color.RGBToHSV(this.UnselectedOutlineColor, out h, out s, out v);
        _unhighlightedOutlineV = v;
        Color.RGBToHSV(this.SelectedOutlineColor, out h, out s, out v);
        _highlightedOutlineV = v;
    }

    public void Configure(Menu.MenuElement element)
    {
        this.Text.text = element.Text;
        _t = 0;
        _highlighted = false;
        updateColor();
    }

    public void Highlight()
    {
        _highlighted = true;
    }

    public void UnHighlight()
    {
        _highlighted = false;
    }

    void FixedUpdate()
    {
        if (_highlighted)
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
    private bool _highlighted;
    private float _unhighlightedTextV;
    private float _highlightedTextV;
    private float _unhighlightedOutlineV;
    private float _highlightedOutlineV;

    private void updateColor()
    {
        float textV = Easing.Linear(_t, _unhighlightedTextV, _highlightedTextV - _unhighlightedTextV, this.AnimDuration);
        float outlineV = Easing.Linear(_t, _unhighlightedOutlineV, _highlightedOutlineV - _unhighlightedOutlineV, this.AnimDuration);
        
        float h, s, v;
        Color.RGBToHSV(this.Text.color, out h, out s, out v);
        this.Text.color = Color.HSVToRGB(h, s, textV);
        
        Color.RGBToHSV(this.Outline.effectColor, out h, out s, out v);
        this.Outline.effectColor = Color.HSVToRGB(h, s, outlineV);
    }
}
