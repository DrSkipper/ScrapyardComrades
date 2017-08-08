using UnityEngine;

public class BlockHandler : MonoBehaviour
{
    public Damagable Damagable;

    public void HandleBlock(int freezeFrames, SCAttack.HitData hitData)
    {
        this.Damagable.Block(freezeFrames, hitData);
    }
}
