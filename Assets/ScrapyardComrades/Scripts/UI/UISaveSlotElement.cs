using UnityEngine;
using UnityEngine.UI;

public class UISaveSlotElement : UIMenuElementSpec
{
    public Text SlotNameText;
    public Image SlotImage;
    public Color UnsafeSlotColor = Color.red;
    public Color SafeSlotColor = Color.white;
    public Color EmptySlotColor = Color.white;
    public Color EraseSlotColor = Color.yellow;
    public string GameplayScene = "Gameplay";
    public string IntroScene = "IntroScene";
    public Sprite EmptySlotSprite;
    public Sprite SafeSlotSprite;
    public Sprite UnsafeSlotSprite;

    public override void Configure(Menu menu, Menu.MenuElement element)
    {
        _saveSlotMenu = menu as UISaveSlotMenu;

        if (element.Action.Param == UISaveSlotMenu.EMPTY_SLOT || element.Action.Param == UISaveSlotMenu.ERASE)
        {
            this.SlotNameText.text = element.Text;
            this.SlotNameText.color = element.Action.Param == UISaveSlotMenu.ERASE ? this.EraseSlotColor : this.EmptySlotColor;
            if (element.Action.Param == UISaveSlotMenu.EMPTY_SLOT)
                this.SlotImage.sprite = this.EmptySlotSprite;
        }
        else
        {
            _slotSummary = _saveSlotMenu.SaveSlotForName(element.Action.Param);
            this.SlotNameText.text = element.Text;
            if (_slotSummary.UnsafeSave)
            {
                this.SlotNameText.color = this.UnsafeSlotColor;
                this.SlotImage.sprite = this.UnsafeSlotSprite;
            }
            else
            {
                this.SlotNameText.color = this.SafeSlotColor;
                this.SlotImage.sprite = this.SafeSlotSprite;
            }
        }
    }

    public override void Highlight()
    {
    }

    public override void UnHighlight()
    {
    }

    public override Menu.Action HandleCustomAction(Menu.Action action)
    {
        Menu.ActionType actionType = Menu.ActionType.SceneTransition;
        string actionParam = this.GameplayScene;

        if (action.Param == UISaveSlotMenu.EMPTY_SLOT)
        {
            if (_saveSlotMenu.Mode == UISaveSlotMenu.MenuMode.Loading)
            {
                actionParam = this.IntroScene;
                _saveSlotMenu.CreateNewSlot();
            }
            else
            {
                return new Menu.Action();
            }
        }
        else if (action.Param == UISaveSlotMenu.ERASE)
        {
            actionType = Menu.ActionType.OpenMenu;
            actionParam = _saveSlotMenu.EraseMenu;
        }
        else
        {
            if (_saveSlotMenu.Mode == UISaveSlotMenu.MenuMode.Loading)
            {
                if (!_slotSummary.UnsafeSave)
                {
                    SaveData.LoadFromDisk(_slotSummary.Name);
                    SaveData.UnsafeSave = true;
                    SaveData.SaveToDisk();
                }
                else
                {
                    return new Menu.Action();
                }
            }
            else // Erasing
            {
                SaveSlotData.EraseSlot(_slotSummary.Name);
                actionType = Menu.ActionType.Reload;
                actionParam = StringExtensions.EMPTY;
            }
        }

        Menu.Action retVal = new Menu.Action();
        retVal.Type = actionType;
        retVal.Param = actionParam;
        return retVal;
    }

    /**
     * Private
     */
    private UISaveSlotMenu _saveSlotMenu;
    private SaveSlotData.SlotSummary _slotSummary;
}
