using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Messaging;
using Messaging.Event;
using UnityEngine;

public abstract class AbstractEnemy : NetworkBehaviour
{
    public abstract void TakeDamage(float amount, Player player);

    protected abstract void Die(Player player);
    
    protected void SendEnemyKilledEvent(string username, GameEnemyType enemyType)
    {
        var evnt = EnemyKilledEvent.Create(username, enemyType);
        EventSender.GetInstance().SendStatistics(evnt);
    }
}
