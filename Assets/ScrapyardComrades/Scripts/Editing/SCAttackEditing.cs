using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SCAttackEditing : MonoBehaviour
{
    public SCSpriteAnimator AttackAnimator;
    public SCSpriteAnimator Animator;
    public IntegerRectCollider[] Hitboxes;
    public IntegerRectCollider Hurtbox;
    public SCAttack AttackObject;
    [HideInInspector]
    public int CurrentIndex = 0;
    [HideInInspector]
    public int Frame = 0;

    public void SaveToCurrentIndex()
    {
        if (this.AttackObject == null)
        {
            Debug.LogWarning("Attempting to save attack keyframes to null attack object");
            return;
        }
        if (this.Hitboxes == null || this.Hitboxes.Length == 0)
            Debug.LogWarning("Saving attack keyframes with no hitboxes");

        if (this.Hurtbox == null)
            Debug.LogWarning("Saving attack keyframes with no hurtbox");
        
        if (this.CurrentIndex >= 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Length)
        {
            this.AttackObject.HitboxKeyframes[this.CurrentIndex] = gatherKeyframeData();
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
                    this.Hitboxes[i].transform.position = (Vector2)currentHitboxFrame.HitboxPositions[i];
                    this.Hitboxes[i].Size = currentHitboxFrame.HitboxSizes[i];
                }
                else
                {
                    this.Hitboxes[i].enabled = false;
                }
            }
            this.Hurtbox.Offset = currentHitboxFrame.HurtboxRect.Center;
            this.Hurtbox.Size = currentHitboxFrame.HurtboxRect.Size;
        }
        this.Animator.GoToFrame(this.Frame);
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

        keyframe.HurtboxRect = this.Hurtbox != null && this.Hurtbox.enabled ? new IntegerRect(this.Hurtbox.Offset, this.Hurtbox.Size) : new IntegerRect();
        keyframe.Frame = this.Animator.GetDataFrameForVisualFrame(this.Frame);
        keyframe.VisualFrame = this.Frame;
        keyframe.HitboxPositions = hitboxPositions.ToArray();
        keyframe.HitboxSizes = hitboxSizes.ToArray();
        return keyframe;
    }

    private void saveState()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this.AttackObject);
        AssetDatabase.SaveAssets();
#endif
    }
}
