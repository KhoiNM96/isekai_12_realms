using System.Collections;
using Isekai12Realms.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Battle
{
    public class BattleCharacterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI shieldText;
        [SerializeField] private Image spriteImage;
        [SerializeField] private Image hpFill;
        [SerializeField] private Image manaFill;

        private Coroutine moveRoutine;
        private Coroutine hpRoutine;
        private Coroutine manaRoutine;

        public void Bind(TextMeshProUGUI name, Image hp, Image mana, Image sprite)
        {
            nameText = name;
            hpFill = hp;
            manaFill = mana;
            spriteImage = sprite;
        }

        public void SetName(string value) { if (nameText != null) nameText.text = value; }
        public void SetLevel(int level) { if (levelText != null) levelText.text = "Lv. " + level; }
        public void SetHp(int current, int max) => SetFill(hpFill, current, max, ref hpRoutine);
        public void SetMana(int current, int max) => SetFill(manaFill, current, max, ref manaRoutine);
        public void SetShield(int shield) { if (shieldText != null) shieldText.text = shield > 0 ? shield + " Shield" : string.Empty; }
        public void SetSprite(string assetId)
        {
            if (spriteImage == null) return;
            Sprite sprite = AssetSpriteBinder.GetSprite(assetId);
            if (sprite == null) return;
            spriteImage.sprite = sprite;
            spriteImage.preserveAspect = true;
            spriteImage.color = Color.white;
        }

        public void PlayIdle() { }
        public void PlayAttack() => Punch(1.12f, 0.12f);
        public void PlayCast() => Punch(1.18f, 0.16f);
        public void PlayHurt() { PlaySmallShake(); FlashWhite(); }
        public void PlayVictory() => Punch(1.2f, 0.22f);
        public void PlayDefeat() => Punch(0.85f, 0.20f);
        public void PlaySmallShake() { if (moveRoutine != null) StopCoroutine(moveRoutine); moveRoutine = StartCoroutine(Shake(0.15f, 12f)); }
        public void FlashWhite() { if (spriteImage != null) StartCoroutine(Flash()); }

        private void SetFill(Image fill, int current, int max, ref Coroutine routine)
        {
            if (fill == null || max <= 0) return;
            if (routine != null)
            {
                StopCoroutine(routine);
            }
            routine = StartCoroutine(AnimateFill(fill, Mathf.Clamp01((float)current / max)));
        }

        private IEnumerator AnimateFill(Image fill, float target)
        {
            float start = fill.rectTransform.anchorMax.x;
            for (float t = 0f; t < 0.18f; t += Time.deltaTime)
            {
                fill.rectTransform.anchorMax = new Vector2(Mathf.Lerp(start, target, t / 0.18f), 1f);
                yield return null;
            }
            fill.rectTransform.anchorMax = new Vector2(target, 1f);
        }

        private void Punch(float scale, float duration) { if (moveRoutine != null) StopCoroutine(moveRoutine); moveRoutine = StartCoroutine(PunchRoutine(scale, duration)); }
        private IEnumerator PunchRoutine(float scale, float duration)
        {
            Vector3 baseScale = Vector3.one;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float n = Mathf.Sin((t / duration) * Mathf.PI);
                transform.localScale = Vector3.Lerp(baseScale, baseScale * scale, n);
                yield return null;
            }
            transform.localScale = baseScale;
        }

        private IEnumerator Shake(float duration, float strength)
        {
            Vector3 basePos = transform.localPosition;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.localPosition = basePos + new Vector3(Mathf.Sin(t * 80f) * strength, 0f, 0f);
                yield return null;
            }
            transform.localPosition = basePos;
        }

        private IEnumerator Flash()
        {
            Color original = spriteImage.color;
            spriteImage.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            spriteImage.color = original;
        }
    }
}
