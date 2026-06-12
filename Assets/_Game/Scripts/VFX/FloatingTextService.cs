using System.Collections;
using TMPro;
using UnityEngine;

namespace Isekai12Realms.VFX
{
    public class FloatingTextService : MonoBehaviour
    {
        private RectTransform root;
        private float duration = 0.75f;

        public void Initialize(RectTransform layer, float textDuration)
        {
            root = layer;
            duration = textDuration;
        }

        public void Show(string text, Vector3 worldPosition, Color color, int size = 34)
        {
            if (root == null) return;
            GameObject go = new GameObject("FloatingText", typeof(RectTransform));
            go.transform.SetParent(root, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 80f);
            rect.position = worldPosition;
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.raycastTarget = false;
            label.outlineWidth = 0.18f;
            label.outlineColor = Color.black;
            StartCoroutine(Animate(rect, label));
        }

        private IEnumerator Animate(RectTransform rect, TextMeshProUGUI label)
        {
            Vector3 start = rect.localPosition;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float n = t / duration;
                rect.localPosition = start + Vector3.up * Mathf.Lerp(0f, 90f, n);
                Color c = label.color;
                c.a = 1f - n;
                label.color = c;
                yield return null;
            }
            Destroy(rect.gameObject);
        }
    }
}
