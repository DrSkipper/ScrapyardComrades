using UnityEditor;

public static class SCAnimationCreator
{
    private const string PATH = "Assets/ScrapyardComrades/Animations/";

    [MenuItem("Custom Objects/Create Sprite Animation")]
    public static void CreateSpriteAnimation()
    {
        SCSpriteAnimation asset = new SCSpriteAnimation();
        AssetDatabase.CreateAsset(asset, PATH + "SpriteAnimation.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
