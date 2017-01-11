using UnityEditor;
using UnityEngine;

public static class SCCustomObjectCreator
{
    private const string PATH = "Assets/ScrapyardComrades/";
    private const string ANIMATION_PATH = "Animations/NewSpriteAnimation.asset";
    private const string ATTACK_PATH = "Attacks/NewAttack.asset";
    private const string MOVESET_PATH = "Attacks/NewMoveSet.asset";
    private const string PARALLAX_PATH = "Parallax/NewParallaxLayer.asset";
    private const string PICKUP_PATH = "Pickups/NewPickup.asset";
    private const string CONSUMABLE_PATH = "Consumables/NewConsumable.asset";
    private const string DIALOG_PATH = "Dialog/NewDialog.asset";

    [MenuItem("Custom Objects/Create Sprite Animation")]
    public static void CreateSpriteAnimation()
    {
        SaveAsset(new SCSpriteAnimation(), PATH + ANIMATION_PATH);
    }

    [MenuItem("Custom Objects/Create Attack")]
    public static void CreateAttack()
    {
        SaveAsset(new SCAttack(), PATH + ATTACK_PATH);
    }

    [MenuItem("Custom Objects/Create Move Set")]
    public static void CreateMoveSet()
    {
        SaveAsset(new SCMoveSet(), PATH + MOVESET_PATH);
    }

    [MenuItem("Custom Objects/Create Parallax Layer")]
    public static void CreateParallaxLayer()
    {
        SaveAsset(new SCParallaxLayer(), PATH + PARALLAX_PATH);
    }

    [MenuItem("Custom Objects/Create Pickup")]
    public static void CreatePickup()
    {
        SaveAsset(new SCPickup(), PATH + PICKUP_PATH);
    }

    [MenuItem("Custom Objects/Create Consumable")]
    public static void CreateConsumable()
    {
        SaveAsset(new SCConsumable(), PATH + CONSUMABLE_PATH);
    }

    [MenuItem("Custom Objects/Create Dialog")]
    public static void CreateDialog()
    {
        SaveAsset(new SCDialog(), PATH + DIALOG_PATH);
    }

    public static void SaveAsset(ScriptableObject asset, string path)
    {
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
