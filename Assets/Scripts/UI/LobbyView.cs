using FishNet;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Messaging;
using Messaging.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : View
{
    [SerializeField] private Button _disconnectButton;

    [SerializeField] private Button _quitButton;

    [SerializeField] private Button toggleReadyButton;

    [SerializeField] private Button startGameButton;

    [SerializeField] private List<TextMeshProUGUI> waitingTexts;

    [SerializeField] private List<TextMeshProUGUI> playerNames;

    [SerializeField] private List<Toggle> plaerReadyToggles;

    [SerializeField] private GameObject _menu;

    [SerializeField] private TextMeshProUGUI _gameName;

    [SerializeField] private TextMeshProUGUI _gameIP;

    [SerializeField] private TextMeshProUGUI _gameToken;

    [SerializeField] private TextMeshProUGUI _selectedMapText;

    [SerializeField] private TMP_Dropdown _mapSelector;

    private bool _isMenuOn;

    public override void Initialize()
    {
        _isMenuOn = false;

        _gameName.text = LobbyManager.Instance.gameName;
        _gameIP.text = LobbyManager.Instance.gameIP;
        _gameToken.text = "Token: " + LobbyManager.Instance.gameToken;
        _selectedMapText.text = LobbyManager.Instance.SelectedMapName;

        _mapSelector.options.Clear();
        foreach (var name in LobbyManager.Instance.MapNames)
        {
            _mapSelector.options.Add(new TMP_Dropdown.OptionData() {text = name});
            Debug.Log(name);
        }

        toggleReadyButton.onClick.AddListener(
            () => Player.LocalInstance.ServerSetIsReady(!Player.LocalInstance.isReady));

        _quitButton.onClick.AddListener(() => { Application.Quit(); });

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

        if (InstanceFinder.IsHost)
        {
            startGameButton.onClick.AddListener(() => LobbyManager.Instance.StartGame());
            startGameButton.gameObject.SetActive(true);

            _mapSelector.gameObject.SetActive(true);
            _selectedMapText.gameObject.SetActive(false);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
            _mapSelector.gameObject.SetActive(false);
            _selectedMapText.gameObject.SetActive(true);
        }

        base.Initialize();
    }

    private void Awake()
    {
        UpdateReadyView();
        LobbyManager.Instance.SendMapChoiceUpdatedEvent();
    }

    public void HandleMapSelectorValueChange(int value)
    {
        if (value < LobbyManager.Instance.MapNames.Count && value > -1)
        {
            LobbyManager.Instance.SelectedMapName = LobbyManager.Instance.MapNames[value];
        }
        else
        {
            LobbyManager.Instance.SelectedMapName = LobbyManager.Instance.MapNames[2];
        }

        switch (value)
        {
            case 0:
                LobbyManager.Instance.SelectedMapSceneName = ConstantValuesHolder.mapArenaSceneName;
                break;
            case 1:
                LobbyManager.Instance.SelectedMapSceneName = ConstantValuesHolder.mapWarehouseSceneName;
                break;
            case 2:
                LobbyManager.Instance.SelectedMapSceneName = ConstantValuesHolder.mapTestingAreaSceneName;
                break;
            default:
                LobbyManager.Instance.SelectedMapSceneName = ConstantValuesHolder.mapTestingAreaSceneName;
                break;
        }

        LobbyManager.Instance.SendMapChoiceUpdatedEvent();
    }

    private void UpdateReadyView()
    {
        for (int i = 0; i < ConstantValuesHolder.maxPlayerCount; i++)
        {
            if (LobbyManager.Instance.players.Count > i)
            {
                plaerReadyToggles[i].gameObject.SetActive(true);
                playerNames[i].gameObject.SetActive(true);
                plaerReadyToggles[i].isOn = LobbyManager.Instance.players[i].isReady;
                playerNames[i].text = LobbyManager.Instance.players[i].username;
                waitingTexts[i].gameObject.SetActive(false);
            }
            else
            {
                plaerReadyToggles[i].gameObject.SetActive(false);
                playerNames[i].gameObject.SetActive(false);
                waitingTexts[i].gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isMenuOn = !_isMenuOn;
            _menu.SetActive(_isMenuOn);
        }

        if (InstanceFinder.IsHost)
        {
            startGameButton.interactable = LobbyManager.Instance.canStart;
        }
    }

    private void LateUpdate()
    {
        UpdateReadyView();
        _selectedMapText.text = LobbyManager.Instance.SelectedMapName;
    }
}