using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    public static WaveManager Instance;

    [SyncVar]
    public int CurrentEnemies = 0;

    [SyncVar]
    public int Wave = 0;

    [SerializeField]
    private int _enemyPerWaveMultiplier;

    [SerializeField]
    private GameObject[] _spawnPoints;

    [SerializeField]
    private GameObject _enemyPrefab;

    public int MaxWave;

    private bool _win = false;
    private bool _ended = false;
    private List<Player> _players;
    private LobbyManager _manager;

    private void Awake()
    {
        Instance = this;
        _players = new List<Player>();
        _manager = LobbyManager.Instance;
    }

    private void OnEnable()
    {
        EnemyDrone.OnDeath += DecrementCurrentEnemiesCount;
        RusherDrone.OnDeath += DecrementCurrentEnemiesCount;
    }

    private void OnDisable()
    {
        EnemyDrone.OnDeath -= DecrementCurrentEnemiesCount;
        RusherDrone.OnDeath -= DecrementCurrentEnemiesCount;
    }

    private void Update()
    {
        if (!IsHost) return;

        UpdateLivingPlayers();

        if (_players.Count == 0 && !_ended)
        {
            _ended = true;
            Invoke(nameof(EndGame), 2f);
        }

        if (Wave == MaxWave && CurrentEnemies == 0)
        {
            _win = true;
            Invoke(nameof(EndGame), 2f);
        }

        if(Wave < MaxWave && CurrentEnemies == 0)
        {
            Wave++;
            SpawnWave();
        }
    }

    private void DecrementCurrentEnemiesCount()
    {
        CurrentEnemies--;
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

    private void SpawnWave()
    {
        if (!IsHost) return;

        while (CurrentEnemies < Wave * _enemyPerWaveMultiplier * LobbyManager.Instance.players.Count)
        {
            GameObject spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            GameObject enemyInstance = Instantiate(_enemyPrefab, spawnPoint.transform.position, Quaternion.identity);

            Spawn(enemyInstance);

            CurrentEnemies++;
        }
    }
}
