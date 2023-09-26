using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneProjectile : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody body;

    [SerializeField]
    private GameObject explosion;

    [SerializeField]
    private AudioSource audioSrc;

    [SerializeField]
    private AudioClip explosionClip;

    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    [Range(0f, 1f)]
    private float bounciness;

    [SerializeField]
    private bool useGravity;

    [SerializeField]
    private float explosionDamage;

    [SerializeField]
    private float explosionRange;

    [SerializeField]
    private int maxBounces;

    [SerializeField]
    private float maxLifetime;

    [SerializeField]
    private bool expolodeOnTouch = true;

    private int bounces;
    private PhysicMaterial material;
    private bool exploded = false;

    private void Setup()
    {
        material = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };

        GetComponent<SphereCollider>().material = material;
        body.useGravity = useGravity;
    }

    private void Explode()
    {
        if (!exploded)
        {
            exploded = true;
            if (explosion != null)
            {
                HandleExplosion(transform.position);
                //audioSrc.Stop();
                //audioSrc.clip = explosionClip;
                //audioSrc.Play();
                //var explosionEffect = Instantiate(explosion, transform.position, Quaternion.identity);
                //var mesh = GetComponent<MeshRenderer>();
                //if (mesh is not null)
                //{
                //    Destroy(GetComponent<MeshRenderer>());
                //}
                //StartCoroutine(DespawnExplosion(explosionEffect, 1f));
            }

            Collider[] targets = Physics.OverlapSphere(transform.position, explosionRange, playerMask);

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].GetComponent<Pawn>().ReceiveDamage(explosionDamage);
            }

            Invoke(nameof(DestroySelf), 1f);
        }
    }

    [ObserversRpc]
    private void HandleExplosion(Vector3 posittion)
    {
        audioSrc.Stop();
        audioSrc.clip = explosionClip;
        audioSrc.loop = false;
        audioSrc.Play();

        var vfx = Instantiate(explosion, posittion, Quaternion.identity);
        var mesh = GetComponent<MeshRenderer>();
        if (mesh is not null)
        {
            Destroy(GetComponent<MeshRenderer>());
        }
        Destroy(GetComponent<TrailRenderer>());
        StartCoroutine(DespawnExplosion(vfx, 0.9f));
    }

    private void Awake()
    {
        audioSrc.Play();
    }

    private void DestroySelf()
    {
        this.Despawn();
        //Destroy(gameObject);
    }

    private IEnumerator DespawnExplosion(GameObject explosion, float delay)
    {
        yield return new WaitForSeconds(delay);
        //explosion.Despawn();
        Destroy(explosion);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        bounces++;

        if(collision.collider.CompareTag("Player") && expolodeOnTouch)
            Explode();
    }

    void Start()
    {
        Setup();
    }

    void Update()
    {
        if (!IsHost)
        {
            return;
        }

        if (bounces > maxBounces && !exploded)
            Explode();

        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0 && !exploded)
            Explode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
