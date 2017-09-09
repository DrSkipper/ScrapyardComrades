using UnityEngine;
using UnityEngine.UI;

public class UISaveSlotElement : UIMenuElementSpec
{
    public Text SlotNameText;
    public Image Image;
    public Color UnsafeSlotColor = Color.red;
    public string GameplayScene = "Gameplay";

    public override void Configure(Menu menu, Menu.MenuElement element)
    {
        _saveSlotMenu = menu as UISaveSlotMenu;

        if (element.Action.Param == UISaveSlotMenu.EMPTY_SLOT)
        {
            this.SlotNameText.text = element.Text;
        }
        else
        {
            _slotSummary = _saveSlotMenu.SaveSlotForName(element.Action.Param);
            this.SlotNameText.text = StringExtensions.IsEmpty(_slotSummary.Name) ? "_MISSING_SLOT_" : _slotSummary.Name;
            if (_slotSummary.UnsafeSave)
                this.Image.color = this.UnsafeSlotColor;
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
        if (action.Param == UISaveSlotMenu.EMPTY_SLOT)
        {
            _saveSlotMenu.CreateNewSlot();
        }
        else if (!_slotSummary.UnsafeSave && !StringExtensions.IsEmpty(_slotSummary.Name))
        {
            SaveData.LoadFromDisk(_slotSummary.Name);
            SaveData.UnsafeSave = true;
            SaveData.SaveToDisk();
        }
        else
        {
            //TODO: Allow user to erase this slot
            return new Menu.Action();
        }

        Menu.Action retVal = new Menu.Action();
        retVal.Type = Menu.ActionType.SceneTransition;
        retVal.Param = this.GameplayScene;
        return retVal;
    }

    /**
     * Private
     */
    private UISaveSlotMenu _saveSlotMenu;
    private SaveSlotData.SlotSummary _slotSummary;
}
