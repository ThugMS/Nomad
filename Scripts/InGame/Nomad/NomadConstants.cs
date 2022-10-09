using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NomadConstants 
{
    public const int CART_BASE_COUNT = 3;//초기 카트 개수
    public const float SPEED = 2f;
    public const float SPEEDWEIGHT = 0.1f;
    public const float SPACE = 2.3f;
    public const int MAX_HP = 100;
    public const int RECOVERY_COST = 300;
    public const float ENGINE_TURN_SPEED = 3;
    public const float ENGINE_MAXHP_PLUSVALUE = 0.7f;
    public const int ENGINE_CAZELIN_COEFFICIENT = 2;

    public const int MAX_INIT_MINERALAMOUNT = 1000;

    public const int CART_UI_DISTANCE = 3;

    public const int BARCORD_HP_DIVIDE = 100;
    //Weapon
    public const int WEAPONCART_MAX_SLOTCOUNT = 4;
    public const int WEAPONCART_INIT_SLOTCOUNT = 4;

    public const string CARTNAME_ENGINECART = "엔진 칸";
    public const string CARTNAME_STARLIGHTCART = "<sprite=5> 스타라이트 칸";
    public const string CARTNAME_CAZELINCART = "<sprite=4> 카젤린 칸";
    public const string CARTNAME_WEAPONCART = "<sprite=11> 무기 칸";
    public const string CARTNAME_BROKEN_STARLIGHTCART = "<sprite=5> 부서진 스타라이트 칸";
    public const string CARTNAME_BROKEN_CAZELINCART = "<sprite=4> 부서진 카젤린 칸";
    public const string CARTNAME_BROKEN_WEAPONCART = "<sprite=11> 부서진 무기 칸";
}
