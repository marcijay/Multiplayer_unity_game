using System;
using FishNet;
using System.Collections;
using System.Collections.Generic;
using Messaging.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateGameView : View
{
    [SerializeField] private Button _createButton;

    [SerializeField] private Button _backButton;

    [SerializeField] private Button _logoutButton;

    [SerializeField] private Button _quitButton;

    [SerializeField] private TMP_InputField _gameNameInput;

    [SerializeField] private Toggle _isPublicToggle;

    [SerializeField] private TextMeshProUGUI _messageText;

    [SerializeField] private GameObject _mainField;

    [SerializeField] private GameObject _menu;

    private bool _isMenuOn;

    public override void Initialize()
    {
        _logoutButton.onClick.AddListener(() =>
        {
            DataManager.Instance.ClearPlayerData();

            UIManager.Instance.Show<LoginView>();
        });

        _quitButton.onClick.AddListener(() => { Application.Quit(); });

        _createButton.onClick.AddListener(() =>
        {
            InstanceFinder.TransportManager.Transport.SetClientAddress(
                DataManager.Instance.PlayerData.ProvidedIPAddress);

            //DontDestroyOnLoad(FindObjectOfType<DataManager>());
            StartCoroutine(PostRequest(ConstantValuesHolder.createURL, _gameNameInput.text,
                DataManager.Instance.PlayerData.ProvidedIPAddress, _isPublicToggle.isOn));
        });

        _backButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Show<HostJoinView>(DataManager.Instance.PlayerData.ProvidedIPAddress);
        });

        //_isPublicToggle.onValueChanged.AddListener((value) =>
        //{
        //    if (value)
        //    {
        //        _passcodeInputHolder.SetActive(false);
        //    }
        //    else
        //    {
        //        _passcodeInputHolder.SetActive(true);
        //    }
        //});

        _isMenuOn = false;

        _messageText.text = "";

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

        if (args is string gameName)
        {
            _gameNameInput.text = gameName;
        }

        _menu.SetActive(_isMenuOn);
    }

    private IEnumerator PostRequest(string url, string gameName, string ipAddress, bool isPublic)
    {
        var data = new HostGameData
        {
            gameName = gameName,
            isPublic = isPublic,
            token = "",
            gameIp = ipAddress,
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest uwr = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Authorization", "Bearer " + DataManager.Instance.PlayerData.AuthToken);

            yield return uwr.SendWebRequest();

            Debug.Log(uwr.responseCode);

            if (uwr.responseCode == 200)
            {
                var hostGameResponse = JsonConvert.DeserializeObject<HostGameResponse>(uwr.downloadHandler.text);
                uwr.Dispose();
                PopulateGameInfoScriptableObject(hostGameResponse);

                _messageText.text = "";
                UIManager.Instance.Show<HostJoinView>();

                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();

                _messageText.text = "Cannot create lobby at provided ip address";
                _messageText.color = Color.yellow;

            }
            else if (uwr.responseCode == 0)
            {
                _messageText.text = "Server is currently unavailable, lobby cannot be created";
                _messageText.color = Color.yellow;
                uwr.Dispose();
            }
            else
            {
                uwr.Dispose();
            }
        }
    }

    private void PopulateGameInfoScriptableObject(HostGameResponse hostResponse)
    {
        void PopulatingClosure(GameInfoHolderSO g)
        {
            g.GameId = hostResponse.gameId;
            g.GameName = hostResponse.gameName;
            g.GameToken = hostResponse.gameToken;
            g.InfluenceQueueUrl = hostResponse.influenceQueueUrl;
            g.LiveFeedQueueUrl = hostResponse.liveFeedQueueUrl;
            g.StaticsticsQueueUrl = hostResponse.statisticsQueueUrl;
        }
        PopulatingClosure(DataManager.Instance.GameData);
    }
}