using System;
using Isekai12Realms.Enemies;
using Isekai12Realms.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Isekai12Realms.Adventure
{
    public class AdventureMonsterView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private EnemyDefinition enemyDefinition;
        [SerializeField] private bool isBoss;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image spriteImage;
        [SerializeField] private float patrolRadius = 18f;
        [SerializeField] private float encounterRadius = 110f;

        private AdventureMapService adventureMapService;
        private AdventurePlayerController playerController;
        private Vector2 homePosition;
        private bool defeated;
        private float patrolSeed;

        public EnemyDefinition EnemyDefinition => enemyDefinition;
        public bool IsBoss => isBoss;

        public void Initialize(EnemyDefinition enemy, bool boss, AdventureMapService service, AdventurePlayerController player)
        {
            enemyDefinition = enemy;
            isBoss = boss;
            adventureMapService = service;
            playerController = player;
            rectTransform = GetComponent<RectTransform>();
            spriteImage = GetComponent<Image>();
            if (spriteImage != null && enemyDefinition != null)
            {
                Sprite sprite = AssetSpriteBinder.GetSprite(enemyDefinition.spriteAssetId);
                if (sprite != null)
                {
                    spriteImage.sprite = sprite;
                    spriteImage.preserveAspect = true;
                    spriteImage.color = Color.white;
                }
            }

            patrolSeed = UnityEngine.Random.Range(0f, 10f);
            homePosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        }

        public void SetSpawnPosition(Vector2 position)
        {
            homePosition = position;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }
        }

        public void MarkDefeated()
        {
            defeated = true;
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (defeated || rectTransform == null)
            {
                return;
            }

            if (playerController != null)
            {
                Vector2 playerPosition = playerController.GetPosition();
                if (Vector2.Distance(rectTransform.anchoredPosition, playerPosition) <= encounterRadius)
                {
                    TryStartEncounter();
                }
            }

            Vector2 drift = new Vector2(Mathf.Sin(Time.time * 1.3f + patrolSeed), Mathf.Cos(Time.time * 1.1f + patrolSeed)) * patrolRadius;
            rectTransform.anchoredPosition = homePosition + drift;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TryStartEncounter();
        }

        private void TryStartEncounter()
        {
            if (defeated || adventureMapService == null)
            {
                return;
            }

            if (playerController != null)
            {
                float distance = Vector2.Distance(playerController.GetPosition(), rectTransform.anchoredPosition);
                if (distance > encounterRadius)
                {
                    return;
                }
            }

            if (adventureMapService.StartEncounter(enemyDefinition, isBoss))
            {
                defeated = true;
            }
        }
    }
}
