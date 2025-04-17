using System.Collections.Generic;
using static Define.Character;

namespace DefineExtension
{
    public static class CharacterExtension
    {
        private static readonly Dictionary<CharacterId, HashSet<Ability>> _abilityMap = new()
        {
            {
                CharacterId.Hour,
                new HashSet<Ability> { Ability.LiftObject, Ability.CarryMilli }
            },
            {
                CharacterId.Milli,
                new HashSet<Ability> { Ability.NarrowPassage, Ability.WallClimb }
            }
        };

        public static bool HasAbility(this CharacterId id, Ability ability)
        {
            return _abilityMap.TryGetValue(id, out HashSet<Ability> set) && set.Contains(ability);
        }

        public static IEnumerable<Ability> GetAbilities(this CharacterId id)
        {
            return _abilityMap.TryGetValue(id, out HashSet<Ability> set) ? set : System.Array.Empty<Ability>();
        }
    }
}

