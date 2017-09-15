using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneHotkeys
{
    [MenuItem("Scenes/Load Main Menu %1")]
    public static void LoadMainMenu() { openScene(MAIN_MENU); }

    [MenuItem("Scenes/Load World Editor %2")]
    public static void LoadWorldEditor() { openScene(WORLD_EDITOR); }

    [MenuItem("Scenes/Load Level Editor %3")]
    public static void LoadLevelEditor() { openScene(LEVEL_EDITOR); }

    [MenuItem("Scenes/Load Gameplay %4")]
    public static void LoadGameplay() { openScene(GAMEPLAY); }

    [MenuItem("Scenes/Load Intro %5")]
    public static void LoadIntro() { openScene(INTRO_SCENE); }

    [MenuItem("Scenes/Load Character Editor %6")]
    public static void LoadCharacterEditor() { openScene(CHARACTER_EDITOR); }

    [MenuItem("Scenes/Load TilesetEditor %7")]
    public static void LoadTilesetEditor() { openScene(TILESET_EDITOR); }

    [MenuItem("Scenes/Load End Demo Screen %8")]
    public static void LoadEndDemoScreen() { openScene(END_DEMO); }

    [MenuItem("Custom Objects/Wipe Player Prefs")]
    public static void WipePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }


    /**
     * Private
     */
    private const string PATH = "Assets/ScrapyardComrades/Scenes/";
    private const string MAIN_MENU = "Runtime/MainMenu";
    private const string WORLD_EDITOR = "Runtime/WorldEditor";
    private const string LEVEL_EDITOR = "Runtime/LevelEditing";
    private const string GAMEPLAY = "Runtime/Gameplay";
    private const string INTRO_SCENE = "Runtime/IntroScene";
    private const string SAVE_SLOTS = "Runtime/SaveSlots";
    private const string CHARACTER_EDITOR = "EditorOnly/CharacterEditing";
    private const string TILESET_EDITOR = "EditorOnly/TilesetEditor";
    private const string END_DEMO = "Runtime/EndDemoScene";
    private const string SCENE_SUFFIX = ".unity";

    private static void openScene(string scene)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(PATH + scene + SCENE_SUFFIX, OpenSceneMode.Single);
    }
}
