using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIMenuElement : UIMenuElementSpec
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
        _unhighlightedTextHSV = this.UnselectedTextColor.GetHSV();
        _highlightedTextHSV = this.SelectedTextColor.GetHSV();
        _unhighlightedOutlineHSV = this.UnselectedOutlineColor.GetHSV();
        _highlightedOutlineHSV = this.SelectedOutlineColor.GetHSV();
    }

    public override void Configure(Menu menu, Menu.MenuElement element)
    {
        if (element.Action.Type == Menu.ActionType.ChangeValue)
            this.Text.text = element.Text + StringExtensions.SPACE + OptionsValues.GetDisplaySuffix(element.Action.Param);
        else
            this.Text.text = element.Text;
        _t = 0;
        _highlighted = false;
        updateColor();
    }

    public override void Highlight()
    {
        _highlighted = true;
    }

    public override void UnHighlight()
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

    public override Menu.Action HandleCustomAction(Menu.Action action)
    {
        return new Menu.Action();
    }

    /**
     * Private
     */
    private int _t;
    private bool _highlighted;
    private ColorExtensions.HSV _unhighlightedTextHSV;
    private ColorExtensions.HSV _highlightedTextHSV;
    private ColorExtensions.HSV _unhighlightedOutlineHSV;
    private ColorExtensions.HSV _highlightedOutlineHSV;

    private void updateColor()
    {
        this.Text.color = _unhighlightedTextHSV.LerpTo(_highlightedTextHSV, _t, this.AnimDuration).RGBColor;
        this.Outline.effectColor = _unhighlightedOutlineHSV.LerpTo(_highlightedOutlineHSV, _t, this.AnimDuration).RGBColor;
    }
}
