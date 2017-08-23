using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneHotkeys
{
    [MenuItem("Scenes/Load Main Menu _F1")]
    public static void LoadMainMenu() { openScene(MAIN_MENU); }

    [MenuItem("Scenes/Load World Editor _F2")]
    public static void LoadWorldEditor() { openScene(WORLD_EDITOR); }

    [MenuItem("Scenes/Load Level Editor _F3")]
    public static void LoadLevelEditor() { openScene(LEVEL_EDITOR); }

    [MenuItem("Scenes/Load Gameplay _F4")]
    public static void LoadGameplay() { openScene(GAMEPLAY); }

    [MenuItem("Scenes/Load Intro _F5")]
    public static void LoadIntro() { openScene(INTRO_SCENE); }

    [MenuItem("Scenes/Load Save Slots _F6")]
    public static void LoadSaveSlots() { openScene(SAVE_SLOTS); }

    [MenuItem("Scenes/Load Character Editor _F7")]
    public static void LoadCharacterEditor() { openScene(CHARACTER_EDITOR); }

    [MenuItem("Scenes/Load TilesetEditor _F8")]
    public static void LoadTilesetEditor() { openScene(TILESET_EDITOR); }

    [MenuItem("Scenes/Load End Demo Screen _F9")]
    public static void LoadEndDemoScreen() { openScene(END_DEMO); }


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
        EditorSceneManager.OpenScene(PATH + scene + SCENE_SUFFIX, OpenSceneMode.Single);
    }
}
