using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MemoryMatch.Data;
using MemoryMatch.Gameplay;

namespace MemoryMatch.Core
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Configs")]
        [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>()
        {
            new LevelConfig { levelIndex = 0, rows = 2, columns = 2, maxFlipCount = 3 },
            new LevelConfig { levelIndex = 1, rows = 2, columns = 4, maxFlipCount = 6 },
            new LevelConfig { levelIndex = 2, rows = 4, columns = 4, maxFlipCount = 14 }
        };

        [Header("Card Setup")]
        [SerializeField] private Transform cardParent;
        [SerializeField] private Card cardPrefab;
        [SerializeField] private List<Sprite> cardFrontSprites = new List<Sprite>();

        private readonly List<Card> currentCards = new List<Card>();

        public int LevelCount => levelConfigs.Count;

        public LevelConfig GetLevelConfig(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levelConfigs.Count)
                return null;
            return levelConfigs[levelIndex];
        }

        public void GenerateLevel(int levelIndex)
        {
            LevelConfig config = GetLevelConfig(levelIndex);
            if (config == null)
            {
                Debug.LogError($"未找到关卡配置: {levelIndex}");
                return;
            }

            if (cardParent == null || cardPrefab == null)
            {
                Debug.LogError("LevelManager 缺少 cardParent 或 cardPrefab");
                return;
            }

            ClearCards();
            ApplyGridLayout(config);

            int cardCount = config.TotalCards;
            int pairCount = config.PairCount;
            List<int> ids = BuildShuffledIds(pairCount);

            for (int i = 0; i < cardCount; i++)
            {
                Card card = Instantiate(cardPrefab, cardParent);
                int spriteIndex = ids[i] % Mathf.Max(1, cardFrontSprites.Count);
                Sprite frontSprite = cardFrontSprites.Count > 0 ? cardFrontSprites[spriteIndex] : null;
                card.Setup(ids[i], frontSprite);
                currentCards.Add(card);
            }
        }

        public void ClearCards()
        {
            if (cardParent == null)
            {
                currentCards.Clear();
                return;
            }

            for (int i = cardParent.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(cardParent.GetChild(i).gameObject);
                else
                    Destroy(cardParent.GetChild(i).gameObject);
#else
                Destroy(cardParent.GetChild(i).gameObject);
#endif
            }

            currentCards.Clear();
        }

        private void ApplyGridLayout(LevelConfig config)
        {
            GridLayoutGroup grid = cardParent.GetComponent<GridLayoutGroup>();
            RectTransform rect = cardParent as RectTransform;
            if (grid == null || rect == null)
                return;

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = config.columns;

            float width = rect.rect.width > 0f ? rect.rect.width : 1200f;
            float height = rect.rect.height > 0f ? rect.rect.height : 720f;
            float spacingX = grid.spacing.x;
            float spacingY = grid.spacing.y;
            float paddingX = grid.padding.left + grid.padding.right;
            float paddingY = grid.padding.top + grid.padding.bottom;

            float cellWidth = (width - paddingX - spacingX * (config.columns - 1)) / config.columns;
            float cellHeight = (height - paddingY - spacingY * (config.rows - 1)) / config.rows;
            float size = Mathf.Max(80f, Mathf.Min(cellWidth, cellHeight));
            grid.cellSize = new Vector2(size, size);
        }

        private List<int> BuildShuffledIds(int pairCount)
        {
            var ids = new List<int>(pairCount * 2);
            for (int i = 0; i < pairCount; i++)
            {
                ids.Add(i);
                ids.Add(i);
            }

            for (int i = 0; i < ids.Count; i++)
            {
                int swapIndex = Random.Range(i, ids.Count);
                (ids[i], ids[swapIndex]) = (ids[swapIndex], ids[i]);
            }

            return ids;
        }
    }
}
