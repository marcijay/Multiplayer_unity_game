using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreView : View
{
    [SerializeField]
    private Button _disconnectButton;

    [SerializeField]
    private Button _quitButton;

    [SerializeField]
    private GameObject _menu;

    [SerializeField]
    private Button _toLobbyButton;

    [SerializeField]
    private TextMeshProUGUI _resultText;

    [SerializeField] private List<TextMeshProUGUI> _playerNamesTexts;

    [SerializeField] private List<TextMeshProUGUI> _playerKillsTexts;

    [SerializeField] private List<TextMeshProUGUI> _playerDeathsTexts;

    [SerializeField] private List<TextMeshProUGUI> _playerScoresTexts;

    private bool _isMenuOn;

    public override void Initialize()
    {
        _quitButton.onClick.AddListener(() =>
        {
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

        if (InstanceFinder.IsHost)
        {
            _toLobbyButton.gameObject.SetActive(true);
            _toLobbyButton.onClick.AddListener(() =>
            {
                LobbyManager.Instance.StopGame();
            }); 
        }
        else
        {
            _toLobbyButton.gameObject.SetActive(false);
        }

        if (Player.LocalInstance.controlledPawn != null)
        {
            Player.LocalInstance.controlledPawn.Controller.DefaultInput.Disable();
            Player.LocalInstance.controlledPawn.Controller.Weapon.WeaponInput.Disable();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

    private void SetScoreboardValues()
    { 
        if (LobbyManager.Instance.ScoreboardData != null)
        {
            var scoreboardData = LobbyManager.Instance.ScoreboardData;
            int i = 0;
            while (i < scoreboardData.data.scoreboard.Count)
            {
                _playerNamesTexts[i].text = scoreboardData.data.scoreboard[i].player.username;
                _playerKillsTexts[i].text = scoreboardData.data.scoreboard[i].kills.ToString();
                _playerDeathsTexts[i].text = scoreboardData.data.scoreboard[i].deaths.ToString();
                _playerScoresTexts[i].text = scoreboardData.data.scoreboard[i].points.ToString();

                i++;
            }
            while(i < ConstantValuesHolder.maxPlayerCount)
            {
                _playerNamesTexts[i].text = "";
                _playerKillsTexts[i].text = "";
                _playerDeathsTexts[i].text = "";
                _playerScoresTexts[i].text = "";

                i++;
            }
        }
    }

    public override void Show(object args = null)
    {
        if (args is string message)
        {
            _resultText.text = message;
            SetScoreboardValues();
        }

        base.Show(args);
    }
}
