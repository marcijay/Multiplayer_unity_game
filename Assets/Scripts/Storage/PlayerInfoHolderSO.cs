using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerInfoHolderSO : ScriptableObject
{
    [SerializeField]
    private string _playerName;

    [SerializeField]
    private string _providedIPAddress;

    [SerializeField]
    private string _authToken;

    public string PlayerName
    {
        get { return _playerName; }
        set { _playerName = value; }
    }

    public string ProvidedIPAddress
    {
        get { return _providedIPAddress; }
        set { _providedIPAddress = value; }
    }

    public string AuthToken
    {
        get { return _authToken; }
        set { _authToken = value; }
    }
}
