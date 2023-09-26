using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messaging;
using Messaging.Event;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    [SyncVar] public string username;

    [SyncVar] public bool isReady;

    [SerializeField] private PlayerInfoHolderSO PlayerData;

    [SerializeField] private GameObject MessagingProcessorPrefab;

    [SyncVar] public Pawn controlledPawn;

    [SyncVar] public string CurrentEventText = "";

    public List<string> TextPromptMessageQueue = new List<string>();

    private static readonly string msgProcessorName = "MessagingProcessor";

    public Queue<InfluenceEvent> ActionsQueue = new Queue<InfluenceEvent>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        LocalInstance = this;

        PlayerData = DataManager.Instance.PlayerData;

        ServerSetIsReady(false);
        ServerSetUsername(PlayerData.PlayerName);

        SendPlayerActivityEvent(PlayerData.PlayerName, "JOINED");

        UIManager.Instance.Initialize();
        UIManager.Instance.Show<LobbyView>();

        if (LobbyManager.Instance.HasStarted)
        {
            Debug.Log($"Requesting load for player mid game: {name}");

            LobbyManager.Instance.LoadPlayer(this);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        LobbyManager.Instance.players.Add(this);

        // IMPORTANT - ENABLED SUPPORT FOR MESSAGING
        EnableServerSideMessageProcessor();

        if (!IsHost) return;

        LobbyManager.Instance.gameName = DataManager.Instance.GameData.GameName;
        LobbyManager.Instance.gameIP = DataManager.Instance.PlayerData.ProvidedIPAddress;
        LobbyManager.Instance.gameToken = DataManager.Instance.GameData.GameToken;
    }

    private void EnableServerSideMessageProcessor()
    {
        if (GameObject.Find(msgProcessorName) == null) // Allow only one messaging processor prefab
        {
            var messagingProcessor = Instantiate(MessagingProcessorPrefab, Vector3.zero, Quaternion.identity);
            // messagingProcessor.SetActive(true);
            messagingProcessor.name = msgProcessorName;
            messagingProcessor.GetComponent<EventConsumer>()
                .RegisterInfluenceSqsListener(DataManager.Instance.GameData.GameId);
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            DoConsumePlayerActionIfAvailable();
            DoConsumeTextPromptIfAvailable();
        }
    }

    private void DoConsumePlayerActionIfAvailable()
    {
        if (ActionsQueue.Count > 0)
        {
            var influenceEvent = ActionsQueue.Dequeue();
            var eventName = influenceEvent.header.name;

            if (eventName is "randomTextPromptDisplay" or "gameTextPrompt")
            {
                var eventText = influenceEvent.payload["message"];
                ServerAddMessageToPromptQueue(eventText);
            }
            else if (eventName == "randomPlayerHpDrop")
            {
                SpawnConsumableRandomLocation(ConstantValuesHolder.healthPack);
            }
            else if (eventName == "randomPlayerArmorDrop")
            {
                SpawnConsumableRandomLocation(ConstantValuesHolder.armorPack);
            }
            else if (eventName == "randomPlayerAmmoDrop")
            {
                SpawnConsumableRandomLocation(ConstantValuesHolder.ammmunitionBox);
            }
            else if (eventName == "redeployPlayers")
            {
                LobbyManager.Instance.RedeployAllPlayers();
            }
        }
    }

    private void DoConsumeTextPromptIfAvailable()
    {
        if (CurrentEventText is null or "" && TextPromptMessageQueue.Count > 0)
        {
            Debug.Log("Text prompt consumed");
            var msg = TextPromptMessageQueue[0];
            TextPromptMessageQueue.RemoveAt(0);

            LobbyManager.Instance.players.ToList().ForEach(p => { p.CurrentEventText = msg; });

            StartCoroutine(TextPromptFade());
        }
    }

    IEnumerator TextPromptFade()
    {
        yield return new WaitForSeconds(4);
        LobbyManager.Instance.players.ToList().ForEach(p => p.CurrentEventText = "");
    }

    private void ServerAddMessageToPromptQueue(string eventText)
    {
        Debug.Log("Text prompt added to queue");
        TextPromptMessageQueue.Add(eventText);
    }

    // UNUSED
    [ObserversRpc]
    public void SetTextPromptFromEvent(string text)
    {
        Debug.Log("Executing set text prompt");
        CurrentEventText = text;
        Debug.Log("After set text prompt");
    }

    [Server]
    public void SpawnConsumableRandomLocation(string consumableName)
    {
        Debug.Log("Executing spawn consumable");
        GameObject prefab = Addressables.LoadAssetAsync<GameObject>(consumableName).WaitForCompletion();
        var rand = new System.Random();
        var playersWithPawns = LobbyManager.Instance.players.Where(p => p.controlledPawn != null).ToList();
        var randomPlayer = playersWithPawns[rand.Next(playersWithPawns.Count)];
        var spawnLocation = randomPlayer.controlledPawn.transform.position +
                            new Vector3(rand.Next(-2, 2), 3, rand.Next(-2, 2));
        GameObject prefabInstance = Instantiate(prefab, spawnLocation, Quaternion.identity);
        Spawn(prefabInstance);
        Debug.Log("After spawn consumable");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        // NOTE cannot be serverRPC
        var joinEvent =
            PlayerConnectionActivityEvent.buildFor(username, "LEFT");
        EventSender.GetInstance().SendLiveFeed(joinEvent);

        LobbyManager.Instance.players.Remove(this);
    }

    [ServerRpc]
    public void SendPlayerActivityEvent(string username, string activityType)
    {
        Debug.Log("Sending player activity event");
        var joinEvent =
            PlayerConnectionActivityEvent.buildFor(username, activityType);
        EventSender.GetInstance().SendLiveFeed(joinEvent);
    }

    [ServerRpc]
    public void ServerSetIsReady(bool val)
    {
        isReady = val;
    }

    [ServerRpc]
    public void ServerSetUsername(string name)
    {
        username = name;
    }

    [TargetRpc]
    private void TargetGameStarted(NetworkConnection networkConnection)
    {
        UIManager.Instance.Show<TempGameView>();
    }

    public void StartGame()
    {
        GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressablePawnName)
            .WaitForCompletion();

        GameObject pawnInstance = Instantiate(pawnPrefab, ConstantValuesHolder.playerSpawnPoint, Quaternion.identity);

        Spawn(pawnInstance, Owner);

        controlledPawn = pawnInstance.GetComponent<Pawn>();

        controlledPawn.controllingPlayer = this;

        TargetPawnSpawned(Owner);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSpawnPawn()
    {
        GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressablePawnName)
            .WaitForCompletion();

        GameObject pawnInstance = Instantiate(pawnPrefab, ConstantValuesHolder.playerSpawnPoint, Quaternion.identity);

        Spawn(pawnInstance, Owner);

        controlledPawn = pawnInstance.GetComponent<Pawn>();

        controlledPawn.controllingPlayer = this;

        TargetPawnSpawned(Owner);
    }

    [TargetRpc]
    private void TargetPawnSpawned(NetworkConnection networkConnection)
    {
        UIManager.Instance.Show<HUDView>();
    }

    [TargetRpc]
    public void TargetPawnKilled(NetworkConnection networkConnection)
    {
        SendPlayerDiedEvent(username);
        UIManager.Instance.Show<DeathView>();
    }

    [ServerRpc]
    public void SendPlayerDiedEvent(string username)
    {
        var evnt = PlayerDiedEvent.Create(username);
        EventSender.GetInstance().SendStatistics(evnt);
    }

    [TargetRpc]
    public void TargetGameEnded(NetworkConnection networkConnection)
    {
        UIManager.Instance.Show<LobbyView>();
    }

    [TargetRpc]
    public void TargetGameScored(NetworkConnection networkConnection, string resultMessage)
    {
        UIManager.Instance.Show<ScoreView>(resultMessage);
    }

    public void ScoreGame(string resultMessage)
    {
        if (controlledPawn is not null)
        {
            controlledPawn.Controller.DefaultInput.Disable();
            controlledPawn.Controller.Weapon.WeaponInput.Disable();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        TargetGameScored(Owner, resultMessage);
    }

    public void StopGame()
    {
        if (controlledPawn != null && controlledPawn.IsSpawned) controlledPawn.Despawn();
        //_mainCameraAudioListener.enabled = true;
        ServerSetIsReady(false);
        TargetGameEnded(Owner);
    }
}