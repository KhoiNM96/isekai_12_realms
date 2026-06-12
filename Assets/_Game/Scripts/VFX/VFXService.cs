using System.Collections;
using Isekai12Realms.Data;
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

        public void PlayTileMatchVfx(Vector3 position, TileType type) => PlayCircle(position, ColorFor(type), 72f, 0.24f);
        public void PlayDamageVfx(Vector3 position) => PlayCircle(position, new Color(1f, 0.22f, 0.18f, 0.75f), 96f, 0.22f);
        public void PlayHealVfx(Vector3 position) => PlayCircle(position, new Color(0.35f, 1f, 0.45f, 0.75f), 90f, 0.25f);
        public void PlayShieldVfx(Vector3 position) => PlayCircle(position, new Color(0.55f, 0.75f, 1f, 0.75f), 90f, 0.25f);
        public void PlayManaVfx(Vector3 position) => PlayCircle(position, new Color(0.45f, 0.55f, 1f, 0.75f), 90f, 0.25f);
        public void PlaySkillVfx(string skillId, Vector3 position)
        {
            string key = (skillId ?? string.Empty).ToLowerInvariant();
            if (key.Contains("shuffle") || key.Contains("static"))
            {
                PlayCircle(position, new Color(0.45f, 0.72f, 1f, 0.75f), 118f, 0.24f);
                return;
            }

            if (key.Contains("burst") || key.Contains("chain") || key.Contains("ultimate"))
            {
                PlayCircle(position, new Color(1f, 0.82f, 0.25f, 0.85f), 160f, 0.3f);
                return;
            }

            PlayCircle(position, new Color(1f, 0.45f, 0.2f, 0.75f), 130f, 0.28f);
        }
        public void PlayFullScreenFlash(Color color, float duration) { if (root != null) StartCoroutine(Flash(color, duration)); }

        private void PlayCircle(Vector3 position, Color color, float size, float duration)
        {
            if (root == null) return;
            GameObject go = new GameObject("VFX_Circle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            rect.position = position;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            StartCoroutine(Pop(rect, image, duration));
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
    }
}
