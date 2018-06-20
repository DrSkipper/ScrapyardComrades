using UnityEditor;
using UnityEngine;

public static class SCCustomObjectCreator
{
    public const string PATH = "Assets/ScrapyardComrades/CustomObjects/";
    public const string ASSET_SUFFIX = ".asset";
    public const string ANIMATION_PATH = "Animations/";
    private const string ANIMATION_NAME = "NewSpriteAnimation.asset";
    private const string ATTACK_PATH = "Attacks/NewAttack.asset";
    private const string MOVESET_PATH = "Attacks/NewMoveSet.asset";
    private const string PICKUP_PATH = "Interactables/Pickups/NewPickup.asset";
    private const string CONSUMABLE_PATH = "Interactables/Consumables/NewConsumable.asset";
    private const string DIALOG_PATH = "Interactables/Dialog/NewDialog.asset";
    private const string TILESET_DATA_PATH = "Tilesets/NewTileset.asset";
    private const string TILESET_COLLECTION_PATH = "TilesetCollections/NewTilesetCollection.asset";
    private const string PREFAB_COLLECTION_PATH = "PrefabCollections/NewPrefabCollection.asset";
    private const string PACKED_SPRITE_GROUP_PATH = "NewPackedSpriteGroup.asset";
    private const string HERO_PROGRESSION_DATA_PATH = "ProgressionData/NewHeroProgressionData.asset";
    private const string ANIMATION_COLLECTION_PATH = "PrefabCollections/NewSpriteAnimationCollection.asset";
    private const string POWERUP_STATE_PATH = "PowerupData/States/NewPowerupState.asset";
    private const string POWERUP_TIERS_PATH = "PowerupData/Tiers/NewPowerupTiers.asset";
    private const string POWERUP_LEVELS_PATH = "PowerupData/Levels/NewPowerupLevels.asset";
    private const string SOUND_DATA_PATH = "SoundData/NewSoundData.asset";
    private const string REGION_DATA_PATH = "RegionData/NewRegionData.asset";

    [MenuItem("Custom Objects/Create Sprite Animation")]
    public static void CreateSpriteAnimation()
    {
        SaveAsset(SCSpriteAnimation.CreateInstance<SCSpriteAnimation>(), PATH + ANIMATION_PATH + ANIMATION_NAME);
    }

    [MenuItem("Custom Objects/Create Attack")]
    public static void CreateAttack()
    {
        SaveAsset(SCAttack.CreateInstance<SCAttack>(), PATH + ATTACK_PATH);
    }

    [MenuItem("Custom Objects/Create Move Set")]
    public static void CreateMoveSet()
    {
        SaveAsset(SCMoveSet.CreateInstance<SCMoveSet>(), PATH + MOVESET_PATH);
    }

    [MenuItem("Custom Objects/Create Pickup")]
    public static void CreatePickup()
    {
        SaveAsset(SCPickup.CreateInstance<SCPickup>(), PATH + PICKUP_PATH);
    }

    [MenuItem("Custom Objects/Create Consumable")]
    public static void CreateConsumable()
    {
        SaveAsset(SCConsumable.CreateInstance<SCConsumable>(), PATH + CONSUMABLE_PATH);
    }

    [MenuItem("Custom Objects/Create Dialog")]
    public static void CreateDialog()
    {
        SaveAsset(SCDialog.CreateInstance<SCDialog>(), PATH + DIALOG_PATH);
    }

    [MenuItem("Custom Objects/Create Tileset")]
    public static void CreateTileset()
    {
        SaveAsset(TilesetData.CreateInstance<TilesetData>(), PATH + TILESET_DATA_PATH);
    }

    [MenuItem("Custom Objects/Create Tileset Collection")]
    public static void CreateTilesetCollection()
    {
        SaveAsset(TilesetCollection.CreateInstance<TilesetCollection>(), PATH + TILESET_COLLECTION_PATH);
    }

    [MenuItem("Custom Objects/Create Prefab Collection")]
    public static void CreatePrefabCollection()
    {
        SaveAsset(PrefabCollection.CreateInstance<PrefabCollection>(), PATH + PREFAB_COLLECTION_PATH);
    }

    [MenuItem("Custom Objects/Packed Sprite Groups/Create Packed Sprite Group")]
    public static void CreatePackedSpriteGroup()
    {
        SaveAsset(PackedSpriteGroup.CreateInstance<PackedSpriteGroup>(), PATH + PACKED_SPRITE_GROUP_PATH);
    }

    [MenuItem("Custom Objects/Create Hero Progression Data")]
    public static void CreateHeroProgressionData()
    {
        SaveAsset(HeroProgressionData.CreateInstance<HeroProgressionData>(), PATH + HERO_PROGRESSION_DATA_PATH);
    }

    [MenuItem("Custom Objects/Create Sprite Animation Collection")]
    public static void CreateSpriteAnimationCollection()
    {
        SaveAsset(SpriteAnimationCollection.CreateInstance<SpriteAnimationCollection>(), PATH + ANIMATION_COLLECTION_PATH);
    }

    [MenuItem("Custom Objects/Create Powerup State")]
    public static void CreatePowerupState()
    {
        SaveAsset(PowerupState.CreateInstance<PowerupState>(), PATH + POWERUP_STATE_PATH);
    }

    [MenuItem("Custom Objects/Create Powerup Tiers")]
    public static void CreatePowerupTiers()
    {
        SaveAsset(PowerupTiers.CreateInstance<PowerupTiers>(), PATH + POWERUP_TIERS_PATH);
    }

    [MenuItem("Custom Objects/Create Powerup Levels")]
    public static void CreatePowerupLevels()
    {
        SaveAsset(PowerupLevels.CreateInstance<PowerupLevels>(), PATH + POWERUP_LEVELS_PATH);
    }

    [MenuItem("Custom Objects/Create Sound Data")]
    public static void CreateSoundData()
    {
        SaveAsset(SoundData.CreateInstance<SoundData>(), PATH + SOUND_DATA_PATH);
    }

    [MenuItem("Custom Objects/Create Region Data")]
    public static void CreateRegionData()
    {
        SaveAsset(RegionDataCollection.CreateInstance<RegionDataCollection>(), PATH + REGION_DATA_PATH);
    }

    public static void SaveAsset(ScriptableObject asset, string path)
    {
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
