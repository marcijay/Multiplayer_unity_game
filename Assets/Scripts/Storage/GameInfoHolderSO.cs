using System.Collections;
using System.Collections.Generic;
using Messaging.Dto;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class GameInfoHolderSO : ScriptableObject
{
    [SerializeField] private string _gameId;

    [SerializeField] private string _gameName;

    [SerializeField] private string _gameToken;

    [FormerlySerializedAs("_influenceTokenQueueUrl")] [SerializeField] private string influenceQueueUrl;

    [SerializeField] private string _liveFeedQueueUrl;

    [SerializeField] private string _staticsticsQueueUrl;

    public string GameId
    {
        get { return _gameId; }
        set { _gameId = value; }
    }

    public string GameName
    {
        get { return _gameName; }
        set { _gameName = value; }
    }

    public string GameToken
    {
        get { return _gameToken; }
        set { _gameToken = value; }
    }

    public string InfluenceQueueUrl
    {
        get { return influenceQueueUrl; }
        set { influenceQueueUrl = value; }
    }

    public string LiveFeedQueueUrl
    {
        get { return _liveFeedQueueUrl; }
        set { _liveFeedQueueUrl = value; }
    }

    public string StaticsticsQueueUrl
    {
        get { return _staticsticsQueueUrl; }
        set { _staticsticsQueueUrl = value; }
    }
}