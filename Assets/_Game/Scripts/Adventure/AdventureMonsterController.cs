using Isekai12Realms.Enemies;
using Isekai12Realms.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Adventure
{
    public class AdventureMonsterController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition enemyDefinition;
        [SerializeField] private bool isBoss;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image spriteImage;

        private AdventureMapService adventureMapService;
        private AdventurePlayerController playerController;
        private PlatformSegmentData platform;
        private MonsterSpawnData spawnData;
        private Vector2 spawnPosition;
        private Vector2 patrolCenter;
        private float patrolDistance = 200f;
        private float patrolSpeed = 60f;
        private float patrolDirection = 1f;
        private bool defeated;
        private bool hidden;
        private bool encounterStarted;

        public EnemyDefinition EnemyDefinition => enemyDefinition;
        public bool IsBoss => isBoss;
        public bool IsDefeated => defeated;
        public string EncounterId => enemyDefinition != null ? enemyDefinition.id : name;
        public Vector2 GetPosition() => rectTransform != null ? rectTransform.anchoredPosition : (Vector2)transform.localPosition;

        public void Initialize(EnemyDefinition enemy, bool boss, AdventureMapService service, AdventurePlayerController player, PlatformSegmentData ground, MonsterSpawnData spawn)
        {
            enemyDefinition = enemy;
            isBoss = boss;
            adventureMapService = service;
            playerController = player;
            platform = ground;
            spawnData = spawn;
            rectTransform = GetComponent<RectTransform>();
            spriteImage = GetComponent<Image>();
            patrolDistance = spawn != null ? Mathf.Max(80f, spawn.patrolDistance) : 200f;
            patrolSpeed = boss ? 50f : 72f;
            hidden = spawn != null && spawn.initiallyHidden;
            if (spriteImage != null && enemyDefinition != null)
            {
                Sprite sprite = AssetSpriteBinder.GetSprite(enemyDefinition.spriteAssetId);
                if (sprite != null)
                {
                    spriteImage.sprite = sprite;
                    spriteImage.preserveAspect = true;
                }
                else
                {
                    spriteImage.color = boss ? new Color(0.76f, 0.48f, 1f, 1f) : new Color(0.6f, 0.86f, 0.48f, 1f);
                }
            }

            spawnPosition = spawn != null ? spawn.spawnPosition : Vector2.zero;
            patrolCenter = platform != null ? new Vector2(platform.position.x + platform.size.x * 0.5f, platform.position.y) : spawnPosition;
            if (rectTransform != null && platform != null)
            {
                float halfMonsterWidth = rectTransform.rect.width * 0.5f;
                float maxPatrol = Mathf.Max(60f, platform.size.x * 0.5f - halfMonsterWidth - 8f);
                patrolDistance = Mathf.Min(patrolDistance, maxPatrol);
            }
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = spawnPosition;
            }

            SetVisible(!hidden);
            UpdateLabel();
        }

        public void SetVisible(bool visible)
        {
            hidden = !visible;
            if (gameObject != null)
            {
                gameObject.SetActive(visible && !defeated);
            }
        }

        public void Reveal()
        {
            hidden = false;
            gameObject.SetActive(true);
        }

        public void MarkDefeated()
        {
            defeated = true;
            encounterStarted = false;
            gameObject.SetActive(false);
        }

        public void ResetEncounterLock()
        {
            encounterStarted = false;
            if (!defeated)
            {
                gameObject.SetActive(true);
            }
        }

        public void ResetToSpawn()
        {
            defeated = false;
            encounterStarted = false;
            patrolCenter = platform != null ? new Vector2(platform.position.x + platform.size.x * 0.5f, platform.position.y) : spawnPosition;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = spawnPosition;
            }
            SetVisible(!hidden);
        }

        public void SetPatrolCenter(Vector2 center)
        {
            patrolCenter = center;
        }

        private void Update()
        {
            if (defeated || hidden || rectTransform == null)
            {
                return;
            }

            Patrol();
            TryStartEncounter();
        }

        private void Patrol()
        {
            Vector2 position = rectTransform.anchoredPosition;
            float halfMonsterWidth = rectTransform.rect.width * 0.5f;
            float edgeLeft = patrolCenter.x - patrolDistance;
            float edgeRight = patrolCenter.x + patrolDistance;
            if (platform != null)
            {
                edgeLeft = platform.position.x + halfMonsterWidth;
                edgeRight = platform.position.x + platform.size.x - halfMonsterWidth;
            }

            position.x += patrolDirection * patrolSpeed * Time.deltaTime;
            if (position.x <= edgeLeft)
            {
                position.x = edgeLeft;
                patrolDirection = 1f;
            }
            else if (position.x >= edgeRight)
            {
                position.x = edgeRight;
                patrolDirection = -1f;
            }

            if (platform != null)
            {
                float platformTop = platform.position.y + platform.size.y * 0.5f;
                position.y = platformTop + (rectTransform.rect.height * 0.5f);
            }

            rectTransform.anchoredPosition = position;
            transform.localScale = new Vector3(patrolDirection < 0f ? -1f : 1f, 1f, 1f);
        }

        public void TryStartEncounter()
        {
            if (playerController == null || adventureMapService == null)
            {
                return;
            }

            Vector2 playerPosition = playerController.GetPosition();
            float distance = Vector2.Distance(playerPosition, rectTransform.anchoredPosition);
            float encounterRadius = isBoss ? 150f : 110f;
            if (distance > encounterRadius)
            {
                return;
            }

            if (encounterStarted)
            {
                return;
            }

            encounterStarted = true;
            if (!adventureMapService.StartEncounter(this))
            {
                encounterStarted = false;
            }
        }

        private void UpdateLabel()
        {
            TextMeshProUGUI label = GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && enemyDefinition != null)
            {
                label.text = isBoss ? $"Boss\n{enemyDefinition.displayName}" : enemyDefinition.displayName;
            }
        }
    }
}
