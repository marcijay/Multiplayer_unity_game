using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Spawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject _spawnPoint;

    [SerializeField]
    private GameObject _enemyPrefab;

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        Pawn player = other.GetComponent<Pawn>();
        if(player != null)
        {
            //GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressableEnemyDroneName).WaitForCompletion();

            GameObject enemyInstance = Instantiate(_enemyPrefab, _spawnPoint.transform.position, Quaternion.identity);

            Spawn(enemyInstance);
        }
    }
}
