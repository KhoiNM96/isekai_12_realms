using UnityEngine;

namespace Isekai12Realms.Battle
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Battle Animation Settings")]
    public class BattleAnimationSettings : ScriptableObject
    {
        public float tileSwapDuration = 0.12f;
        public float invalidSwapReturnDuration = 0.10f;
        public float tilePopDuration = 0.14f;
        public float tileDropDuration = 0.18f;
        public float cascadeDelay = 0.08f;
        public float damageNumberDuration = 0.75f;
        public float characterHitShakeDuration = 0.15f;
        public float characterHitShakeStrength = 12f;
        public float skillFlashDuration = 0.18f;
        public float resultPopupDelay = 0.35f;

        public static BattleAnimationSettings CreateDefault()
        {
            return CreateInstance<BattleAnimationSettings>();
        }
    }
}
