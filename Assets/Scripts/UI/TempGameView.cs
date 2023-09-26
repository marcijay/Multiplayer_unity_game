using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempGameView : View
{
    [SerializeField]
    private Button _disconnectButton;

    [SerializeField]
    private Button _quitButton;

    [SerializeField]
    private GameObject _menu;

    private bool _isMenuOn;

    public override void Initialize()
    {
        _quitButton.onClick.AddListener(() =>
        {
            DataManager.Instance.ClearPlayerData();

            Application.Quit();
        });

        _disconnectButton.onClick.AddListener(() =>
        {
            if (InstanceFinder.IsServer)
            {
                InstanceFinder.ServerManager.StopConnection(true);
            }
            else if (InstanceFinder.IsClient)
            {
                InstanceFinder.ClientManager.StopConnection();
            }
        });

        _isMenuOn = false;

        base.Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isMenuOn = !_isMenuOn;
            _menu.SetActive(_isMenuOn);
        }
    }
}
