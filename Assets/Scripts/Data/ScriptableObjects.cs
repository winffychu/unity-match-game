using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch.Data
{
    [CreateAssetMenu(fileName = "CardData_New", menuName = "MemoryMatch/Card Data")]
    public class CardDataSO : ScriptableObject
    {
        public int cardId;
        public Sprite frontSprite;
        public string cardName;
    }

    [CreateAssetMenu(fileName = "LevelConfig_New", menuName = "MemoryMatch/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        public int levelId;
        public string levelName;
        [TextArea(2, 4)] public string description;
        
        [Range(2, 8)] public int rows = 4;
        [Range(2, 8)] public int columns = 4;
        public float cardSpacing = 1.2f;
        public float cardScale = 1f;
        
        public int maxFlipCount = 0;
        public int timeLimit = 0;
        public float previewTime = 1.5f;
        public float matchDisappearDelay = 0.5f;
        
        public int baseMatchScore = 100;
        public float comboMultiplier = 1.5f;
        public int timeBonusPerSecond = 10;
        
        public List<int> cardIds = new List<int>();
        public Sprite cardBackSprite;
        
        public int oneStarScore = 500;
        public int twoStarScore = 1000;
        public int threeStarScore = 2000;
        
        public int TotalCardCount => rows * columns;
        public int PairCount => TotalCardCount / 2;
        
        public bool IsValid()
        {
            if (TotalCardCount % 2 != 0)
            {
                Debug.LogError($"关卡 {levelName}: 卡牌总数必须为偶数");
                return false;
            }
            if (cardIds.Count < PairCount)
            {
                Debug.LogError($"关卡 {levelName}: 需要至少 {PairCount} 种卡牌图案");
                return false;
            }
            return true;
        }
        
        public int GetStarRating(int score)
        {
            if (score >= threeStarScore) return 3;
            if (score >= twoStarScore) return 2;
            if (score >= oneStarScore) return 1;
            return 0;
        }
    }

    [CreateAssetMenu(fileName = "GameConfig", menuName = "MemoryMatch/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        public float flipAnimationDuration = 0.3f;
        public float disappearAnimationDuration = 0.5f;
        public float shakeIntensity = 5f;
        public float shakeDuration = 0.3f;
        public float gameStartDelay = 1f;
        public float levelEndDelay = 2f;
        public Color defaultCardBackColor = new Color(0.2f, 0.4f, 0.8f);
        public Color matchSuccessColor = Color.green;
        public Color matchFailColor = Color.red;
    }

    [CreateAssetMenu(fileName = "AudioConfig", menuName = "MemoryMatch/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        public AudioClip flipSound;
        public AudioClip matchSuccessSound;
        public AudioClip matchFailSound;
        public AudioClip levelCompleteSound;
        public AudioClip levelFailSound;
        public AudioClip buttonClickSound;
        public AudioClip menuBGM;
        public AudioClip gameBGM;
        
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
    }
}
