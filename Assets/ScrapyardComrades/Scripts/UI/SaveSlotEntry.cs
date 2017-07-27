using UnityEngine;
using UnityEngine.UI;

public class SaveSlotEntry : MonoBehaviour
{
    public Text SlotNameText;
    public Image Image;
    public Color UnsafeSlotColor = Color.red;

    public void Configure(SaveSlotData.SlotSummary slotSummary)
    {
        this.SlotNameText.text = StringExtensions.IsEmpty(slotSummary.Name) ? "_MISSING_SLOT_" : slotSummary.Name;
        if (slotSummary.UnsafeSave)
            this.Image.color = this.UnsafeSlotColor;
    }

    public void ConfigureForEmpty()
    {
        this.SlotNameText.text = "Start New Game";
    }
}
