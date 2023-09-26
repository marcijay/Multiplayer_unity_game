using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractConsumable : NetworkBehaviour
{
    protected void CheckForKillbox(Collider other)
    {
        if (other.TryGetComponent<Killbox>(out _))
        {
            Despawn();
        }
    }
}
