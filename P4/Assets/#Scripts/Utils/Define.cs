using System;
using System.Collections.Generic;

public enum EGameScene
{
    Login,
    Robby,
    Battle,
}

public enum EVersionCheckResult
{
    UpToDate,
    OptionalUpdate,
    ForceUpdate,
    Failed
}

public enum EFileDataType
{
    Option,
    Stage,
}

public enum EUIType
{
    Popup,
    Scene,
}

public enum ETileType : int
{
    None = 0,
    Grass = 1,
    Road = 2,
    Rock = 3,
    Water = 4,
    Spawn = 5,
    Goal = 6,
}

[System.Flags]
public enum EUnitMask
{
    None = 0,
    Melee = 1 << 0, // 1
    Ranged = 1 << 1, // 2
    Trap = 1 << 2, // 4
}

public enum EAddressablesLabel
{
    Scene,
    Prefab_Common,
    Prefab_Battle,
    Table,
    Sprite,
    Tile,
    FieldTable,
    BGM,
    SFX,
}

public enum EMonsterState 
{
    Moving,
    Engaged
}

public enum ETowerState
{
    Idle,
    Attack,
}

public enum EBattleSpeedMode
{
    Normal,
    Placement,
    PausedOrEnd,
    Fast
}

public enum EBGM
{
    TitleBGM,
    LobbyBGM,
    BattleBGM,
}

public static class Define
{
    public const string UNIT_CARD_PATH = "Prefabs_Battle/UnitCard.prefab";
    public const string SLOT_TILE_PATH = "Prefabs_Battle/SlotTile.prefab";

    public const float DEFAULT_COST_SPEED = 1f;
    public const int LIFE_COUNT = 5;

    public static string SceneName(int diff, int stage)
    {
        return $"Scenes/{diff}-{stage}.unity";
    }
}

public static class Config
{
    public const string WebServerUrl = "http://3.24.195.47:5000";
    public const string GameServerHost = "http://3.24.195.47";
    public const int GameServerPort = 5020;
}

public static class WebServerRequest
{
    public const string Login = "/login";
    public const string Version = "/version";
    public const string Android = "/Android";
}