using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConstants 
{
    public const float MIN_VELOCITY_SPEED = 0.3f;
    public const float MAX_VELOCITY_SPEED = 5f;
    public const float ACCELERATION = 5f;
    public const float MAX_OXYGEN = 100f;
    public const float OXYGEN_INCREASE_SPEED = 25f;
    public const float OXYGEN_DECREASE_SPEED = 2f;
    public const int POSITIVE = 1;
    public const int ZERO = 0;
    public const int NEGATIVE = -1;

    //AI가 가지고 있어야 하는 최소 산소
    public const float MIN_OXYGEN = 40f;
    //AI가 활동하기에 충분한 산소
    public const float SUFFICIENT_OXYGEN = 60f;
}
