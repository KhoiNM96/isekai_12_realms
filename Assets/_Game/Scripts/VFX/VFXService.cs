using System.Collections;
using Isekai12Realms.Core;
using Isekai12Realms.Data;
using Isekai12Realms.Performance;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.VFX
{
    public class VFXService : MonoBehaviour
    {
        private RectTransform root;

        public void Initialize(RectTransform layer)
        {
            root = layer;
        }

        public void PlayTileMatchVfx(Vector3 position, TileType type)
        {
            if (ReduceEffects()) return;
            PlayCircle(position, ColorFor(type), 72f, 0.24f, VfxFor(type));
        }
        public void PlayDamageVfx(Vector3 position) => PlayCircle(position, new Color(1f, 0.22f, 0.18f, 0.75f), 96f, 0.22f, "vfx_attack_slash");
        public void PlayHealVfx(Vector3 position) => PlayCircle(position, new Color(0.35f, 1f, 0.45f, 0.75f), 90f, 0.25f, "vfx_heal_heart");
        public void PlayShieldVfx(Vector3 position) => PlayCircle(position, new Color(0.55f, 0.75f, 1f, 0.75f), 90f, 0.25f, "vfx_shield_bubble");
        public void PlayManaVfx(Vector3 position) => PlayCircle(position, new Color(0.45f, 0.55f, 1f, 0.75f), 90f, 0.25f, "vfx_mana_glow");
        public void PlaySkillVfx(string skillId, Vector3 position)
        {
            string key = (skillId ?? string.Empty).ToLowerInvariant();
            if (key.Contains("shuffle") || key.Contains("static"))
            {
                PlayCircle(position, new Color(0.45f, 0.72f, 1f, 0.75f), 118f, 0.24f, "vfx_match_sparkle");
                return;
            }

            if (key.Contains("burst") || key.Contains("chain") || key.Contains("ultimate"))
            {
                PlayCircle(position, new Color(1f, 0.82f, 0.25f, 0.85f), 160f, 0.3f, SkillVfxFor(skillId));
                return;
            }

            PlayCircle(position, new Color(1f, 0.45f, 0.2f, 0.75f), 130f, 0.28f, "vfx_match_pop");
        }
        public void PlayFullScreenFlash(Color color, float duration)
        {
            if (root == null) return;
            if (ReduceEffects()) color.a *= 0.45f;
            StartCoroutine(Flash(color, duration));
        }

        private void PlayCircle(Vector3 position, Color color, float size, float duration, string assetId = null)
        {
            if (root == null) return;
            if (ReduceEffects())
            {
                color.a *= 0.65f;
                size *= 0.8f;
            }
            GameObject go = new GameObject("VFX_Circle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            rect.position = position;
            Image image = go.GetComponent<Image>();
            Sprite sprite = !string.IsNullOrEmpty(assetId) ? Isekai12Realms.UI.AssetSpriteBinder.GetSprite(assetId) : null;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
                color = Color.white;
            }
            image.color = color;
            image.raycastTarget = false;
            StartCoroutine(Pop(rect, image, duration));
        }

        private static bool ReduceEffects()
        {
            return ServiceLocator.TryResolve<PerformanceService>(out PerformanceService performance) && performance.ReduceEffects;
        }

        private IEnumerator Pop(RectTransform rect, Image image, float duration)
        {
            Vector3 start = Vector3.one * 0.5f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float n = t / duration;
                rect.localScale = Vector3.Lerp(start, Vector3.one * 1.5f, n);
                Color c = image.color;
                c.a = 1f - n;
                image.color = c;
                yield return null;
            }
            Destroy(rect.gameObject);
        }

        private IEnumerator Flash(Color color, float duration)
        {
            GameObject go = new GameObject("VFX_Flash", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                Color c = color;
                c.a = Mathf.Lerp(color.a, 0f, t / duration);
                image.color = c;
                yield return null;
            }
            Destroy(go);
        }

        private static Color ColorFor(TileType type)
        {
            switch (type)
            {
                case TileType.Heart: return new Color(1f, 0.28f, 0.38f, 0.8f);
                case TileType.Coin: return new Color(1f, 0.78f, 0.22f, 0.8f);
                case TileType.Food: return new Color(0.45f, 0.85f, 0.32f, 0.8f);
                case TileType.Book: return new Color(0.72f, 0.5f, 1f, 0.8f);
                case TileType.Mana: return new Color(0.32f, 0.52f, 1f, 0.8f);
                case TileType.Shield: return new Color(0.58f, 0.64f, 0.72f, 0.8f);
                case TileType.Star: return new Color(1f, 0.95f, 0.25f, 0.8f);
                default: return new Color(1f, 1f, 1f, 0.75f);
            }
        }

        private static string VfxFor(TileType type)
        {
            switch (type)
            {
                case TileType.Heart: return "vfx_heal_heart";
                case TileType.Coin: return "vfx_coin_pop";
                case TileType.Food: return "vfx_food_pop";
                case TileType.Book: return "vfx_exp_book";
                case TileType.Mana: return "vfx_mana_glow";
                case TileType.Shield: return "vfx_shield_bubble";
                default: return "vfx_match_pop";
            }
        }

        private static string SkillVfxFor(string skillId)
        {
            string key = (skillId ?? string.Empty).ToLowerInvariant();
            if (key.Contains("tide")) return "vfx_skill_tide_wave";
            if (key.Contains("storm") || key.Contains("chain")) return "vfx_skill_storm_chain";
            return "vfx_skill_flame_burst";
        }
    }
}
