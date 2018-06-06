using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/**
 * SpriteAnimationCreator:
 * Handles automatic slicing of animations, and saving that sliced data into an SCSpriteAnimation asset that is saved to the given location.
 */
public class SpriteAnimationCreator : EditorWindow
{
    private string _path = "Characters";
    private string _folder = "NewCharacter";
    private Texture2D _texture;
    private bool _overrideSpritesheet = false;
    private int _spriteWidth = 32;
    private int _spriteHeight = 32;
    private int _framesPerFrame = 3;
    private bool _loops = false;
    private int _loopFrame = 0;

    [MenuItem("Window/Sprite Animation Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SpriteAnimationCreator));
    }

    void OnGUI()
    {
        // Gather parameters
        _path = EditorGUILayout.TextField("Parent Folder Path", _path);
        _folder = EditorGUILayout.TextField("Animation Folder Name", _folder);
        _texture = (Texture2D)EditorGUILayout.ObjectField("Animation Texture", _texture, typeof(Texture2D), false);
        _overrideSpritesheet = EditorGUILayout.Toggle("Override Spritesheet", _overrideSpritesheet);
        _spriteWidth = EditorGUILayout.IntField("Sprite Width", _spriteWidth);
        _spriteHeight = EditorGUILayout.IntField("Sprite Height", _spriteHeight);
        _framesPerFrame = EditorGUILayout.IntField("Game frames per visual frame", _framesPerFrame);
        _loops = EditorGUILayout.Toggle("Loops By Default", _loops);
        _loopFrame = EditorGUILayout.IntField("Loop Frame", _loopFrame);

        // Check if user says it's time to generate the asset
        if (GUILayout.Button("Generate Sprite Animation") && _texture != null)
        {
            // Slice the sprites in the texture data
            DivideSpritesheet(_texture, _spriteWidth, _spriteHeight, _overrideSpritesheet);

            // Create the animation asset and set its parameters
            SCSpriteAnimation animation = SCSpriteAnimation.CreateInstance<SCSpriteAnimation>();
            animation.name = _texture.name;
            animation.Frames = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_texture)).OfType<Sprite>().ToArray();
            animation.LengthInFrames = animation.Frames.Length * _framesPerFrame;
            animation.LoopsByDefault = _loops;
            animation.LoopFrame = _loopFrame;

            // Create the folder for the asset if it doesn't yet exist
            string parentFolderPath = SCCustomObjectCreator.PATH + SCCustomObjectCreator.ANIMATION_PATH + _path;
            if (!AssetDatabase.IsValidFolder(parentFolderPath + StringExtensions.SLASH + _folder))
            {
                AssetDatabase.CreateFolder(parentFolderPath, _folder);
                AssetDatabase.SaveAssets();
            }

            // Delete any already existing asset at this location
            string fullAssetPath = parentFolderPath + StringExtensions.SLASH + _folder + StringExtensions.SLASH + _texture.name + SCCustomObjectCreator.ASSET_SUFFIX;

            if (AssetDatabase.DeleteAsset(fullAssetPath))
                AssetDatabase.SaveAssets();

            // Save the asset
            SCCustomObjectCreator.SaveAsset(animation, fullAssetPath);
        }
    }

    /**
     * DivideSpritesheet:
     * Slices out sprites from 'texture' starting from its upper-left corner, of size 'spriteWidth' x 'spriteHeight'. Empty sprites are skipped.
     * 
     * If 'overrideSpritesheet' is specified, existing sprite data will be overwritten, otherwise it will be kept if it already exists.
     */
    public static List<SpriteMetaData> DivideSpritesheet(Texture2D texture, int spriteWidth, int spriteHeight, bool overrideSpritesheet)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        ti.isReadable = true;

        if (overrideSpritesheet)
        {
            if (ti.spriteImportMode == SpriteImportMode.Multiple)
            {
                // Bug? Need to convert to single then back to multiple in order to make changes when it's already sliced
                ti.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
        else if (ti.spriteImportMode == SpriteImportMode.Multiple)
        {
            // If we don't want to override the existing spritesheet on the texture, just return the existing one if it already exists
            SpriteMetaData[] spritesheet = ti.spritesheet;
            if (spritesheet != null && spritesheet.Length > 0)
                return new List<SpriteMetaData>();
        }

        ti.spriteImportMode = SpriteImportMode.Multiple;
        List<SpriteMetaData> spriteData = new List<SpriteMetaData>();

        int count = 0;
        for (int j = texture.height; j >= spriteHeight; j -= spriteHeight)
        {
            for (int i = 0; i <= texture.width - spriteWidth; i += spriteWidth)
            {
                Rect region = new Rect(i, j - spriteHeight, spriteWidth, spriteHeight);

                if (!IsEmptyTextureRegion(texture, region))
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.name = texture.name + StringExtensions.UNDERSCORE + count;
                    smd.pivot = new Vector2(0.5f, 0.5f);
                    smd.alignment = 9;
                    smd.rect = region;

                    spriteData.Add(smd);
                    ++count;
                }
            }
        }

        ti.spritesheet = spriteData.ToArray();
        ti.isReadable = true;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return spriteData;
    }

    /**
     * IsEmptyTextureRegion:
     * Returns true if the given region within the given texture is empty, i.e. completely alpha'd to 0.
     */
    public static bool IsEmptyTextureRegion(Texture2D texture, Rect rect)
    {
        Color[] pixels = texture.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));

        for (int i = 0; i < pixels.Length; ++i)
        {
            if (pixels[i].a != 0)
                return false;
        }
        return true;
    }
}
