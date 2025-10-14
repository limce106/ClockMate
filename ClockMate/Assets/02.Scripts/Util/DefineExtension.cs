using System;
using Define;
using UnityEngine;
using static Define.Block;
using static Define.Character;
using static Define.Icon;

namespace DefineExtension
{
    public static class CharacterExtension
    {
        public static Type GetCharacterType(this CharacterName name)
        {
            return name switch
            {
                CharacterName.Hour => typeof(Hour),
                CharacterName.Milli => typeof(Milli),
            };
        }

        public static bool IsPlayerCollider(this Collider collider)
        {
            return collider.CompareTag("Hour") || collider.CompareTag("Milli");
        }
    }
    public static class MapExtension
    {
        public static int GetLastStageIndex(this Map.MapName mapName)
        {
            return mapName switch
            {
                Map.MapName.Desert => 3,
                Map.MapName.Glacier => 3,
                Map.MapName.Forest => 3,
                Map.MapName.ClockTower => 1,
                _ => 0
            };
        }

        public static Map.MapName GetNextMap(this Map.MapName mapName)
        {
            return mapName switch
            {
                Map.MapName.Desert => Map.MapName.Glacier,
                Map.MapName.Glacier => Map.MapName.Forest,
                Map.MapName.Forest => Map.MapName.ClockTower,
                _ => Map.MapName.None
            };
        }

        public static string GetMapSceneName(this Map.MapName mapName)
        {
            return mapName switch
            {
                Map.MapName.Desert => "Desert",
                Map.MapName.Glacier => "Glacier",
                Map.MapName.Forest => "Forest",
                Map.MapName.ClockTower => "ClockTower",
                _ => "None"
            };
        }
    }

    public static class BlockExtension
    {
        public static Vector3 GetMovingDirectionVector(this MovingDirection direction)
        {
            return direction switch
            {
                MovingDirection.Up => Vector3.up,
                MovingDirection.Down => Vector3.down,
                MovingDirection.Left => Vector3.left,
                MovingDirection.Right => Vector3.right,
                MovingDirection.Forward => Vector3.forward,
                MovingDirection.Backward => Vector3.back,
                _ => Vector3.right
            };
        }
    }

    public static class NetworkExtension
    {
        public static void RunNetworkOrLocal(Action localAction, Action networkAction, bool isMine = true)
        {
            if (NetworkManager.Instance.IsInRoomAndReady() && isMine)
            {
                networkAction?.Invoke();
            }
            else
            {
                localAction?.Invoke();
            }
        }
    }

    public static class IconExtension
    {
        private const string Base = "UI/Sprites/Key";
        
        // Key -> 파일명의 앞부분 매핑
        private static string ToToken(this Key key) => key switch
        {
            Key.E      => "interact_active",
            Key.Q      => "keyboard_q",
            Key.W      => "keyboard_w",
            Key.A      => "keyboard_a",
            Key.S      => "keyboard_s",
            Key.D      => "keyboard_d",
            Key.Space  => "keyboard_space",
            Key.Arrows => "keyboard_arrows",
            _               => null
        };

        // Style -> 파일명 뒷부분 매핑
        private static string ToSuffix(this Style style) => style switch
        {
            Style.Default => "", // 디폴트는 접미사x
            Style.Outline => "_outline",
            Style.Filled  => "_filled",
            _                  => ""
        };

        // 경로 조합: "UI/Sprites/Key/{token}{suffix}"
        public static string GetPath(this Key key, Style style = Style.Default)
        {
            string token = key.ToToken();
            if (string.IsNullOrEmpty(token)) return string.Empty;

            string suffix = style.ToSuffix();
            return $"{Base}/{token}{suffix}";
        }

        public static Sprite LoadSprite(this Key key, Style style = Style.Default)
        {
            Sprite sprite = Resources.Load<Sprite>(GetPath(key, style));
            if (sprite == null)
            {
                Debug.LogError($"[IconExtension] Failed to load sprite: {GetPath(key, style)}");
            }
            return sprite;
        }
    }
}
