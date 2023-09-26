using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : NetworkBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [SerializeField]
    [SyncVar]
    private int _enemiesToKill;

    [SyncVar]
    public string objectiveText;

    [SerializeField]
    private int _rushersSpawnInterwalSeconds;

    [SerializeField]
    private int _rushersPerPlayerMultiplier;

    [SerializeField]
    private GameObject[] _spawnPoints;

    [SerializeField]
    private GameObject _enemyPrefab;

    private int _activeRushers = 0;
    private bool _win = false;
    private bool _ended = false;
    private List<Player> _players;
    private LobbyManager _manager;

    [ServerRpc(RequireOwnership = false)]
    private void UpdateObjectiveText()
    {
        var objectiveEnemyText = _enemiesToKill >= 0 ? _enemiesToKill : 0;
        objectiveText = $"Destrony {objectiveEnemyText} Red drones to win";
    }

    private void Start()
    {
        Instance = this;
        _players = new List<Player>();
        _manager = LobbyManager.Instance;
        UpdateObjectiveText();

        if (!IsHost) return;

        StartCoroutine(SpawnRushers());
    }

    private void OnEnable()
    {
        EnemyDrone.OnDeath += DecrementEnemiesCount;
    }

    private void OnDisable()
    {
        EnemyDrone.OnDeath -= DecrementEnemiesCount;
    }

    private void DecrementEnemiesCount()
    {
        _enemiesToKill--;
        UpdateObjectiveText();
    }

    private void EndGame()
    {
        LobbyManager.Instance.ScoreGame(_win ? "You won!" : "You lost");
    }

    private void UpdateLivingPlayers()
    {
        _players.Clear();

        foreach (var player in _manager.players)
        {
            if (player.controlledPawn != null)
            {
                _players.Add(player);
            }
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        UpdateLivingPlayers();

        if(_players.Count == 0 && !_ended)
        {
            _ended = true;
            Invoke(nameof(EndGame), 2f);
        }

        if(_enemiesToKill <= 0)
        {
            _win = true;
            Invoke(nameof(EndGame), 2f);
        }
    }

    private IEnumerator SpawnRushers()
    {
        while (!_ended)
        {
            yield return new WaitForSeconds(_rushersSpawnInterwalSeconds);

            while (_activeRushers < _rushersPerPlayerMultiplier * LobbyManager.Instance.players.Count)
            {
                GameObject spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

                GameObject enemyInstance = Instantiate(_enemyPrefab, spawnPoint.transform.position, Quaternion.identity);

                Spawn(enemyInstance);

                _activeRushers++;
            }
        }
    }
}
