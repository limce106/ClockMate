using UnityEngine;

namespace Define
{
    public static class Character
    {
        /// <summary>
        /// 캐릭터 타입 정의 (아워/밀리)
        /// </summary>
        public enum CharacterId
        {
            Hour = 0,
            Milli = 1
        }
        
        
        /// <summary>
        /// 캐릭터가 사용 가능한 고유 능력
        /// </summary>
        public enum Ability
        {
            None = 0,
            LiftObject,
            CarryMilli,
            NarrowPassage,
            WallClimb,
        }
        
        /// <summary>
        /// 캐릭터 상태 (애니메이션 전환 기준)
        /// </summary>
        public enum State
        {
            Idle = 0,
            Walk,
            Jump,
            DoubleJump,
            Fall,
            Land,
            LiftStart,
            Carrying,
            ClimbStart,
            Climbing,
            ClimbEnd,
            Attack,
            Dead
        }

        /// <summary>
        /// 캐릭터가 바닥에 있는지 여부
        /// </summary>
        public enum GroundState
        {
            Grounded = 0,
            InAir,
            Landing
        }
        
        /// <summary>
        /// 공격 방식
        /// </summary>
        public enum AttackType
        {
            None = 0,
            Basic // 기본 공격
        }
        
        /// <summary>
        /// Lift 가능한 대상 종류
        /// </summary>
        public enum LiftTargetType
        {
            None = 0,
            Milli,
            Object
        }
    }
}