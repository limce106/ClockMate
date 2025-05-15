// using UnityEngine;
// using UnityEngine.Serialization;
//
// public class PlayerDebugUI : MonoBehaviour
// {
//     [FormerlySerializedAs("_player")] [SerializeField] private CharacterBase character;
//
//     private GUIStyle _style;
//
//     private void Awake()
//     {
//         _style = new GUIStyle();
//         _style.fontSize = 16;
//         _style.normal.textColor = Color.white;
//     }
//
//     private void OnGUI()
//     {
//         if (character == null) return;
//
//         string debugText = $"<b>▶ PLAYER DEBUG</b>\n" +
//                           // $"- State: {character.CurrentStateName}\n" +
//                            $"- IsGrounded: {character.IsGrounded}\n" +
//                            $"- JumpCount: {character.JumpCount}";
//
//         GUI.Label(new Rect(10, 10, 300, 100), debugText, _style);
//     }
// }