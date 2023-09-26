using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SyncObject]
    private readonly SyncList<AbstractEnemy> _enemies = new SyncList<AbstractEnemy>();

    [SyncVar]
    public int enemiesToKill;

    [SyncVar]
    public int enemiesKilled;

    private void Awake()
    {
        Instance = this;
        enemiesKilled = 0;
    }

    //public override void OnStartNetwork()
    //{
    //    base.OnStartNetwork();
    //    Instance = this;
    //    enemiesKilled = 0;
    //}

    private void Update()
    {
        if (!IsServer) return;

        //if(enemiesKilled == enemiesToKill)
        //{
        //    StopGame();
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddDroneToList(AbstractEnemy enemy)
    {
        _enemies.Add(enemy);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveDroneFromList(AbstractEnemy enemy)
    {
        _enemies.Remove(enemy);
    }

    [Server]
    public void DestroyAllRemainingEnemies()
    {
        for(int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            enemy.Despawn();
        }
    }

    [Server]
    public void StopGame()
    {
        LobbyManager.Instance.StopGame();
    }
}
