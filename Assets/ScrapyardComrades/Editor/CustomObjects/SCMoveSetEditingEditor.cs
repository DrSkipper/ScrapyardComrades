﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SCMoveSetEditing))]
public class SCMoveSetEditingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SCMoveSetEditing behavior = this.target as SCMoveSetEditing;
        bool dirty = false;
        bool newAttack = false;

        SCMoveSet moveSet = behavior.MoveSet;
        if (moveSet != null)
        {
            if (moveSet != behavior.EditingMoveSet)
            {
                newAttack = true;
                behavior.EditingMoveSet = moveSet;
                behavior.AttackObject = behavior.MoveSet.GroundNeutral;
                behavior.MoveIndex = 0;
                behavior.CurrentIndex = 0;
                behavior.Frame = 0;
            }

            if (_moveOptions == null)
            {
                //TODO: Expand as more attack options, combos, etc are added
                _moveOptions = new List<string>();
                _moveOptions.Add("Ground Neutral");
                _moveOptions.Add("Ground Strong");
                _moveOptions.Add("Ground Dodge");
                _moveOptions.Add("Air Neutral");
                _moveOptions.Add("Air Strong");
                _moveOptions.Add("Air Dodge");
                _moveOptions.Add("Extra Ref A");
                _moveOptions.Add("Extra Ref B");
                _moveOptions.Add("Extra Ref C");
                _moveOptions.Add("Extra Ref D");
                _moveOptions.Add("Extra Ref E");
                _moveOptions.Add("Extra Ref F");

                /*if (behavior.AttackObject.Combos != null)
                {
                    for (int i = 0; i < behavior.AttackObject.Combos.Length; ++i)
                    {
                        _moveOptions.Add("Combo " + behavior.AttackObject.Combos[i].ComboMove.name);
                    }
                }*/
            }

            int oldMoveIndex = behavior.MoveIndex;
            behavior.MoveIndex = EditorGUILayout.Popup(label: "Current Move", selectedIndex: behavior.MoveIndex, displayedOptions: _moveOptions.ToArray());

            //TODO: Expand as more attack options, combos, etc are added
            if (oldMoveIndex != behavior.MoveIndex)
            {
                newAttack = true;
                if (behavior.MoveIndex == 0)
                    behavior.AttackObject = behavior.MoveSet.GroundNeutral;
                else if (behavior.MoveIndex == 1)
                    behavior.AttackObject = behavior.MoveSet.GroundStrong;
                else if (behavior.MoveIndex == 2)
                    behavior.AttackObject = behavior.MoveSet.GroundDodge;
                else if (behavior.MoveIndex == 3)
                    behavior.AttackObject = behavior.MoveSet.AirNeutral;
                else if (behavior.MoveIndex == 4)
                    behavior.AttackObject = behavior.MoveSet.AirStrong;
                else if (behavior.MoveIndex == 5)
                    behavior.AttackObject = behavior.MoveSet.AirDodge;
                else if (behavior.MoveIndex == 6)
                    behavior.AttackObject = behavior.MoveSet.ExtraRefA;
                else if (behavior.MoveIndex == 7)
                    behavior.AttackObject = behavior.MoveSet.ExtraRefB;
                else if (behavior.MoveIndex == 8)
                    behavior.AttackObject = behavior.MoveSet.ExtraRefC;
                else if (behavior.MoveIndex == 9)
                    behavior.AttackObject = behavior.MoveSet.ExtraRefD;
                else if (behavior.MoveIndex == 10)
                    behavior.AttackObject = behavior.MoveSet.ExtraRefE;
                else if (behavior.MoveIndex == 11)
                    behavior.AttackObject = behavior.MoveSet.ExtraRefF;
                behavior.CurrentIndex = 0;
                behavior.Frame = 0;
            }
        }

        if (behavior.AttackObject != null)
        {
            if (newAttack)
            {
                behavior.Animator.PlayAnimation(behavior.AttackObject.SpriteAnimation);
            }
            _attackObjectFoldout = EditorGUILayout.Foldout(_attackObjectFoldout, "Move Object Fields");

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

            int prevFrame = behavior.Frame;

            options.Clear();
            values.Clear();
            if (behavior.AttackAnimator.CurrentAnimation != null)
            {
                for (int i = 0; i < behavior.AttackAnimator.CurrentAnimation.Frames.Length; ++i)
                {
                    options.Add("" + i);
                    values.Add(i);
                }
            }

            behavior.Frame = EditorGUILayout.IntPopup("Visual Frame", Mathf.Clamp(behavior.Frame, 0, behavior.AttackAnimator.CurrentAnimation.Frames.Length - 1), options.ToArray(), values.ToArray());

            if (behavior.AttackObject.HitboxKeyframes.Length != 0 &&
                behavior.CurrentIndex == behavior.AttackObject.HitboxKeyframes.Length &&
                behavior.Frame < behavior.AttackObject.HitboxKeyframes[behavior.AttackObject.HitboxKeyframes.Length - 1].VisualFrame + 1)
            {
                behavior.Frame = behavior.AttackObject.HitboxKeyframes[behavior.AttackObject.HitboxKeyframes.Length - 1].VisualFrame + 1;
                dirty = true;
            }
            else
            {
                behavior.Frame = Mathf.Clamp(behavior.Frame, 0, behavior.AttackAnimator.CurrentAnimation.Frames.Length - 1);
            }

            if (prevFrame != behavior.Frame)
            {
                dirty = true;
            }

            if (newAttack || prevCurrentIndex != behavior.CurrentIndex)
            {
                prevCurrentIndex = behavior.CurrentIndex;
                behavior.LoadCurrentIndex();
                dirty = true;
            }
            else
            {
                behavior.Animator.GoToFrame(behavior.Frame);
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
            if (GUILayout.Button("Overwrite Hurtbox Values"))
            {
                behavior.UpdateMoveSetHurtboxes();
            }
        }
        if (dirty)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private bool _attackObjectFoldout = false;
    private List<string> _moveOptions = null;
}
