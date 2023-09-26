using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnWeapon : NetworkBehaviour
{
    private Pawn _pawn;

    private PawnInput _input;

    [SerializeField]
    private float damage;

    [SerializeField]
    private float shotDelay;

    [SerializeField]
    private Transform firePoint;

    private float _timeUntilNextShot;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _pawn = GetComponent<Pawn>();
        _input = GetComponent<PawnInput>();
    }

    [ServerRpc]
    private void ServerFire(Vector3 firePointPosition, Vector3 firePointDirection, Player player)
    {
        if(Physics.Raycast(firePointPosition, firePointDirection, out RaycastHit hit))
        {
            Debug.Log(hit.transform.tag);
            EnemyDrone enemy =  hit.transform.GetComponentInParent<EnemyDrone>();

            if(enemy != null)
            {
                enemy.TakeDamage(damage, player);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if(_timeUntilNextShot <= 0.0f)
        {
            if (_input.fire)
            {
                ServerFire(firePoint.position, firePoint.forward, _pawn.controllingPlayer);

                _timeUntilNextShot = shotDelay;
            }
        }
        else
        {
            _timeUntilNextShot -= Time.deltaTime;
        }
    }
}
