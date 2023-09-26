using FishNet;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Messaging;
using Messaging.Event;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class RusherDrone : AbstractEnemy
{
    public delegate void Death();
    public static event Death OnDeath;

    [SerializeField]
    [SyncVar]
    private float _health;

    [SerializeField]
    private GameObject deathExplosion;

    [SerializeField]
    private AudioClip explosionSound;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private float timeBetweenAttacks;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private float attackAngle;

    private GameObject projectile;

    [SerializeField]
    private float shootForwardForceMultiplier;

    private LobbyManager manager;

    [SyncVar]
    private bool isDead = false;

    private Vector3 targetPoint;

    private bool hasAttacked;
    private bool playerInAttackRange;

    private List<Player> players;
    private Player _currentTarget;
    private Transform closestPlayerTransform;
    private NavMeshAgent agent;

    public override void TakeDamage(float amount, Player player)
    {
        if (!IsSpawned) return;

        _health -= amount;
        if (_health <= 0)
        {
            if (!isDead)
                Die(player);
        }
    }

    protected override void Die(Player player)
    {
        if (!isDead)
        {
            EnemyManager.Instance.RemoveDroneFromList(this);

            SendEnemyKilledEvent(player.username, GameEnemyType.RUSHING);
            
            isDead = true;
            agent.isStopped = true;
            agent.destination = GetComponent<Transform>().position;
            //audioSource.clip = explosionSound;
            //audioSource.loop = false;

            HandleDeath(transform.position + Vector3.up);
            //var explosion = Instantiate(deathExplosion, transform.position + Vector3.up, Quaternion.identity); //to fix - something with mono/network objects
            //audioSource.Play();
            //Destroy(this.GetComponent<MeshRenderer>());

            //StartCoroutine(DespawnExplosion(explosion, 1f));
            Invoke(nameof(DespawnSelf), 1f);

            OnDeath?.Invoke();
        }
    }

    [ObserversRpc]
    private void HandleDeath(Vector3 posittion)
    {
        audioSource.Stop();
        audioSource.clip = explosionSound;
        audioSource.loop = false;
        audioSource.Play();

        var explosion = Instantiate(deathExplosion, posittion, Quaternion.identity);
        Destroy(this.GetComponent<MeshRenderer>());
        StartCoroutine(DespawnExplosion(explosion, 0.8f));
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        projectile = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressableRusherProjectileName).WaitForCompletion();
        agent = GetComponent<NavMeshAgent>();

        players = new List<Player>();

        manager = LobbyManager.Instance;

        EnemyManager.Instance.AddDroneToList(this);

        UpdateValidTargets();

        if(players.Count > 0)
        {
            _currentTarget = players[Random.Range(0, players.Count)];
        }
    }

    private void UpdateValidTargets()
    {
        players.Clear();

        foreach (var player in manager.players)
        {
            if (player.controlledPawn != null)
            {
                players.Add(player);
            }
        }
    }

    private void ValidateCurrentTarget()
    {
        if (players.Contains(_currentTarget))
        {
            return;
        }
        else
        {
            _currentTarget = players[Random.Range(0, players.Count)];
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(_currentTarget.controlledPawn.GetComponentInParent<Transform>().position);
    }

    private void FindClosestPlayer()
    {
        _currentTarget = players[0];
        closestPlayerTransform = _currentTarget.controlledPawn.GetComponentInParent<Transform>();
        float closestDist = Vector3.Distance(transform.position, closestPlayerTransform.position);

        for (int i = 1; i < players.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, players[i].controlledPawn.GetComponentInParent<Transform>().position);
            if (dist < closestDist)
            {
                closestDist = dist;
                _currentTarget = players[i];
                closestPlayerTransform = _currentTarget.controlledPawn.GetComponentInParent<Transform>();
            }
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        //FindClosestPlayer();

        //transform.LookAt(player);
        Vector3 lookVector = transform.position - closestPlayerTransform.position;
        lookVector.y = 0f;
        Quaternion rot = Quaternion.LookRotation(-lookVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1);

        if (!hasAttacked)
        {
            GameObject projectileFired = Instantiate(projectile, new Vector3(transform.position.x, transform.position.y + 0.65f, transform.position.z), Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(projectileFired);
            Rigidbody projectileBody = projectileFired.GetComponent<Rigidbody>();
            projectileBody.AddForce(transform.forward * shootForwardForceMultiplier, ForceMode.Impulse);
            projectileBody.AddForce(transform.up * (closestPlayerTransform.position.y - transform.position.y), ForceMode.Impulse);

            hasAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        hasAttacked = false;
    }

    private IEnumerator DespawnExplosion(GameObject explosion, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(explosion);
    }

    private bool CheckRange(float sphereRadius, float checkAngle)
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, sphereRadius, playerMask);

        if (playersInRange.Length != 0)
        {
            float closestPlayerRange = float.MaxValue;
            for (int i = 0; i < playersInRange.Length; i++)
            {
                Transform playerTransform = playersInRange[i].transform;
                Vector3 directionToplayer = (playerTransform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, directionToplayer) < checkAngle / 2)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                    if (!Physics.Raycast(transform.position + Vector3.up * 0.65f, directionToplayer, distanceToPlayer, obstacleMask))
                    {
                        if (distanceToPlayer < closestPlayerRange)
                        {
                            closestPlayerRange = distanceToPlayer;
                            closestPlayerTransform = playerTransform;
                            _currentTarget = playersInRange[i].GetComponentInParent<Pawn>().controllingPlayer;
                        }
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            if (closestPlayerRange < float.MaxValue)
            {
                return true;
            }
        }
        return false;
    }

    private void DespawnSelf()
    {
        this.Despawn();
    }

    private void Update()
    {
        if (!IsHost || players.Count == 0)
        {
            return;
        }

        UpdateValidTargets();
        ValidateCurrentTarget();

        playerInAttackRange = CheckRange(attackRange, attackAngle);

        if (!playerInAttackRange && !isDead) ChasePlayer();
        if (playerInAttackRange && !isDead) AttackPlayer();
    }
}
