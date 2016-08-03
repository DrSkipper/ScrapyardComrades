using UnityEngine;
using System.Collections.Generic;

public class SCAttackEditing : MonoBehaviour
{
    public SCSpriteAnimator AttackAnimator;
    public SCSpriteAnimator Animator;
    public IntegerRectCollider[] Hitboxes;
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
        
        if (this.CurrentIndex >= 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Length)
        {
            this.AttackObject.HitboxKeyframes[this.CurrentIndex] = gatherKeyframeData();
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
        this.Frame = Mathf.Clamp(this.Frame, 0, this.AttackAnimator.CurrentAnimation.Frames.Length);
        this.Animator.GoToFrame(this.Frame);
    }

    public void LoadCurrentIndex()
    {
        if (this.CurrentIndex >= 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Length)
        {
            this.Frame = Mathf.RoundToInt((float)this.AttackObject.HitboxKeyframes[this.CurrentIndex].Frame / this.Animator.GetFrameDuration());
            for (int i = 0; i < this.Hitboxes.Length; ++i)
            {
                if (i < this.AttackObject.HitboxKeyframes[this.CurrentIndex].Positions.Length)
                {
                    this.Hitboxes[i].enabled = true;
                    this.Hitboxes[i].Offset = IntegerVector.Zero;
                    this.Hitboxes[i].transform.position = (Vector2)this.AttackObject.HitboxKeyframes[this.CurrentIndex].Positions[i];
                }
                else
                {
                    this.Hitboxes[i].enabled = false;
                }
            }
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

        keyframe.Frame = this.Animator.GetDataFrameForVisualFrame(this.Frame);
        keyframe.Positions = hitboxPositions.ToArray();
        keyframe.Sizes = hitboxSizes.ToArray();
        return keyframe;
    }
}
