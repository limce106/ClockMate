using UnityEngine;

public class PlayerDebugUI : MonoBehaviour
{
    [SerializeField] private PlayerBase _player;

    private GUIStyle _style;

    private void Awake()
    {
        _style = new GUIStyle();
        _style.fontSize = 16;
        _style.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        if (_player == null) return;

        string debugText = $"<b>▶ PLAYER DEBUG</b>\n" +
                           $"- State: {_player.CurrentStateName}\n" +
                           $"- IsGrounded: {_player.IsGrounded}\n" +
                           $"- JumpCount: {_player.JumpCount}";

        GUI.Label(new Rect(10, 10, 300, 100), debugText, _style);
    }
}