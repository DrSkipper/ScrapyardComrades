using UnityEngine;

public static class LayerMaskExtensions
{
    public static bool ContainsLayer(this LayerMask self, int layer)
    {
        return (((1 << layer) | self) == self);
    }
}
