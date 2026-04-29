using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MemoryMatch.Gameplay
{
    public class Card : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image frontImage;
        [SerializeField] private Image backImage;
        [SerializeField] private GameObject matchedEffect;
        [SerializeField] private Animator animator;

        [Header("Settings")]
        [SerializeField] private float flipDuration = 0.3f;

        private int cardId;
        private bool isFaceUp;
        private bool isMatched;
        private bool isFlipping;
        private Sprite frontSprite;
        private Sprite backSprite;
        private RectTransform rectTransform;

        public int CardId => cardId;
        public bool IsFaceUp => isFaceUp;
        public bool IsMatched => isMatched;
        public bool IsFlipping => isFlipping;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(int id, Sprite front, Sprite back)
        {
            cardId = id;
            frontSprite = front;
            backSprite = back;
            isFaceUp = false;
            isMatched = false;
            isFlipping = false;

            if (frontImage != null) frontImage.sprite = frontSprite;
            if (backImage != null) backImage.sprite = backSprite;

            UpdateVisualState();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isMatched || isFaceUp || isFlipping) return;
            Core.GameManager.Instance.OnCardClicked(this);
        }

        public void FlipToFront()
        {
            if (isFaceUp || isFlipping) return;
            StartCoroutine(FlipCoroutine(true));
        }

        public void FlipToBack()
        {
            if (!isFaceUp || isFlipping) return;
            StartCoroutine(FlipCoroutine(false));
        }

        public void SetMatched(bool matched)
        {
            isMatched = matched;
            if (matched)
            {
                isFaceUp = true;
                if (matchedEffect != null) matchedEffect.SetActive(true);
                if (animator != null) animator.SetTrigger("Matched");
            }
            UpdateVisualState();
        }

        private System.Collections.IEnumerator FlipCoroutine(bool toFront)
        {
            isFlipping = true;

            if (animator != null)
            {
                animator.SetTrigger(toFront ? "FlipFront" : "FlipBack");
                yield return new WaitForSeconds(flipDuration);
                
                // Bug fix: Must update state after animator completes
                isFaceUp = toFront;
                UpdateVisualState();
            }
            else
            {
                // Simple scale animation fallback
                Vector3 originalScale = transform.localScale;
                float halfDuration = flipDuration / 2f;
                float elapsed = 0f;

                // Scale down
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    transform.localScale = new Vector3(originalScale.x * (1 - t), originalScale.y, originalScale.z);
                    yield return null;
                }

                isFaceUp = toFront;
                UpdateVisualState();

                // Scale up
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    transform.localScale = new Vector3(originalScale.x * t, originalScale.y, originalScale.z);
                    yield return null;
                }

                transform.localScale = originalScale;
            }

            isFlipping = false;
        }

        public void PlayErrorAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Error");
            }
            else
            {
                StartCoroutine(ShakeCoroutine());
            }
        }

        private System.Collections.IEnumerator ShakeCoroutine()
        {
            Vector3 originalPosition = transform.localPosition;
            float duration = 0.3f;
            float intensity = 10f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed * 30f) * intensity * (1 - elapsed / duration);
                transform.localPosition = originalPosition + new Vector3(x, 0, 0);
                yield return null;
            }

            transform.localPosition = originalPosition;
        }

        private void UpdateVisualState()
        {
            if (frontImage != null) frontImage.gameObject.SetActive(isFaceUp);
            if (backImage != null) backImage.gameObject.SetActive(!isFaceUp);
        }

        public void ResetCard()
        {
            isFaceUp = false;
            isMatched = false;
            isFlipping = false;
            if (matchedEffect != null) matchedEffect.SetActive(false);
            
            // 重置 RectTransform 状态
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
            }
            transform.localScale = Vector3.one;
            
            // 重置 Animator 状态
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
            
            UpdateVisualState();
        }
    }
}