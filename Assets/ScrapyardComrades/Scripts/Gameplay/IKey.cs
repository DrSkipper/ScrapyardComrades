using UnityEngine;

public interface IKey
{
    bool CanOpen(SCPickup.KeyType lockType);
}
