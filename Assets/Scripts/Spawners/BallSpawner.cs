using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BallSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject spawnPoint;

    [SerializeField]
    private GameObject ballPrefab;

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        Pawn player = other.GetComponent<Pawn>();
        if (player != null)
        {
            //GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>(ConstantValuesHolder.addressableDroneProjectileName).WaitForCompletion();

            GameObject ballInstance = Instantiate(ballPrefab, spawnPoint.transform.position, Quaternion.identity);

            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ballInstance, UnityEngine.SceneManagement.SceneManager.GetSceneByName(ConstantValuesHolder.mapTestingAreaSceneName));

            Spawn(ballInstance);
        }
    }
}
