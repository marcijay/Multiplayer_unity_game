using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleLobbyView : View
{
    [SerializeField]
    private Button toggleReadyButton;

    [SerializeField]
    private TextMeshProUGUI toggleReadyButtonText;

    [SerializeField]
    private Button startGameButton;

    public override void Initialize()
    {
        toggleReadyButton.onClick.AddListener(() => PlayerScript.LocalInstance.ServerSetIsReady(!PlayerScript.LocalInstance.isReady));

        if (InstanceFinder.IsHost)
        {
            startGameButton.onClick.AddListener(() => LobbyManager.Instance.StartGame());

            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
        base.Initialize();
    }

    private void Update()
    {
        if (!Initialized) return;

        toggleReadyButtonText.color = PlayerScript.LocalInstance.isReady ? Color.green : Color.red;

        startGameButton.interactable = LobbyManager.Instance.canStart;
    }
}
