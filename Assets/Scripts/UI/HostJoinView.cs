using FishNet;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public sealed class HostJoinView : View
{
    [SerializeField]
    private Button _hostButton;

    [SerializeField]
    private Button _joinButton;

    [SerializeField]
    private Button _logoutButton;

    [SerializeField]
    private Button _quitButton;

    [SerializeField]
    private TMP_InputField _ipInput;

    [SerializeField]
    private TextMeshProUGUI _messageText;

    [SerializeField]
    private GameObject _mainField;

    [SerializeField]
    private GameObject _menu;

    private bool _isMenuOn;

    public override void Initialize()
    {
        _logoutButton.onClick.AddListener(() =>
        {
            DataManager.Instance.ClearPlayerData();

            UIManager.Instance.Show<LoginView>();
        });

        _quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        _hostButton.onClick.AddListener(() => 
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            bool isValid = false;
            foreach (IPAddress ip in host.AddressList)
            {
                Debug.Log(ip);
                if(ip.ToString() == _ipInput.text)
                {
                    isValid = true;
                }
            }
            if(_ipInput.text == "localhost")
            {
                isValid = true;
            }

            if (!isValid)
            {
                _messageText.text = "Provided ip address is not suitable for hosting, try another";
                _messageText.color = Color.yellow;
            }
            else
            {
                DataManager.Instance.PlayerData.ProvidedIPAddress = _ipInput.text;
                UIManager.Instance.Show<CreateGameView>($"{DataManager.Instance.PlayerData.PlayerName}'s game");
            }
        });

        _joinButton.onClick.AddListener(() =>
        {
            InstanceFinder.TransportManager.Transport.SetClientAddress(_ipInput.text);
            InstanceFinder.ClientManager.StartConnection();

            _messageText.text = "There is no lobby available at provided ip address";
            _messageText.color = Color.yellow;
        });

        InstanceFinder.TransportManager.Transport.SetMaximumClients(ConstantValuesHolder.maxPlayerCount);

        _isMenuOn = false;
        _messageText.text = "";
        _ipInput.text = "localhost";

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

    public override void Show(object args = null)
    {
        base.Show(args);

        if (args is string ip)
        {
            _ipInput.text = ip;
        }

        _menu.SetActive(_isMenuOn);
    }
}
