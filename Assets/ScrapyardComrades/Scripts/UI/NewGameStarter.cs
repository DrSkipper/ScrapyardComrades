using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameStarter : MonoBehaviour
{
    public string NewGameEvent = "NEW_GAME_MENU";
    public string SceneDestination = "IntroScene";

    void Awake()
    {
        //TODO: Disable new game option if too many save slots (need to erase some to make room)
        //string[] saveSlotNames = DiskDataHandler.GetAllFilesAtPath(SaveData.DATA_PATH);

        GlobalEvents.Notifier.Listen(this.NewGameEvent, this, onNewGame);
    }

    public static void CreateNewSlot()
    {
        SaveData.LoadFromDisk(SaveSlotData.CreateNewSlotName(SaveSlotData.GetAllSlots()));
    }

    /**
     * Private
     */
    private void onNewGame(LocalEventNotifier.Event e)
    {
        CreateNewSlot();
        SceneManager.LoadScene(this.SceneDestination, LoadSceneMode.Single);
    }
}
