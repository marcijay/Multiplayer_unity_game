using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public sealed class LoginView : View
{
    [SerializeField]
    private TMP_InputField _loginInput;

    [SerializeField]
    private TMP_InputField _passwordInput;

    [SerializeField]
    private Button _loginButton;

    [SerializeField]
    private Button _quitButton;

    [SerializeField]
    private TextMeshProUGUI _messageText;

    [SerializeField]
    private Selectable _firstInput;

    private EventSystem system;

    public override void Initialize()
    {
        _loginButton.onClick.AddListener(() =>
        {
            StartCoroutine(PostRequest(ConstantValuesHolder.authURL, _loginInput.text, _passwordInput.text));
        });

        _quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        base.Initialize();
    }

    private void Awake()
    {
        system = EventSystem.current;
        _firstInput.Select();

        if(DataManager.Instance.PlayerData.AuthToken != null && !DataManager.Instance.PlayerData.AuthToken.Equals(""))
        {
            UIManager.Instance.Show<HostJoinView>();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            Selectable previous = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if(previous != null)
            {
                previous.Select();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                next.Select();
            }
        }
        //else if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    _loginButton.onClick.Invoke();
        //    Debug.Log("Button clicked");
        //}
    }

    public override void Show(object args = null)
    {
        if(args is string message)
        {
            _messageText.text = message;
        }

        base.Show(args);
    }

    private IEnumerator PostRequest(string url, string login, string password)
    {
        UnityWebRequest uwr = new UnityWebRequest(url, "POST");
        string base64Hash = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Authorization", "Basic " + base64Hash);

        yield return uwr.SendWebRequest();

        Debug.Log(uwr.responseCode);
        Debug.Log(uwr.error);
        Debug.Log(uwr.result);

        if (uwr.responseCode == 200)
        {
            DataManager.Instance.PlayerData.PlayerName = _loginInput.text;
            DataManager.Instance.PlayerData.AuthToken = uwr.downloadHandler.text;
            uwr.Dispose();

            _messageText.text = "";
            _loginInput.text = "";
            _passwordInput.text = "";

            _loginInput.image.color = Color.white;
            _passwordInput.image.color = Color.white;
            UIManager.Instance.Show<HostJoinView>();
        }
        else if(uwr.responseCode == 401)
        {
            _messageText.text = "Wrong login or password";
            _messageText.color = Color.red;
            _loginInput.image.color = Color.red;
            _passwordInput.image.color = Color.red;
            uwr.Dispose();
        }
        else if(uwr.responseCode == 0)
        {
            _messageText.text = "Unable to connect to server";
            _messageText.color = Color.yellow;
            _loginInput.image.color = Color.white;
            _passwordInput.image.color = Color.white;
            uwr.Dispose();
        }
        else
        {
            uwr.Dispose();
        }
    } 
}
