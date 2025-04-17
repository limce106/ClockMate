using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사용자 입력을 감지하고 처리
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; } // wasd 
    public bool JumpPressed { get; private set; } // space 키
    public bool InteractPressed { get; private set; } // E 키

    /// <summary>
    /// 카메라 기준 방향으로 회전된 입력 방향 벡터 반환
    /// </summary>
    public Vector3 GetDirectionRelativeTo(Transform reference)
    {
        Vector3 forward = reference.forward;
        Vector3 right = reference.right;
        forward.y = right.y = 0f; // 수평만 유지
        return (forward * MoveInput.y + right * MoveInput.x).normalized;
    }

    void Update()
    {
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        JumpPressed = Input.GetKeyDown(KeyCode.Space);
        InteractPressed = Input.GetKeyDown(KeyCode.E);
    }
}
