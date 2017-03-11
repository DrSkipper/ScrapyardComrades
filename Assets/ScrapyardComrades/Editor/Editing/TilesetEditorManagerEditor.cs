using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilesetEditorManager))]
public class TilesetEditorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TilesetEditorManager behavior = this.target as TilesetEditorManager;

        if (behavior.TilesetToEdit != behavior.CurrentEditingTileset)
        {
            behavior.CurrentEditingTileset = behavior.TilesetToEdit;
            behavior.Reload();
            AssetDatabase.SaveAssets();
        }

        if (behavior.SelectedSprite != null)
        {
            EditorGUILayout.Separator();
            TilesetData.TileType tileType = (TilesetData.TileType)EditorGUILayout.EnumPopup("Tile Type", behavior.SelectedSpriteData.Type);
            bool autoTile = EditorGUILayout.Toggle("Allow AutoTile", behavior.SelectedSpriteData.AllowAutotile);
            EditorGUILayout.Separator();
            if (behavior.ApplySpriteData(tileType, autoTile))
                AssetDatabase.SaveAssets();
        }
    }

    void OnSceneGUI()
    {
        Event current = Event.current;

        if (current.type == EventType.layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else if (current.type == EventType.mouseDown && current.button == 0)
        {
            current.Use();
            TilesetEditorManager behavior = this.target as TilesetEditorManager;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Selection.activeGameObject = behavior.gameObject;

                Texture2D texture = behavior.Texture;
                Vector2 texCoord = hit.textureCoord;
                texCoord.x *= texture.width;
                texCoord.y *= texture.height;
                behavior.SelectSpriteAtPixel(texCoord);
            }
        }
    }
}
