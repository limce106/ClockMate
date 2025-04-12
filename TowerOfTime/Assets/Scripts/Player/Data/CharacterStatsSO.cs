using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("이동")]
    public float walkSpeed = 2f;

    [Header("점프")] 
    public bool canDoubleJump = true;
    public float jumpPower = 5f;
    public float doubleJumpPower = 4f;
    
    [Header("벽 타기")]
    public float climbSpeed = 2f;
    
    [Header("운반 중 속도 패널티")]
    [Range(0f, 1f)]
    public float carrySpeedPenalty = 0.5f;
}
