using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MemoryMatch.Gameplay
{
    public class Card : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image frontImage;
        [SerializeField] private Image backImage;
        [SerializeField] private GameObject matchedEffect;

        [Header("Flip Animation")]
        [SerializeField] private float flipDuration = 0.2f;

        private int cardId;
        private bool isFaceUp;
        private bool isMatched;
        private bool interactionEnabled = true;
        private bool isFlipping;

        public int CardId => cardId;
        public bool IsFaceUp => isFaceUp;
        public bool IsMatched => isMatched;
        public bool IsFlipping => isFlipping;

        public void Setup(int id, Sprite frontSprite)
        {
            cardId = id;
            if (frontImage != null)
                frontImage.sprite = frontSprite;
            isMatched = false;
            interactionEnabled = true;
            ForceFlipImmediate(false);
            SetMatched(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactionEnabled || isMatched || isFaceUp || isFlipping)
                return;
            MemoryMatch.Core.GameManager.Instance?.OnCardClicked(this);
        }

        public void FlipToFront()
        {
            if (!gameObject.activeInHierarchy)
            {
                ForceFlipImmediate(true);
                return;
            }
            StopAllCoroutines();
            StartCoroutine(FlipRoutine(true));
        }

        public void FlipToBack()
        {
            if (!gameObject.activeInHierarchy)
            {
                ForceFlipImmediate(false);
                return;
            }
            StopAllCoroutines();
            StartCoroutine(FlipRoutine(false));
        }

        public void ForceFlipImmediate(bool faceUp)
        {
            StopAllCoroutines();
            isFlipping = false;
            isFaceUp = faceUp;
            UpdateVisualState();
            transform.localScale = Vector3.one;
        }

        public void SetMatched(bool matched)
        {
            isMatched = matched;
            interactionEnabled = !matched;
            if (matchedEffect != null)
                matchedEffect.SetActive(matched);

            var group = GetComponent<CanvasGroup>();
            if (group != null)
                group.alpha = matched ? 0.45f : 1f;
        }

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled && !isMatched;
        }

        public void PlayErrorAnimation()
        {
            transform.localScale = Vector3.one * 1.08f;
        }

        private IEnumerator FlipRoutine(bool faceUp)
        {
            isFlipping = true;
            float half = Mathf.Max(0.01f, flipDuration * 0.5f);
            float elapsed = 0f;
            Vector3 originalScale = Vector3.one;

            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float progress = 1f - Mathf.Clamp01(elapsed / half);
                transform.localScale = new Vector3(progress, originalScale.y, originalScale.z);
                yield return null;
            }

            isFaceUp = faceUp;
            UpdateVisualState();

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / half);
                transform.localScale = new Vector3(progress, originalScale.y, originalScale.z);
                yield return null;
            }

            transform.localScale = originalScale;
            isFlipping = false;
        }

        private void UpdateVisualState()
        {
            if (frontImage != null)
                frontImage.gameObject.SetActive(isFaceUp);
            if (backImage != null)
                backImage.gameObject.SetActive(!isFaceUp);
        }
    }
}
