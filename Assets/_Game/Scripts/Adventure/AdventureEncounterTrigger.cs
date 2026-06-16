using UnityEngine;

namespace Isekai12Realms.Adventure
{
    public class AdventureEncounterTrigger : MonoBehaviour
    {
        [SerializeField] private AdventureMonsterController monsterController;
        [SerializeField] private AdventurePlayerController playerController;
        [SerializeField] private float triggerRadius = 96f;

        public void Initialize(AdventureMonsterController monster, AdventurePlayerController player, float radius)
        {
            monsterController = monster;
            playerController = player;
            triggerRadius = radius;
        }

        private void Update()
        {
            if (monsterController == null || playerController == null || monsterController.IsDefeated)
            {
                return;
            }

            float distance = Vector2.Distance(monsterController.GetPosition(), playerController.GetPosition());
            if (distance <= triggerRadius)
            {
                monsterController.TryStartEncounter();
            }
        }
    }
}
