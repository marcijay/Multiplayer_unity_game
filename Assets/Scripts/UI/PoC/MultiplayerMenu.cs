using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button joinButton;

    private void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();

            InstanceFinder.ClientManager.StartConnection();
        });

        joinButton.onClick.AddListener(() => InstanceFinder.ClientManager.StartConnection());
    }
}
