using System;
using Define;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define.Block;
using static Define.Character;

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
}
