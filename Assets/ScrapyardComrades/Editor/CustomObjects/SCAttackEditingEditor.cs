using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SCAttackEditing))]
public class SCAttackEditingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save Over Current"))
        {
            ((SCAttackEditing)this.target).SaveToCurrentIndex();
        }

        if (GUILayout.Button("Create New Keyframe"))
        {
            ((SCAttackEditing)this.target).SaveAsNewIndex();
        }

        if (GUILayout.Button("Remove Current Keyframe"))
        {
            ((SCAttackEditing)this.target).RemoveCurrent();
        }
    }
}
