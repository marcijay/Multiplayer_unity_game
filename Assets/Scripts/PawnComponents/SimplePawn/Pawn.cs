using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : NetworkBehaviour
{
    [SyncVar]
    public Player controllingPlayer;

    [Header("Settings")]
    [SyncVar]
    public float Health;
    [SyncVar]
    public float MaxHealth;
    [SyncVar]
    public float Armour;
    [SyncVar]
    public float FortifyDamageMultiplier;

    public AdvancedCharacterController Controller;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Controller = GetComponent<AdvancedCharacterController>();
    }

    public void ReceiveDamage(float amount)
    {
        if (!IsSpawned) return;

        if (Controller.IsFortified)
        {
            amount *= FortifyDamageMultiplier;
        }

        if (amount > Armour)
        {
            amount -= Armour;
            Armour = 0;
        }
        else
        {
            Armour -= amount;
            amount = 0;
        }

        if((Health -= amount) <= 0.0f)
        {
            controllingPlayer.controlledPawn = null;

            controllingPlayer.TargetPawnKilled(Owner);

            Despawn();
        }
    }
}
