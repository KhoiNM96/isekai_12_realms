using UnityEngine;

namespace Isekai12Realms.Enemies
{
    public enum EnemyAIDifficulty
    {
        Easy,
        Normal,
        Hard,
        Boss
    }

    [CreateAssetMenu(menuName = "Isekai 12 Realms/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public int level;
        public int maxHp;
        public int attack;
        public int defense;
        public int maxMana;
        public string spriteAssetId;
        public EnemyAIDifficulty difficulty;
    }
}
