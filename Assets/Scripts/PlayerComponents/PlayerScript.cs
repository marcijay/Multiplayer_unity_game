using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerScript : NetworkBehaviour
{
    public static PlayerScript LocalInstance { get; private set; }

    [SyncVar]
    public string username;

    [SyncVar]
    public bool isReady;

    [SyncVar]
    public Pawn controlledPawn;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        LocalInstance = this;

        UIManager.Instance.Initialize();

        UIManager.Instance.Show<LobbyView>();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        //LobbyManager.Instance.players.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        //LobbyManager.Instance.players.Remove(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetIsReady(bool val)
    {
        isReady = val;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSpawnPawn()
    {
        StartGame();
    }

    [TargetRpc]
    private void TargetPawnSpawned(NetworkConnection networkConnection)
    {
        UIManager.Instance.Show<HUDView>();
    }

    [TargetRpc]
    public void TargetPawnKilled(NetworkConnection networkConnection)
    {
        UIManager.Instance.Show<RespawnView>();
    }

    public void StartGame()
    {
        GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>("Pawn").WaitForCompletion();

        GameObject pawnInstance = Instantiate(pawnPrefab);

        Spawn(pawnInstance, Owner);

        controlledPawn = pawnInstance.GetComponent<Pawn>();

        //controlledPawn.controllingPlayer = this;

        TargetPawnSpawned(Owner);
    }

    public void StopGame()
    {
        if (controlledPawn != null && controlledPawn.IsSpawned) controlledPawn.Despawn();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ServerSetIsReady(!isReady);
        }
    }
}
