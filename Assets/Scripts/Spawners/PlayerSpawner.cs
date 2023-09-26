using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject spawnPoint;

    private LobbyManager manager;

    private void Awake()
    {
        manager = LobbyManager.Instance;
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        Pawn pawn = other.GetComponent<Pawn>();
        if (pawn != null)
        {
            manager.RedeployAllPlayers();
        }
    }
}