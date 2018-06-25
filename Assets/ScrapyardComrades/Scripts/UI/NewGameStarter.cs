using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameStarter : MonoBehaviour
{
    public string NewGameEvent = "NEW_GAME_MENU";
    public string ReturnToMenuEvent = "BACK_TO_MAIN";
    //public string SceneEndEvent = "SCENE_END";
    public string SceneDestination = "IntroScene";

    public Menu MainMenu;
    public int NewGameMenuIndex = 0;

    void Awake()
    {
        _defaultAction = this.MainMenu.Elements[this.NewGameMenuIndex].Action;
        _emptyAction = new Menu.Action();
        _emptyAction.Type = Menu.ActionType.None;

        updateMenu(null);
        GlobalEvents.Notifier.Listen(this.ReturnToMenuEvent, this, updateMenu);
        GlobalEvents.Notifier.Listen(this.NewGameEvent, this, onNewGame);
    }

    public static void CreateNewSlot()
    {
        SaveData.LoadFromDisk(SaveSlotData.CreateNewSlotName(SaveSlotData.GetAllSlots()));
    }

    /**
     * Private
     */
    private Menu.Action _defaultAction;
    private Menu.Action _emptyAction;

    private void onNewGame(LocalEventNotifier.Event e)
    {
        CreateNewSlot();
        SceneManager.LoadScene(this.SceneDestination, LoadSceneMode.Single);
    }

    private void updateMenu(LocalEventNotifier.Event e)
    {
        if (DiskDataHandler.GetAllFilesAtPath(SaveData.DATA_PATH).Length >= SaveSlotData.MAX_SLOTS)
        {
            this.MainMenu.Elements[this.NewGameMenuIndex].Action = _emptyAction;
        }
        else
        {
            this.MainMenu.Elements[this.NewGameMenuIndex].Action = _defaultAction;
        }
    }
}
