using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryMatch.Data;
using MemoryMatch.Gameplay;

namespace MemoryMatch.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Managers")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Gameplay")]
        [SerializeField] private float mismatchPreviewDuration = 0.8f;

        private readonly List<Card> selectedCards = new List<Card>(2);
        private readonly List<FlipRecord> currentAttemptRecords = new List<FlipRecord>(2);

        private int currentLevelIndex;
        private int attemptsUsed;
        private int matchedPairs;
        private bool pendingAttemptCommitted;
        private bool isBusy;
        private GameState currentState = GameState.Idle;

        private LevelConfig CurrentConfig => levelManager != null ? levelManager.GetLevelConfig(currentLevelIndex) : null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;
            ShowMainMenu();
            audioManager?.PlayMenuBGM();
        }

        public void StartGame()
        {
            currentLevelIndex = 0;
            StartLevel(currentLevelIndex);
        }

        public void StartNextLevel()
        {
            if (!CanGoNextLevel())
            {
                currentState = GameState.Win;
                uiManager?.ShowAllLevelsComplete();
                return;
            }

            currentLevelIndex++;
            StartLevel(currentLevelIndex);
        }

        public void RestartCurrentLevel()
        {
            StartLevel(currentLevelIndex);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            StopAllCoroutines();
            ResetRoundState();
            currentState = GameState.Idle;
            isBusy = false;
            levelManager?.ClearCards();
            audioManager?.StopBGM();
            audioManager?.PlayMenuBGM();
            ShowMainMenu();
        }

        public void OnCardClicked(Card card)
        {
            if (card == null || currentState != GameState.Playing || isBusy)
                return;

            if (selectedCards.Count >= 2 || card.IsFaceUp || card.IsMatched || card.IsFlipping)
                return;

            if (selectedCards.Count == 0)
                currentAttemptRecords.Clear();

            selectedCards.Add(card);
            currentAttemptRecords.Add(new FlipRecord(card, card.IsMatched));
            card.FlipToFront();
            audioManager?.PlayFlipSound();

            if (selectedCards.Count == 2)
            {
                pendingAttemptCommitted = true;
                attemptsUsed++;
                RefreshHUD();
                UpdateUndoAvailability();
                StartCoroutine(ResolveSelectedCards());
            }
            else
            {
                RefreshHUD();
                UpdateUndoAvailability();
            }
        }

        public void UndoLastFlip()
        {
            if (!CanUndo())
                return;

            StopAllCoroutines();
            isBusy = false;
            currentState = GameState.Playing;

            bool revertedAny = false;
            for (int i = currentAttemptRecords.Count - 1; i >= 0; i--)
            {
                var record = currentAttemptRecords[i];
                if (record?.card == null || record.wasMatched)
                    continue;

                if (record.card.IsFaceUp)
                {
                    record.card.ForceFlipImmediate(false);
                    revertedAny = true;
                }
            }

            if (!revertedAny)
                return;

            selectedCards.Clear();
            currentAttemptRecords.Clear();

            if (pendingAttemptCommitted && attemptsUsed > 0)
                attemptsUsed--;

            pendingAttemptCommitted = false;
            RefreshHUD();
            UpdateUndoAvailability();
        }

        public void OnPauseButtonClicked()
        {
            if (currentState != GameState.Playing || isBusy)
                return;

            Time.timeScale = 0f;
            uiManager?.ShowPausePanel(true);
        }

        public void OnResumeButtonClicked()
        {
            Time.timeScale = 1f;
            uiManager?.ShowPausePanel(false);
        }

        private void StartLevel(int levelIndex)
        {
            if (levelManager == null || uiManager == null)
            {
                Debug.LogError("GameManager 依赖未绑定完整");
                return;
            }

            Time.timeScale = 1f;
            StopAllCoroutines();

            currentLevelIndex = levelIndex;
            ResetRoundState();
            currentState = GameState.Playing;
            isBusy = false;

            levelManager.ClearCards();
            levelManager.GenerateLevel(currentLevelIndex);

            uiManager.ShowMainMenu(false);
            uiManager.ShowAllLevelsCompletePanel(false);
            uiManager.ShowPausePanel(false);
            uiManager.ShowResultPanel(false);
            uiManager.ShowGameUI(true);
            uiManager.UpdateLevelText(currentLevelIndex + 1);

            audioManager?.StopBGM();
            audioManager?.PlayBGM();

            RefreshHUD();
            UpdateUndoAvailability();
        }

        private IEnumerator ResolveSelectedCards()
        {
            currentState = GameState.Checking;
            isBusy = true;

            yield return new WaitUntil(() => selectedCards.Count >= 2 && !selectedCards[0].IsFlipping && !selectedCards[1].IsFlipping);

            Card first = selectedCards[0];
            Card second = selectedCards[1];

            if (first.CardId == second.CardId)
            {
                first.SetMatched(true);
                second.SetMatched(true);
                matchedPairs++;
                pendingAttemptCommitted = false;
                currentAttemptRecords.Clear();
                selectedCards.Clear();
                audioManager?.PlayMatchSuccessSound();

                if (CurrentConfig != null && matchedPairs >= CurrentConfig.PairCount)
                {
                    HandleLevelWin();
                    yield break;
                }
            }
            else
            {
                audioManager?.PlayMatchFailSound();
                first.PlayErrorAnimation();
                second.PlayErrorAnimation();
                yield return new WaitForSeconds(mismatchPreviewDuration);
                first.FlipToBack();
                second.FlipToBack();
                yield return new WaitUntil(() => !first.IsFlipping && !second.IsFlipping);
                currentAttemptRecords.Clear();
                pendingAttemptCommitted = false;
                selectedCards.Clear();
            }

            isBusy = false;
            currentState = GameState.Playing;
            RefreshHUD();
            UpdateUndoAvailability();

            if (CurrentConfig != null && attemptsUsed >= CurrentConfig.maxFlipCount && matchedPairs < CurrentConfig.PairCount)
                HandleLevelLose();
        }

        private void HandleLevelWin()
        {
            currentState = GameState.Win;
            isBusy = false;
            audioManager?.PlayLevelCompleteSound();
            uiManager?.ShowWinPanel(CanGoNextLevel(), currentLevelIndex + 1, attemptsUsed, CurrentConfig != null ? CurrentConfig.maxFlipCount : attemptsUsed);
            UpdateUndoAvailability();
        }

        private void HandleLevelLose()
        {
            currentState = GameState.Lose;
            isBusy = false;
            audioManager?.PlayLevelFailSound();
            uiManager?.ShowLosePanel(currentLevelIndex + 1, attemptsUsed, CurrentConfig != null ? CurrentConfig.maxFlipCount : attemptsUsed);
            UpdateUndoAvailability();
        }

        private bool CanGoNextLevel()
        {
            return levelManager != null && currentLevelIndex + 1 < levelManager.LevelCount;
        }

        private void ShowMainMenu()
        {
            uiManager?.ShowPausePanel(false);
            uiManager?.ShowResultPanel(false);
            uiManager?.ShowAllLevelsCompletePanel(false);
            uiManager?.ShowGameUI(false);
            uiManager?.ShowMainMenu(true);
        }

        private void RefreshHUD()
        {
            if (uiManager == null || CurrentConfig == null)
                return;

            uiManager.UpdateLevelText(currentLevelIndex + 1);
            uiManager.UpdateFlipText(attemptsUsed, CurrentConfig.maxFlipCount);
            uiManager.SetUndoButtonEnabled(CanUndo());
        }

        private void UpdateUndoAvailability()
        {
            uiManager?.SetUndoButtonEnabled(CanUndo());
        }

        private bool CanUndo()
        {
            if (currentState != GameState.Playing || isBusy || currentAttemptRecords.Count == 0)
                return false;

            foreach (var record in currentAttemptRecords)
            {
                if (record?.card != null && !record.wasMatched && record.card.IsFaceUp)
                    return true;
            }

            return false;
        }

        private void ResetRoundState()
        {
            selectedCards.Clear();
            currentAttemptRecords.Clear();
            attemptsUsed = 0;
            matchedPairs = 0;
            pendingAttemptCommitted = false;
        }
    }
}
