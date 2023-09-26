using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Messaging;
using Messaging.Event;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class EnemyDrone : AbstractEnemy
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
    private GameObject bodyFront;

    [SerializeField]
    private GameObject bodyBack;

    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private float targetPointRange;

    [SerializeField]
    private float timeBetweenAttacks;

    [SerializeField]
    private float sightRange;

    [SerializeField]
    private float sightAngle;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private float attackAngle;

    [SerializeField]
    private float targetRange;

    private GameObject projectile;

    [SerializeField]
    private float shootForwardForceMultiplier;

    [SerializeField]
    private float shootUpwardForceMultiplier;

    private LobbyManager manager;

    private bool isDead = false;

    private Vector3 targetPoint;
    private bool targetSet;

    private bool hasAttacked;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    private List<Transform> players;
    private Transform closestPlayer;
    private NavMeshAgent agent;

    private Vector3 playerLastKnownPosition;


    public override void TakeDamage(float amount, Player player)
    {
        if (!IsSpawned) return;

        _health -= amount;
        if (_health <= 0)
        {
            if(!isDead)
                Die(player);
        }
    }

    protected override void Die(Player player)
    {
        if (!isDead)
        {
            EnemyManager.Instance.RemoveDroneFromList(this);
            
            SendEnemyKilledEvent(player.username, GameEnemyType.DRONE);

            isDead = true;
            agent.isStopped = true;
            agent.destination = GetComponent<Transform>().position;
            //audioSource.clip = explosionSound;
            //audioSource.loop = false;

            //var explosion = Instantiate(deathExplosion, bodyBack.transform); //to fix - something with mono/network objects
            HandleDeath(bodyBack.transform.position);

            //audioSource.Play();

            //StartCoroutine(DespawnExplosion(explosion, 2f));
            Invoke(nameof(DespawnSelf), 1f);
            //depawn enemy & explosion

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
        Destroy(bodyBack.GetComponent<MeshRenderer>());
        Destroy(bodyFront.GetComponent<MeshRenderer>());
        StartCoroutine(DespawnExplosion(explosion, 0.8f));
    }

    //private void Awake()
    //{
    //    //player = GameObject.FindGameObjectWithTag("Player").transform;

    //    projectile = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressableDroneProjectileName).WaitForCompletion();
    //    agent = GetComponent<NavMeshAgent>();

    //    players = new List<Transform>();

    //    manager = LobbyManager.Instance;

    //    //EnemyManager.Instance.AddDroneToList(this);

    //    UpdateValidTargets();

    //    //levelManager = FindObjectOfType<LevelManager>();
    //}

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        projectile = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressableDroneProjectileName).WaitForCompletion();
        agent = GetComponent<NavMeshAgent>();

        players = new List<Transform>();

        manager = LobbyManager.Instance;

        EnemyManager.Instance.AddDroneToList(this);

        playerLastKnownPosition = Vector3.zero;
        UpdateValidTargets();
    }

    private void UpdateValidTargets()
    {
        players.Clear();

        foreach (var player in manager.players)
        {
            if (player.controlledPawn != null)
            {
                players.Add(player.controlledPawn.GetComponentInParent<Transform>());
            }
        }
    }

    private void Patrol()
    {
        if (!targetSet) 
            SetTargetPoint();
        if (targetSet)
            agent.SetDestination(targetPoint);

        Vector3 distanceToTargetPoint = transform.position - targetPoint;

        if (distanceToTargetPoint.magnitude < targetRange)
            targetSet = false;
    }

    private void SetTargetPoint()
    {
        if(playerLastKnownPosition != Vector3.zero)
        {
            targetPoint = playerLastKnownPosition;
            playerLastKnownPosition = Vector3.zero;
            targetSet = true;
        }
        else
        {
            float randomX = Random.Range(-targetPointRange, targetPointRange);
            float randomZ = Random.Range(-targetPointRange, targetPointRange);

            targetPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(targetPoint, -transform.up, 2f, groundMask))
                targetSet = true;
        }
    }

    private void ChasePlayer()
    {
        //UpdateValidTargets();
        //FindClosestPlayer();
        agent.SetDestination(closestPlayer.position);
    }

    private void FindClosestPlayer()
    {
        closestPlayer = players[0];
        float closestDist = Vector3.Distance(transform.position, closestPlayer.position);

        for(int i = 1; i < players.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, players[i].position);
            if(dist < closestDist)
            {
                closestDist = dist;
                closestPlayer = players[i];
            }
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        //FindClosestPlayer();

        //transform.LookAt(player);
        Vector3 lookVector = transform.position - closestPlayer.position;
        lookVector.y = 0f;
        Quaternion rot = Quaternion.LookRotation(-lookVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1);

        if (!hasAttacked)
        {
            GameObject projectileFired = Instantiate(projectile, new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z), Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(projectileFired);
            Rigidbody projectileBody = projectileFired.GetComponent<Rigidbody>();
            projectileBody.AddForce(transform.forward * (lookVector.magnitude * 0.9f), ForceMode.Impulse);
            projectileBody.AddForce(transform.up * shootUpwardForceMultiplier, ForceMode.Impulse);

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
        //explosion.Despawn();
        Destroy(explosion);
    }

    private bool CheckRange(float sphereRadius, float checkAngle)
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, sphereRadius, playerMask);

        if(playersInRange.Length != 0)
        {
            float closestPlayerRange = float.MaxValue;
            for(int i = 0; i < playersInRange.Length; i++)
            {
                Transform playerTransform = playersInRange[i].transform;
                Vector3 directionToplayer = (playerTransform.position - transform.position).normalized;

                if(Vector3.Angle(transform.forward, directionToplayer) < checkAngle / 2)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                    if(!Physics.Raycast(transform.position + Vector3.up * 2, directionToplayer, distanceToPlayer, obstacleMask))
                    {
                        if(distanceToPlayer < closestPlayerRange)
                        {
                            closestPlayerRange = distanceToPlayer;
                            closestPlayer = playerTransform;
                            playerLastKnownPosition = playerTransform.position;
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
            if(closestPlayerRange < float.MaxValue)
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
        if(!IsHost)
        {
            return;
        }

        //UpdateValidTargets();

        //playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
        //playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

        playerInSightRange = CheckRange(sightRange, sightAngle);
        playerInAttackRange = CheckRange(attackRange, attackAngle);

        if (!playerInSightRange && !playerInAttackRange && !isDead) Patrol();
        if (playerInSightRange && !playerInAttackRange && !isDead) ChasePlayer();
        if (playerInSightRange && playerInAttackRange && !isDead) AttackPlayer();
    }
}
