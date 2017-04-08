﻿using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SCMoveSetEditing : MonoBehaviour
{
    public SCSpriteAnimator AttackAnimator;
    public SCSpriteAnimator Animator;
    public IntegerRectCollider[] Hitboxes;
    public IntegerRectCollider NormalHurtbox;
    public IntegerRectCollider DuckHurtbox;
    public SCSpriteAnimator EffectAnimator;
    public SCMoveSet MoveSet;
    public SCAttack.HurtboxState HurtboxState;
    public bool EffectFrame;
    [HideInInspector]
    public SCAttack AttackObject;
    [HideInInspector]
    public int CurrentIndex = 0;
    [HideInInspector]
    public int Frame = 0;
    [HideInInspector]
    public SCMoveSet EditingMoveSet;
    [HideInInspector]
    public int MoveIndex;

    public void SaveToCurrentIndex()
    {
        if (this.AttackObject == null)
        {
            Debug.LogWarning("Attempting to save attack keyframes to null attack object");
            return;
        }
        if (this.Hitboxes == null || this.Hitboxes.Length == 0)
            Debug.LogWarning("Saving attack keyframes with no hitboxes");
        
        if (this.CurrentIndex >= 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Length)
        {
            this.AttackObject.HitboxKeyframes[this.CurrentIndex] = gatherKeyframeData();
            if (this.EffectAnimator != null)
                saveEffectFrameData();
            saveState();
        }
        else
        {
            Debug.LogWarning("Attempted to save attack keyframe at invalid index");
        }
    }

    public void SaveAsNewIndex()
    {
        if (this.AttackObject == null)
        {
            Debug.LogWarning("Attempting to save attack keyframes to null attack object");
            return;
        }
        if (this.Hitboxes == null || this.Hitboxes.Length == 0)
            Debug.LogWarning("Saving attack keyframes with no hitboxes");
        
        List<SCAttack.HitboxKeyframe> keyframes = this.AttackObject.HitboxKeyframes != null ? new List<SCAttack.HitboxKeyframe>(this.AttackObject.HitboxKeyframes) : new List<SCAttack.HitboxKeyframe>();
        keyframes.Add(gatherKeyframeData());
        this.AttackObject.HitboxKeyframes = keyframes.ToArray();
        this.CurrentIndex = keyframes.Count;
        this.Frame += 1;
        this.Frame = Mathf.Clamp(this.Frame, 0, this.AttackAnimator.CurrentAnimation.Frames.Length - 1);
        this.Animator.GoToFrame(this.Frame);

        if (this.EffectAnimator != null)
            saveEffectFrameData();
        saveState();
    }

    public void LoadCurrentIndex()
    {
        if (this.CurrentIndex >= 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Length)
        {
            SCAttack.HitboxKeyframe currentHitboxFrame = this.AttackObject.HitboxKeyframes[this.CurrentIndex];
            this.Frame = currentHitboxFrame.VisualFrame;
            for (int i = 0; i < this.Hitboxes.Length; ++i)
            {
                if (currentHitboxFrame.HitboxPositions != null && i < currentHitboxFrame.HitboxPositions.Length)
                {
                    this.Hitboxes[i].enabled = true;
                    this.Hitboxes[i].Offset = IntegerVector.Zero;
                    this.Hitboxes[i].transform.localPosition = (Vector2)currentHitboxFrame.HitboxPositions[i];
                    this.Hitboxes[i].Size = currentHitboxFrame.HitboxSizes[i];
                }
                else
                {
                    this.Hitboxes[i].enabled = false;
                }
            }
            this.HurtboxState = currentHitboxFrame.HurtboxState;

        }
        this.Animator.GoToFrame(this.Frame);
        
        int foundEffect = findEffectForFrame();
        if (foundEffect >= 0 && this.EffectAnimator != null)
        {
            this.EffectFrame = true;
            this.EffectAnimator.gameObject.SetActive(true);
            this.EffectAnimator.DefaultAnimation = this.AttackObject.Effects[foundEffect].Animation;
            this.EffectAnimator.spriteRenderer.sprite = this.EffectAnimator.DefaultAnimation != null && this.EffectAnimator.DefaultAnimation.Frames.Length > 0 ? this.EffectAnimator.DefaultAnimation.Frames[0] : null;
            this.EffectAnimator.transform.SetLocalPosition2D(this.AttackObject.Effects[foundEffect].Position.X, this.AttackObject.Effects[foundEffect].Position.Y);
        }
        else
        {
            this.EffectFrame = false;
        }
    }

    public void RemoveCurrent()
    {
        if (this.CurrentIndex >= 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Length)
        {
            List<SCAttack.HitboxKeyframe> keyframes = this.AttackObject.HitboxKeyframes != null ? new List<SCAttack.HitboxKeyframe>(this.AttackObject.HitboxKeyframes) : new List<SCAttack.HitboxKeyframe>();
            keyframes.RemoveAt(this.CurrentIndex);
            this.CurrentIndex = Mathf.Max(0, this.CurrentIndex - 1);
            this.AttackObject.HitboxKeyframes = keyframes.ToArray();
            saveState();
        }
    }

    public void UpdateMoveSetHurtboxes()
    {
#if UNITY_EDITOR
        this.MoveSet.NormalHitboxSpecs = new IntegerRect(this.NormalHurtbox.Offset, this.NormalHurtbox.Size);
        this.MoveSet.DuckHitboxSpecs = new IntegerRect(this.DuckHurtbox.Offset, this.DuckHurtbox.Size);
        EditorUtility.SetDirty(this.MoveSet);
        AssetDatabase.SaveAssets();
#endif
    }

    /**
     * Private
     */
    private SCAttack.HitboxKeyframe gatherKeyframeData()
    {
        List<IntegerVector> hitboxPositions = new List<IntegerVector>();
        List<IntegerVector> hitboxSizes = new List<IntegerVector>();
        SCAttack.HitboxKeyframe keyframe = new SCAttack.HitboxKeyframe();
        for (int i = 0; i < this.Hitboxes.Length; ++i)
        {
            if (this.Hitboxes[i].enabled)
            {
                keyframe.HitboxCount++;
                hitboxPositions.Add(this.Hitboxes[i].Offset + new IntegerVector(this.Hitboxes[i].transform.localPosition));
                hitboxSizes.Add(this.Hitboxes[i].Size);
            }
        }

        keyframe.HurtboxState = this.HurtboxState;
        keyframe.Frame = this.Animator.GetDataFrameForVisualFrame(this.Frame);
        keyframe.VisualFrame = this.Frame;
        keyframe.HitboxPositions = hitboxPositions.ToArray();
        keyframe.HitboxSizes = hitboxSizes.ToArray();
        return keyframe;
    }

    private int findEffectForFrame()
    {
        int foundEffect = -1;
        int searchFrame = this.Animator.GetDataFrameForVisualFrame(this.Frame);
        if (this.AttackObject.Effects != null)
        {
            for (int i = 0; i < this.AttackObject.Effects.Length; ++i)
            {
                if (this.AttackObject.Effects[i].Frame == searchFrame)
                {
                    foundEffect = i;
                    break;
                }
            }
        }
        return foundEffect;
    }

    private void saveEffectFrameData()
    {
        int foundEffect = findEffectForFrame();
        if (this.EffectFrame)
        {
            if (foundEffect >= 0)
            {
                this.AttackObject.Effects[foundEffect].Animation = this.EffectAnimator.DefaultAnimation;
                this.AttackObject.Effects[foundEffect].Position = (Vector2)this.EffectAnimator.transform.localPosition;
            }
            else
            {
                SCAttack.Effect effect = new SCAttack.Effect();
                effect.Animation = this.EffectAnimator.DefaultAnimation;
                effect.Position = (Vector2)this.EffectAnimator.transform.localPosition;
                effect.Frame = this.Animator.GetDataFrameForVisualFrame(this.Frame);

                List<SCAttack.Effect> effects = this.AttackObject.Effects != null ? new List<SCAttack.Effect>(this.AttackObject.Effects) : new List<SCAttack.Effect>(1);
                effects.Add(effect);
                effects.Sort(frameCompareEffects);
                this.AttackObject.Effects = effects.ToArray();
            }
        }
        else
        {

            if (foundEffect >= 0)
            {
                List<SCAttack.Effect> effects = new List<SCAttack.Effect>(this.AttackObject.Effects);
                effects.RemoveAt(foundEffect);
                this.AttackObject.Effects = effects.ToArray();
            }
        }
    }

    private static int frameCompareEffects(SCAttack.Effect e1, SCAttack.Effect e2)
    {
        return Mathf.Clamp(e2.Frame - e1.Frame, -1, 1);
    }

    private void saveState()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this.AttackObject);
        AssetDatabase.SaveAssets();
#endif
    }
}
