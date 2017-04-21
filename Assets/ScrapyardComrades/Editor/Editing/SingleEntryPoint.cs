using UnityEditor;

//Credit: http://answers.unity3d.com/questions/447701/event-for-unity-editor-pause-and-playstop-events.html
[InitializeOnLoad]
public class SingleEntryPoint
{
    static SingleEntryPoint()
    {
        //Debug.Log("SingleEntryPoint. Up and running");
        EditorPlayMode.PlayModeChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeState currentMode, PlayModeState changedMode)
    {
        //Debug.Log("play mode changed");
        if (changedMode == PlayModeState.AboutToPlay)
            PackedSpriteGroupEditor.AggregateAllAtlases();
    }
}
