using System.Collections;
using System.Collections.Generic;
using Messaging;
using UnityEngine;

public static class ConstantValuesHolder
{
    static ConstantValuesHolder()
    {
        var hostConfig = AwsConfigUtils.ProvideAwsConfig().hostConfig;
        _backendUrlPrefix = hostConfig.remoteBackendEnabled ? hostConfig.backendOutsideUrl : "localhost:8080";
    }

    #region - Connection -

    private static string _backendUrlPrefix;
    public static string authURL => $"{_backendUrlPrefix}/auth";
    public static string graphQLURL => $"{_backendUrlPrefix}/graphql";
    public static string createURL => $"{_backendUrlPrefix}/api/game/host";
    public static readonly int maxPlayerCount = 4;
    public static readonly Vector3 playerSpawnPoint = new Vector3(3, 0, 3);

    #endregion

    #region - Maps -

    public static readonly string mapTestingAreaSceneName = "Online_Map_1";
    public static readonly string mapArenaSceneName = "Online_Arena";
    public static readonly string mapWarehouseSceneName = "Online_Kill_All";
    public static readonly string onlinelobbySceneName = "Online_LobbyScene";

    #endregion

    #region - Addressables -

    public static readonly string addressablePawnName = "PlayerCharacter"; //"Pawn";
    public static readonly string addressableEnemyDroneName = "Enemy_drone";
    public static readonly string addressableDroneProjectileName = "Projectile";
    public static readonly string addressableRusherProjectileName = "Bullet";

    #endregion

    #region - Animations -

    public static readonly string verticalMovementInputValue = "VerticalMovementInput";
    public static readonly string horizontalMovementInputValue = "HorizontalMovementInput";
    public static readonly string isInFortifieddStance = "Fortified";
    public static readonly string isRunning = "Running";
    public static readonly string isJumping = "Jumping";
    public static readonly string isJumpingTrigger = "Jump";
    public static readonly string fireTrigger = "Shoot";
    public static readonly string reloadTrigger = "Reload";

    #endregion

    #region - Consumables -

    public static readonly string ammmunitionBox = "AmmoBox";
    public static readonly string healthPack = "HealthPack";
    public static readonly string armorPack = "Armor";

    #endregion
}