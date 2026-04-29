using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryMatch.Data;
using MemoryMatch.Gameplay;
using MemoryMatch.Utils;

namespace MemoryMatch.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Managers")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Settings")]
        [SerializeField] private float mismatchFlipBackDelay = 0.8f;
        [SerializeField] private float matchDelay = 0.3f;
        [SerializeField] private float inputCooldown = 0.1f;

        private GameState currentState = GameState.Idle;
        private List<Card> selectedCards = new List<Card>(2);
        private Stack<FlipRecord> flipHistory = new Stack<FlipRecord>();

        private int currentLevelIndex = 0;
        private int currentFlipCount = 0;
        private int matchedPairCount = 0;
        private int totalPairCount = 0;
        private int maxFlipCount = 0;
        private int currentScore = 0;
        private int comboCount = 0;
        private int maxComboCount = 0;

        // 精确控制协程引用
        private Coroutine checkMatchRoutine;
        private Coroutine spawnCardsRoutine;

        private float lastInputTime;

        public GameState CurrentState => currentState;
        public int CurrentLevelIndex => currentLevelIndex;
        public int CurrentFlipCount => currentFlipCount;
        public int MaxFlipCount => maxFlipCount;
        public int CurrentScore => currentScore;
        public int ComboCount => comboCount;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            uiManager.ShowMainMenu(true);
            uiManager.ShowGameUI(false);
            uiManager.ShowResultPanel(false);
        }

        public void StartGame()
        {
            currentLevelIndex = 0;
            StartLevel(currentLevelIndex);
        }

        public void RestartCurrentLevel()
        {
            StartLevel(currentLevelIndex);
        }

        public void StartNextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= levelManager.LevelCount)
            {
                uiManager.ShowAllLevelsComplete();
                return;
            }
            StartLevel(currentLevelIndex);
        }

        public void ReturnToMainMenu()
        {
            if (checkMatchRoutine != null) { StopCoroutine(checkMatchRoutine); checkMatchRoutine = null; }
            if (spawnCardsRoutine != null) { StopCoroutine(spawnCardsRoutine); spawnCardsRoutine = null; }

            Time.timeScale = 1f;
            currentState = GameState.Idle;
            selectedCards.Clear();
            flipHistory.Clear();
            levelManager.ClearCards();
            uiManager.ShowPausePanel(false);
            uiManager.ShowAllLevelsCompletePanel(false);
            uiManager.ShowMainMenu(true);
            uiManager.ShowGameUI(false);
            uiManager.ShowResultPanel(false);
        }

        private void StartLevel(int levelIndex)
        {
            // 精确停止正在运行的协程
            if (checkMatchRoutine != null) { StopCoroutine(checkMatchRoutine); checkMatchRoutine = null; }
            if (spawnCardsRoutine != null) { StopCoroutine(spawnCardsRoutine); spawnCardsRoutine = null; }

            currentState = GameState.Playing;
            currentFlipCount = 0;
            matchedPairCount = 0;
            currentScore = 0;
            comboCount = 0;
            maxComboCount = 0;

            selectedCards.Clear();
            flipHistory.Clear();

            LevelConfig config = levelManager.GetLevelConfig(levelIndex);
            maxFlipCount = config.maxFlipCount;
            totalPairCount = config.PairCount;

            levelManager.ClearCards();
            spawnCardsRoutine = levelManager.GenerateLevel(levelIndex);

            uiManager.ShowMainMenu(false);
            uiManager.ShowGameUI(true);
            uiManager.ShowResultPanel(false);
            uiManager.UpdateLevelText(levelIndex + 1);
            uiManager.UpdateFlipText(currentFlipCount, maxFlipCount);
            uiManager.UpdateScoreText(currentScore);
            uiManager.UpdateComboText(comboCount);
            uiManager.SetUndoButtonEnabled(false);

            audioManager?.PlayBGM();
        }

        public void OnCardClicked(Card clickedCard)
        {
            // 输入保护：防止快速双击
            if (Time.time - lastInputTime < inputCooldown) return;
            if (currentState != GameState.Playing) return;
            if (clickedCard == null || clickedCard.IsFaceUp || clickedCard.IsMatched) return;
            if (selectedCards.Count >= 2) return;

            lastInputTime = Time.time;

            clickedCard.FlipToFront();
            selectedCards.Add(clickedCard);
            flipHistory.Push(new FlipRecord(clickedCard, false));

            audioManager?.PlayFlipSound();

            uiManager.SetUndoButtonEnabled(flipHistory.Count > 0);

            if (selectedCards.Count == 2)
            {
                currentFlipCount++;
                uiManager.UpdateFlipText(currentFlipCount, maxFlipCount);
                checkMatchRoutine = StartCoroutine(CheckMatchCoroutine());
            }
        }

        private IEnumerator CheckMatchCoroutine()
        {
            currentState = GameState.Checking;

            yield return new WaitForSeconds(matchDelay);

            Card first = selectedCards[0];
            Card second = selectedCards[1];

            if (first.CardId == second.CardId)
            {
                // Match!
                first.SetMatched(true);
                second.SetMatched(true);

                matchedPairCount++;
                comboCount++;
                maxComboCount = Mathf.Max(maxComboCount, comboCount);
                int points = 100 * comboCount;
                currentScore += points;

                audioManager?.PlayMatchSuccessSound();

                uiManager.UpdateScoreText(currentScore);
                uiManager.UpdateComboText(comboCount);
                uiManager.ShowComboEffect(comboCount);

                selectedCards.Clear();

                if (matchedPairCount >= totalPairCount)
                {
                    yield return new WaitForSeconds(0.5f);
                    currentState = GameState.Win;
                    audioManager?.PlayLevelCompleteSound();
                    bool hasNextLevel = currentLevelIndex < levelManager.LevelCount - 1;
                    uiManager.ShowWinPanel(hasNextLevel, currentScore, maxComboCount);
                }
                else
                {
                    currentState = GameState.Playing;
                }
            }
            else
            {
                // Mismatch
                comboCount = 0;
                uiManager.UpdateComboText(comboCount);

                audioManager?.PlayMatchFailSound();
                first.PlayErrorAnimation();
                second.PlayErrorAnimation();

                yield return new WaitForSeconds(mismatchFlipBackDelay);

                if (first != null && !first.IsMatched) first.FlipToBack();
                if (second != null && !second.IsMatched) second.FlipToBack();

                selectedCards.Clear();

                if (currentFlipCount >= maxFlipCount)
                {
                    currentState = GameState.Lose;
                    audioManager?.PlayLevelFailSound();
                    uiManager.ShowLosePanel(currentScore);
                }
                else
                {
                    currentState = GameState.Playing;
                }
            }

            checkMatchRoutine = null;
            uiManager.SetUndoButtonEnabled(flipHistory.Count > 0);
        }

        public void UndoLastFlip()
        {
            if (currentState != GameState.Playing) return;
            if (flipHistory.Count == 0) return;

            // 如果正在等待配对结果，取消协程并翻回两张牌
            if (selectedCards.Count == 2)
            {
                if (checkMatchRoutine != null) { StopCoroutine(checkMatchRoutine); checkMatchRoutine = null; }
                
                Card first = selectedCards[0];
                Card second = selectedCards[1];
                if (!first.IsMatched) first.FlipToBack();
                if (!second.IsMatched) second.FlipToBack();
                selectedCards.Clear();
                currentFlipCount--;
                uiManager.UpdateFlipText(currentFlipCount, maxFlipCount);
            }

            // 撤销最后一张翻开的牌
            FlipRecord record = flipHistory.Pop();
            if (record.card != null && !record.card.IsMatched && record.card.IsFaceUp)
            {
                record.card.FlipToBack();
                selectedCards.Remove(record.card);
            }

            uiManager.SetUndoButtonEnabled(flipHistory.Count > 0);
            currentState = GameState.Playing;
        }

        public void OnPauseButtonClicked()
        {
            Time.timeScale = 0f;
            uiManager.ShowPausePanel(true);
        }

        public void OnResumeButtonClicked()
        {
            Time.timeScale = 1f;
            uiManager.ShowPausePanel(false);
        }
    }
}