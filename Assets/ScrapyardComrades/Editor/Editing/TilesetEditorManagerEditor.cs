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
        }
    }

    void OnSceneGUI()
    {
        Event current = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        switch (current.type)
        {
            case EventType.mouseDown:
                TilesetEditorManager behavior = this.target as TilesetEditorManager;
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Selection.activeGameObject = behavior.gameObject;
                }

                current.Use();
                break;

            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
        }
    }
}
