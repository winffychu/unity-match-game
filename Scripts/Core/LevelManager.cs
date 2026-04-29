using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryMatch.Data;
using MemoryMatch.Utils;

namespace MemoryMatch.Core
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Configs")]
        [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>()
        {
            new LevelConfig { levelIndex = 0, rows = 2, columns = 2, maxFlipCount = 6 },
            new LevelConfig { levelIndex = 1, rows = 2, columns = 4, maxFlipCount = 12 },
            new LevelConfig { levelIndex = 2, rows = 4, columns = 4, maxFlipCount = 24 }
        };

        [Header("Card Spawn")]
        [SerializeField] private Gameplay.Card cardPrefab;
        [SerializeField] private Transform cardParent;
        [SerializeField] private GridLayoutGroup gridLayout;

        [Header("Card Sprites")]
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private List<Sprite> cardFrontSprites = new List<Sprite>();

        [Header("Animation")]
        [SerializeField] private float spawnDelay = 0.05f;

        private List<Gameplay.Card> currentCards = new List<Gameplay.Card>();
        private Coroutine spawnRoutine;

        public int LevelCount => levelConfigs.Count;

        public LevelConfig GetLevelConfig(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < levelConfigs.Count)
                return levelConfigs[levelIndex];
            return levelConfigs[0];
        }

        public Coroutine GenerateLevel(int levelIndex)
        {
            LevelConfig config = GetLevelConfig(levelIndex);

            if (config.TotalCards % 2 != 0)
            {
                Debug.LogError("卡牌总数必须为偶数！");
                return null;
            }

            if (cardFrontSprites.Count < config.PairCount)
            {
                Debug.LogError($"卡牌正面图片数量不足！需要 {config.PairCount} 种，只有 {cardFrontSprites.Count} 种");
                return null;
            }

            // Setup grid
            if (gridLayout != null)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = config.columns;
                gridLayout.cellSize = CalculateCardSize(config.rows, config.columns);
                gridLayout.spacing = new Vector2(10, 10);
            }

            // Generate card IDs
            List<int> cardIds = new List<int>();
            for (int i = 0; i < config.PairCount; i++)
            {
                cardIds.Add(i);
                cardIds.Add(i);
            }
            ShuffleUtility.Shuffle(cardIds);

            // Clear existing cards
            currentCards.Clear();

            // Spawn cards
            for (int i = 0; i < cardIds.Count; i++)
            {
                int cardId = cardIds[i];
                Sprite frontSprite = cardFrontSprites[cardId];
                
                Gameplay.Card card = Instantiate(cardPrefab, cardParent);
                card.Initialize(cardId, frontSprite, cardBackSprite);
                card.name = $"Card_{i}_ID{cardId}";
                
                // Initial scale for spawn animation
                card.transform.localScale = Vector3.zero;
                
                currentCards.Add(card);
            }

            // Spawn animation
            spawnRoutine = StartCoroutine(SpawnCardsAnimation(currentCards));

            return spawnRoutine;
        }

        private IEnumerator SpawnCardsAnimation(List<Gameplay.Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    StartCoroutine(ScaleCard(cards[i].transform, Vector3.one, 0.3f));
                }
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private IEnumerator ScaleCard(Transform cardTransform, Vector3 targetScale, float duration)
        {
            Vector3 startScale = cardTransform.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Ease out back
                float easeT = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
                cardTransform.localScale = Vector3.Lerp(startScale, targetScale, easeT);
                yield return null;
            }
            
            cardTransform.localScale = targetScale;
        }

        private Vector2 CalculateCardSize(int rows, int columns)
        {
            // 使用 RectTransform 的实际尺寸
            RectTransform parentRect = cardParent as RectTransform;
            if (parentRect == null)
            {
                // Fallback for non-RectTransform parents
                float spacing = 10f;
                float maxWidth = 800f;
                float maxHeight = 600f;
                
                float availableWidth = maxWidth - (columns - 1) * spacing;
                float availableHeight = maxHeight - (rows - 1) * spacing;
                
                float cardWidth = availableWidth / columns;
                float cardHeight = availableHeight / rows;
                float size = Mathf.Min(cardWidth, cardHeight);
                
                return new Vector2(size, size * 1.3f);
            }

            float horizontalPadding = gridLayout.padding.left + gridLayout.padding.right;
            float verticalPadding = gridLayout.padding.top + gridLayout.padding.bottom;
            
            float spacingX = gridLayout.spacing.x;
            float spacingY = gridLayout.spacing.y;

            float availableWidth = parentRect.rect.width - horizontalPadding - (columns - 1) * spacingX;
            float availableHeight = parentRect.rect.height - verticalPadding - (rows - 1) * spacingY;

            float cardWidth = availableWidth / columns;
            float cardHeight = availableHeight / rows;
            
            float aspectRatio = 1.3f;
            float sizeByWidth = cardWidth;
            float sizeByHeight = cardHeight / aspectRatio;
            
            float finalWidth = Mathf.Min(sizeByWidth, sizeByHeight);
            float finalHeight = finalWidth * aspectRatio;

            return new Vector2(finalWidth, finalHeight);
        }

        public void ClearCards()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
            
            for (int i = cardParent.childCount - 1; i >= 0; i--)
            {
                Destroy(cardParent.GetChild(i).gameObject);
            }
            currentCards.Clear();
        }
    }
}