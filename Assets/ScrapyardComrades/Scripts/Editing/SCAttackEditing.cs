using UnityEngine;
using System.Collections.Generic;

public class SCAttackEditing : MonoBehaviour
{
    public SCAttack AttackObject;
    public IntegerRectCollider[] Hitboxes;
    public int CurrentIndex = 0;
    public int Frame = 0;


    public void SaveToCurrentIndex()
    {
        if (this.CurrentIndex > 0 && this.CurrentIndex < this.AttackObject.HitboxKeyframes.Count)
        {
            List<IntegerVector> hitboxPositions = new List<IntegerVector>();
            List<IntegerVector> hitboxSizes = new List<IntegerVector>();
            SCAttack.HitboxKeyframe keyframe = new SCAttack.HitboxKeyframe();
            //keyframe.Frame = this.Frame;
            //keyframe.Position
            //this.AttackObject.HitboxKeyframes[this.CurrentIndex] = new SCAttack.HitboxKeyframe()
        }
    }

    public void SaveAsNewIndex()
    {
    }
}
