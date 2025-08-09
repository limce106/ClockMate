namespace Define
{
    public static class Character
    {
        public enum CharacterName
        {
            Hour,
            Milli
        }
        
        public enum CharacterAction
        {
            Move,
            Jump,
            Interact,
            Climb
        }
    }
    public static class UI
    {
        public enum UIType
        {
            FullScreen, // 전체화면 UI
            Windowed // 화면 일부에만 표시되는 UI
        }
    }

    public static class Map
    {
        public enum MapName
        {
            None = 0,
            Desert,
            Glacier,
            Forest,
            ClockTower
        }
    }

    public static class Block
    {
        public enum MovingDirection
        {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Backward
        }
    }

    public static class Battle
    {
        public enum AttackCharacter
        {
            Player = 0,
            Boss = 1
        }

        public enum PhaseType
        {
            SwingAttack,
            FallingAttack,
            PlayerAttack
        }

        public enum PlayerAttackType
        {
            ClockNeedleRecovery,
            CogwheelRevery,
            ClockTowerOperation
        }
    }
}