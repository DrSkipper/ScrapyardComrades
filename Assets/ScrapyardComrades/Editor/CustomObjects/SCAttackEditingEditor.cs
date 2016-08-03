using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SCAttackEditing))]
public class SCAttackEditingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SCAttackEditing behavior = this.target as SCAttackEditing;

        _attackObjectFoldout = EditorGUILayout.Foldout(_attackObjectFoldout, "Attack Object Fields");

        if (_attackObjectFoldout)
        {
            Editor attackObjectEditor = Editor.CreateEditor(behavior.AttackObject);
            attackObjectEditor.OnInspectorGUI();
            EditorGUILayout.Separator();
        }

        List<string> options = new List<string>();
        List<int> values = new List<int>();
        for (int i = 0; i < behavior.AttackObject.HitboxKeyframes.Length; ++i)
        {
            options.Add("" + i);
            values.Add(i);
        }

        options.Add("New (" + behavior.AttackObject.HitboxKeyframes.Length + ")");
        values.Add(behavior.AttackObject.HitboxKeyframes.Length);

        int prevCurrentIndex = behavior.CurrentIndex;
        behavior.CurrentIndex = EditorGUILayout.IntPopup("Current Keyframe", behavior.CurrentIndex >= 0 && behavior.CurrentIndex < behavior.AttackObject.HitboxKeyframes.Length ? behavior.CurrentIndex : behavior.AttackObject.HitboxKeyframes.Length, options.ToArray(), values.ToArray());

        bool dirty = false;
        if (behavior.AttackObject.HitboxKeyframes.Length != 0 &&
            behavior.CurrentIndex == behavior.AttackObject.HitboxKeyframes.Length &&
            behavior.Frame != behavior.AttackObject.HitboxKeyframes[behavior.AttackObject.HitboxKeyframes.Length - 1].Frame + 1)
        {
            behavior.Frame = behavior.AttackObject.HitboxKeyframes[behavior.AttackObject.HitboxKeyframes.Length - 1].Frame + 1;
            dirty = true;
        }
        else
        {
            behavior.Frame = Mathf.Clamp(behavior.Frame, 0, behavior.AttackAnimator.CurrentAnimation.Frames.Length);
        }

        if (prevCurrentIndex != behavior.CurrentIndex)
        {
            behavior.LoadCurrentIndex();
            dirty = true;
        }

        options.Clear();
        values.Clear();
        for (int i = 0; i < behavior.AttackAnimator.CurrentAnimation.Frames.Length; ++i)
        {
            options.Add("" + i);
            values.Add(i);
        }

        int prevFrame = behavior.Frame;
        behavior.Frame = EditorGUILayout.IntPopup("Visual Frame", Mathf.Clamp(behavior.Frame, 0, behavior.AttackAnimator.CurrentAnimation.Frames.Length - 1), options.ToArray(), values.ToArray());
        if (prevFrame != behavior.Frame)
        {
            behavior.Animator.GoToFrame(behavior.Frame);
            dirty = true;
        }

        bool newKeyframe = behavior.CurrentIndex < 0 || behavior.CurrentIndex >= behavior.AttackObject.HitboxKeyframes.Length;

        if (newKeyframe)
            GUI.enabled = false;
        if (GUILayout.Button("Save Over Current Keyframe"))
        {
            behavior.SaveToCurrentIndex();
        }
        if (newKeyframe)
            GUI.enabled = true;

        if (!newKeyframe)
            GUI.enabled = false;
        if (GUILayout.Button("Create New Keyframe"))
        {
            behavior.SaveAsNewIndex();
        }
        if (!newKeyframe)
            GUI.enabled = true;

        if (newKeyframe)
            GUI.enabled = false;
        if (GUILayout.Button("Remove Current Keyframe"))
        {
            behavior.RemoveCurrent();
        }
        if (newKeyframe)
            GUI.enabled = true;

        if (dirty)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private bool _attackObjectFoldout = false;
}
