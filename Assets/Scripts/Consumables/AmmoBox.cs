using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : AbstractConsumable
{
    [SerializeField]
    private int _amount;

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        CheckForKillbox(other);

        if (other.isTrigger) return;

        Pawn pawn = other.GetComponent<Pawn>();
        if (pawn != null)
        {
            pawn.Controller.Weapon.AmmunitionReserve += _amount;

            Despawn();
        }
    }
}
