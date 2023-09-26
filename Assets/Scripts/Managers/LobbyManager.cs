using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using Messaging;
using Messaging.Event;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Text;
using Newtonsoft.Json;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SyncObject] public readonly SyncList<Player> players = new SyncList<Player>();

    [SyncVar] public bool canStart;

    [SyncVar] public bool HasStarted;

    [SyncVar] public string gameName;

    [SyncVar] public string gameIP;

    [SyncVar] public string gameToken;

    [SyncVar] public string SelectedMapName;

    [SyncVar] public string SelectedMapSceneName;

    [SyncVar] public ScoreboardData ScoreboardData;

    public List<string> MapNames;

    private Dictionary<string, string> _mapSceneNameToApiNameMap;
    public string SelectedMapGameApiName => _mapSceneNameToApiNameMap[SelectedMapSceneName];

    private void Awake()
    {
        Instance = this;

        if (IsHost)
        {
            HasStarted = false;
        }

        SelectedMapSceneName = ConstantValuesHolder.mapArenaSceneName;

        _mapSceneNameToApiNameMap = new Dictionary<string, string>()
        {
            {ConstantValuesHolder.mapArenaSceneName, "wave-arena"},
            {ConstantValuesHolder.mapWarehouseSceneName, "warehouse"},
            {ConstantValuesHolder.mapTestingAreaSceneName, "test-area"}
        };
    }

    private void Update()
    {
        if (!IsServer) return;

        canStart = players.All(player => player.isReady);
    }

    [Server]
    public void StartGame()
    {
        if (!IsHost) return;
        if (!canStart) return;

        if (!HasStarted)
        {
            HasStarted = true;

            //SceneLoadData sld = new SceneLoadData(ConstantValuesHolder.map1SceneName);
            //sld.MovedNetworkObjects = new NetworkObject[players.Count];

            if (IsServer)
            {
                EventSender.GetInstance()
                    .SendLiveFeed(GameStatusChangedEvent.Create(GameStatus.RUNNING, "Game has been started!"));
                HeartbeatGenerator.IsGameRunning = true;
            }

            for (int i = 0; i < players.Count; i++)
            {
                //players[i].StartGame();

                SceneLoadData sld = new SceneLoadData(SelectedMapSceneName);
                NetworkObject nob = players[i].GetComponentInParent<NetworkObject>();
                //sld.MovedNetworkObjects = new NetworkObject[] { nob};
                sld.ReplaceScenes = ReplaceOption.None;
                //NetworkObject nob = players[i].GetComponentInParent<NetworkObject>();
                //sld.MovedNetworkObjects[i] = nob;

                InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);
            }

            //sld.ReplaceScenes = ReplaceOption.None;
            //SceneUnloadData sud = new SceneUnloadData(ConstantValuesHolder.onlinelobbySceneName);
            //InstanceFinder.SceneManager.UnloadGlobalScenes(sud);

            //InstanceFinder.SceneManager.LoadGlobalScenes(sld);

            for (int i = 0; i < players.Count; i++)
            {
                players[i].StartGame();
            }
        }
    }

    [Server]
    [ServerRpc(RequireOwnership = false)]
    public void LoadPlayer(Player player)
    {
        Debug.Log($"Loading player mid game: {player.name}");

        SceneLoadData sld = new SceneLoadData(SelectedMapSceneName);
        NetworkObject nob = player.GetComponentInParent<NetworkObject>();

        sld.ReplaceScenes = ReplaceOption.None;
        InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);

        player.StartGame();
    }

    // Really hope this only gets called by server
    public void SendMapChoiceUpdatedEvent()
    {
        var evnt = MapChoiceUpdatedEvent.Create(SelectedMapGameApiName);
        EventSender.GetInstance().SendLiveFeed(evnt);
    }

    [Server]
    public void ScoreGame(string resultMessage)
    {
        EnemyManager.Instance.DestroyAllRemainingEnemies();

        if (!IsHost) return;

        StartCoroutine(PostRequest(resultMessage));

        //for (int i = 0; i < players.Count; i++)
        //{
        //    players[i].ScoreGame(resultMessage);
        //}
    }

    [Server]
    public void StopGame()
    {
        if (!IsHost) return;

        HasStarted = false;

        //EnemyManager.Instance.DestroyAllRemainingEnemies();

        for (int i = 0; i < players.Count; i++)
        {
            players[i].StopGame();

            SceneUnloadData sud = new SceneUnloadData(SelectedMapSceneName);

            NetworkConnection conn = players[i].Owner;
            NetworkManager.SceneManager.UnloadConnectionScenes(conn, sud);
        }
    }

    private IEnumerator PostRequest(string resultmessage)
    {
        UnityWebRequest uwr = new UnityWebRequest(ConstantValuesHolder.graphQLURL, "POST");
        var json = "{\"query\": \"query { scoreboard(gameId: \\\"" + DataManager.Instance.GameData.GameId +
                   "\\\") { player { username } kills deaths kdRatio  points }}\"}";

        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + DataManager.Instance.PlayerData.AuthToken);

        yield return uwr.SendWebRequest();

        if (uwr.responseCode == 200)
        {
            Debug.Log(uwr.downloadHandler.text);
            ScoreboardData = JsonConvert.DeserializeObject<ScoreboardData>(uwr.downloadHandler.text);
            uwr.Dispose();
            for (int i = 0; i < ScoreboardData.data.scoreboard.Count; i++)
            {
                Debug.Log("Username scoring: " + ScoreboardData.data.scoreboard[i].player.username);
            }

            for (int i = 0; i < players.Count; i++)
            {
                players[i].ScoreGame(resultmessage);
            }
        }
        else
        {
            Debug.Log("Scoring code: " + uwr.responseCode);
            Debug.Log(uwr.downloadHandler.text);
            uwr.Dispose();
        }
    }

    [Server]
    public void RedeployAllPlayers()
    {
        foreach (var player in players)
        {
            if (player.controlledPawn == null)
            {
                player.StartGame();
            }
        }
    }
}