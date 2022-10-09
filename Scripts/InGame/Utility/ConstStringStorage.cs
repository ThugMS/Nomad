using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConstStringStorage
{
    #region Hangeul
    public const string TEXT_CAZELIN = "카젤린";
    public const string TEXT_STARLIGHT = "스타라이트";
    public const string TEXT_DANGER_CHAT_NAME = "[위험]";
    public const string TEXT_GUIDE_CHAT_NAME = "[가이드]";
    public const string TEXT_NOTI_CHAT_NAME = "[알림]";

    public static readonly string[] TEXT_INFO_FIRST_GUIDE = { 
        "여러분에게는 함선이 전부입니다.",
     "함선을 유지하기 위해 우주에서 자원을 모으세요.",
    "그리고 주기적으로 몰려오는 우주 괴물들을 막아 살아남으세요."};

    public static readonly string[] TEXT_INFO_GUIDES = {
        "우주를 탐험하다보면 간혹 새로운 함선 칸을 발견할 수도 있습니다.", 
        "함선 주변에 있으면 산소가 충전됩니다.",
        "조난 당한 팀원의 근처로 이동하면 팀원을 데려올 수 있습니다.",
        "공용 자원을 사용하여 팀원 몰래 자신의 도구를 업그레이드 할 수 있습니다.",
        "함선을 조심히 조종하세요. 자원에 충돌하면 함선이 파괴될 수 있습니다.",
        "우주 공간에서는 자원을 소중히 여겨야합니다. 함선을 이동시키는 것에도 자원이 소모되니까요!",
        "함선이 완전히 고장나면 자신의 자원을 사용해서 복구할 수 있습니다.",
        "함선이 완전히 고장나기 전에 수리하는 것을 추천합니다.",
        "자원을 사용해서 함선을 업그레이드 하세요",
        "생존한 적이 한계치를 초과하면 광폭화됩니다. 정말 조심하세요!"
    };
    public const string TEXT_READYTRUE = "준비 완료";
    public const string TEXT_READYFALSE = "대기 중";

    public const string TEXT_ALIVE = "생존";
    public const string TEXT_DEAD = "사망";
    public static readonly string[] TEXT_DEAD_REASONS = {
        "함선이 장애물과 충돌하여 폭발하였습니다.",
     "함선이 적들에게 공격받아 폭발하였습니다.",
    "팀원들이 모두 사망하였습니다."};
    public static readonly string[] TEXT_TIMES = {"초", "분", "시간"};
    public static readonly string[] TEXT_CARTS = { "None", "엔진 칸", "스타라이트 자원 칸", "카젤린 자원 칸", "무기 칸" };

    public const string TEXT_FULL_INVEN = "가방이 꽉 찼습니다!";

    #endregion

    #region Network
    public const string ISREADY = "IsReady";
    #endregion

    #region Path
    public const string PLAYER_PATH = "Prefabs/Player/Player";
    public const string AI_PLAYER_PATH = "Prefabs/Player/AI";
    public const string NOMAD_PATH = "Prefabs/Nomad/Nomad";
    public const string NOMAD_WEAPONCART_PATH = "Prefabs/Nomad/WeaponCart";
    public const string NOMAD_STARLIGHT_PATH = "Prefabs/Nomad/StarlightCart";
    public const string NOMAD_CAZELINCART_PATH = "Prefabs/Nomad/CazelinCart";
    public const string MONSTER_ATTACKER_PATH = "Prefabs/Monster/AttackerMonster";
    public const string MONSTER_DEFENDER_PATH = "Prefabs/Monster/DefenderMonster";
    public const string MONSTER_SUPPORTER_PATH = "Prefabs/Monster/SupporterMonster";
    public const string MONSTER_POOL_PATH = "Prefabs/Monster/MonsterPoolGO";
    public const string MINERAL_STARLIGHT_PATH = "Prefabs/Mineral/Starlight";
    public const string MINERAL_CAZELINE_PATH = "Prefabs/Mineral/Cazelin";
    public const string MINERAL_STONE_PATH = "Prefabs/Mineral/Stone";
    public const string MAP_MAPRESOURCES_PATH = "Prefabs/Map/MapResources";
    public const string MAP_BACKGROUND_PATH = "Prefabs/Map/Background";
    public const string COMMONUI_PATH = "Prefabs/UI/CommonUICanvas";
    public const string NOMAD_FOLDER_PATH = "Prefabs/Nomad/";
    public const string WEAPON_FOLDER_PATH = "Prefabs/Nomad/Weapon/";
    public const string NOMAD_WEAPON1UI_PATH = "Prefabs/UI/NomadWeaponUI1";

    public const string UI_CHATSLOT_PATH= "Prefabs/UI/ChatSlot";

    public const string UPDATEUPGRADE_EVENTSO_PATH = "ScriptableObject/Event/UpdateUpgradeButtonEventChannelSO";
    public const string SPENDMINERAL_PATH = "ScriptableObject/Event/SpendMineralResultChannelSO";
    public const string CartMineralSO_PATH = "ScriptableObject/Data/CartMineral";
    public const string UPGRADEINFOSO_PATH = "ScriptableObject/Data/UpgradeInfo";
    public const string NOMAD_WEAPONSO_PATH = "ScriptableObject/Data/NomadWeapon";

    public const string PLAYER_ALIVE_IMAGE = "Images/Player/idle_0";
    public const string PLAYER_DEAD_IMAGE = "Images/Player/vanish_0";
    #endregion

    #region Input
    public const string HORIZONTAL = "Horizontal";
    public const string VERTICAL = "Vertical";
    #endregion

    #region SceneName
    public const string INGAME = "InGame";
    public const string ENTER = "Enter";
    #endregion

    #region TAG
    public const string TAG_PLAYER = "Player";
    #endregion

    #region AnimatorParameter
    public const string PLAYER_ANIM_MOVE_X = "MoveX";
    public const string PLAYER_ANIM_MOVE_Y = "MoveY";
    public const string PLAYER_ANIM_ISDEAD = "IsDead";
    public const string PLAYER_ANIM_CHANGE_TOOL_TYPE = "ChangeToolType";
    public const string PLAYER_ANIM_USE_TOOL = "UseTool";
    public const string PLAYER_KEYCODE_E = "IsKeycodeE";
    public const string PLAYER_CLICK_MOUSELEFT = "IsMouseLeft";
    public const string GUN_ANIM_FIRE = "Fire";
    public const string MONSTER_ANIM_MOVE_X = "MoveX";
    public const string MONSTER_ANIM_MOVE_Y = "MoveY";
    public const string MONSTER_ANIM_ISATTACK = "IsAttack";
    public const string MONSTER_ANIM_ISDEAD = "IsDead";
    public const string MONSTER_ANIM_ISMOVE = "IsMove";
    #endregion

    #region UpgradeID
    public const string UPGRADE_TYPE_TOOL = "tool";//tool 업그레이드를 통틀어 말함.(버튼이 한개라서)

    public const string UPGRADE_ID_GUN_ATTACK = "gun_attack";
    public const string UPGRADE_ID_GUN_COOLTIME = "gun_cooltime";
    public const string UPGRADE_ID_PICK_MINING = "pick_mining";
    public const string UPGRADE_ID_EXTRACTOR_MINING = "extractor_mining";
    public const string UPGRADE_ID_REPAIR_EFFICIENCY = "repair_efficiency";

    public const string UPGRADE_ID_INVEN_CAZELIN_CAPACITY = "bag_cazelin_capacity";
    public const string UPGRADE_ID_INVEN_STARLIGHT_CAPACITY = "bag_starlight_capacity";


    public const string UPGRADE_ID_MINERAL_CART_HP = "common_mineral_cart_hp";
    public const string UPGRADE_ID_MINERAL_CART_CAPACITY = "common_mineral_cart_capacity";
    public const string UPGRADE_ID_WEAPON_CART_HP = "common_weapon_cart_hp";
    public const string UPGRADE_ID_WEAPON_CART_CAPACITY = "common_weapon_cart_capacity";
    public const string UPGRADE_ID_WEAPON_CART_ATTACK = "common_weapon_cart_attack";
    public const string UPGRADE_ID_ENGINE_CART_HP = "common_engine_cart_hp";
    #endregion
}
