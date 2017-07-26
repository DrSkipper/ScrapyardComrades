using UnityEngine;
using UnityEngine.UI;

public class SaveSlotEntry : MonoBehaviour
{
    public Text SlotNameText;

    public void Configure(SaveSlotData.SlotSummary slotSummary)
    {
        this.SlotNameText.text = StringExtensions.IsEmpty(slotSummary.Name) ? "_MISSING_SLOT_" : slotSummary.Name;
    }

    public void ConfigureForEmpty()
    {
        this.SlotNameText.text = "Start New Game";
    }
}
