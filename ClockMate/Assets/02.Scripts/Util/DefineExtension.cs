using System;
using System.Linq;
using UnityEngine;
using static Define.Block;
using static Define.Character;
using static Define.Icon;
using static Define.Map;

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
        public static int GetLastStageIndex(this MapName mapName)
        {
            return mapName switch
            {
                MapName.Desert => 3,
                MapName.Glacier => 3,
                MapName.Forest => 3,
                MapName.ClockTower => 1,
                _ => 0
            };
        }

        public static MapName GetNextMap(this MapName mapName)
        {
            return mapName switch
            {
                MapName.Desert => MapName.Glacier,
                MapName.Glacier => MapName.Forest,
                MapName.Forest => MapName.ClockTower,
                _ => MapName.None
            };
        }

        public static string GetMapSceneName(this MapName mapName)
        {
            return mapName switch
            {
                MapName.Desert => "Desert",
                MapName.Glacier => "Glacier",
                MapName.Forest => "Forest",
                MapName.ClockTower => "ClockTower",
                _ => "None"
            };
        }

        public static bool IsPuzzleMap(this string sceneName)
        {
            return Enum.GetValues(typeof(PuzzleMapName)).Cast<PuzzleMapName>()
                .Any(puzzleMap => sceneName.Equals(puzzleMap.ToString()));
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
            Key.Arrows => "keyboard_arrows",
            Key.Space  => "keyboard_space",
            Key.WASD => "keyboard_wasd",
            Key.AD => "keyboard_ad",
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
