﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SCSpriteAnimator))]
public class SCSpriteAnimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SCSpriteAnimator behavior = this.target as SCSpriteAnimator;

        if (behavior.DefaultAnimation != _editingAnimation)
        {
            _editingAnimation = behavior.DefaultAnimation;
            if (behavior.spriteRenderer != null)
                behavior.spriteRenderer.sprite = _editingAnimation != null && _editingAnimation.Frames.Length > 0 ? _editingAnimation.Frames[0] : null;
            else if (behavior.UiImage != null)
                behavior.UiImage.sprite = _editingAnimation != null && _editingAnimation.Frames.Length > 0 ? _editingAnimation.Frames[0] : null;
        }

    }
    
    private SCSpriteAnimation _editingAnimation; // For editor script
}
