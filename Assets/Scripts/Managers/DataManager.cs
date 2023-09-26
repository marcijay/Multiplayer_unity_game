using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public PlayerInfoHolderSO PlayerData;

    public GameInfoHolderSO GameData;

    private void Awake()
    {
        Instance = this;
    }

    private void OnApplicationQuit()
    {
        ClearPlayerData();
        ClearGameData();
    }

    public void ClearPlayerData()
    {
        ClearPlayerName();
        ClearAuthToken();
        ClearIPAdderss();
    }

    public void ClearPlayerName()
    {
        PlayerData.PlayerName = null;
    }

    public void ClearAuthToken()
    {
        PlayerData.AuthToken = null;
    }

    public void ClearIPAdderss()
    {
        PlayerData.ProvidedIPAddress = null;
    }

    public void ClearGameData()
    {
        ClearGameId();
        ClearGameName();
        ClearGameToken();
        ClearInfluenceTokenUrl();
        ClearLiveFeedUrl();
        ClearStatisticsUrl();
    }

    private void ClearGameId()
    {
        GameData.GameId = null;
    }

    private void ClearGameName()
    {
        GameData.GameName = null;
    }

    private void ClearGameToken()
    {
        GameData.GameToken = null;
    }

    private void ClearInfluenceTokenUrl()
    {
        GameData.InfluenceQueueUrl = null;
    }

    private void ClearLiveFeedUrl()
    {
        GameData.LiveFeedQueueUrl = null;
    }

    private void ClearStatisticsUrl()
    {
        GameData.StaticsticsQueueUrl = null;
    }
}
