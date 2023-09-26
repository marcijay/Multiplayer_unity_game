using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ConsumableSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject _spawnPoint;

    [SerializeField]
    private string _consumableName;

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        Pawn player = other.GetComponent<Pawn>();
        if (player != null)
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(_consumableName).WaitForCompletion();

            GameObject prefabInstance = Instantiate(prefab, _spawnPoint.transform.position, Quaternion.identity);

            Spawn(prefabInstance);
        }
    }
}
