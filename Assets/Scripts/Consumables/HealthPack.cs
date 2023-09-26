using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : AbstractConsumable
{
    [SerializeField]
    private int _amount;

    [Server]
    protected void OnTriggerEnter(Collider other)
    {
        CheckForKillbox(other);

        if (other.isTrigger) return;

        Pawn pawn = other.GetComponent<Pawn>();
        if (pawn != null && pawn.Health < pawn.MaxHealth)
        {
            if (pawn.MaxHealth - pawn.Health > _amount)
            {
                pawn.Health += _amount;
            }
            else
            {
                pawn.Health = pawn.MaxHealth;
            }
            Despawn();
        }
    }
}
