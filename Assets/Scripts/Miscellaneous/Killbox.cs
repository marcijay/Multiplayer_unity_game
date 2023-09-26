using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killbox : NetworkBehaviour
{
    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Pawn>(out var pawn))
        {
            pawn.ReceiveDamage((pawn.Health + pawn.Armour) * 5);
        }
    }
}
