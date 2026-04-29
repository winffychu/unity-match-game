using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        [Header("Game UI")]
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text flipText;
        [SerializeField] private Button undoButton;

        [Header("Result Panel")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitle;
        [SerializeField] private TMP_Text resultDetailText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMenuButton;
        [SerializeField] private Button pauseButton;

        [Header("All Levels Complete")]
        [SerializeField] private GameObject allLevelsCompletePanel;
        [SerializeField] private Button allLevelsMenuButton;

        [Header("Animation")]
        [SerializeField] private float panelFadeDuration = 0.2f;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(() => GameManager.Instance?.StartGame());
            if (restartButton != null)
                restartButton.onClick.AddListener(() => GameManager.Instance?.RestartCurrentLevel());
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(() => GameManager.Instance?.StartNextLevel());
            if (menuButton != null)
                menuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => GameManager.Instance?.OnPauseButtonClicked());
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => GameManager.Instance?.OnResumeButtonClicked());
            if (pauseRestartButton != null)
                pauseRestartButton.onClick.AddListener(() =>
                {
                    GameManager.Instance?.OnResumeButtonClicked();
                    GameManager.Instance?.RestartCurrentLevel();
                });
            if (pauseMenuButton != null)
                pauseMenuButton.onClick.AddListener(() =>
                {
                    GameManager.Instance?.OnResumeButtonClicked();
                    GameManager.Instance?.ReturnToMainMenu();
                });
            if (undoButton != null)
                undoButton.onClick.AddListener(() => GameManager.Instance?.UndoLastFlip());
            if (allLevelsMenuButton != null)
                allLevelsMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() =>
                {
                    Application.Quit();
                });
            }
        }

        public void ShowMainMenu(bool show)
        {
            StartCoroutine(FadePanel(mainMenuPanel, show));
        }

        public void ShowGameUI(bool show)
        {
            if (gameUIPanel != null)
                gameUIPanel.SetActive(show);
        }

        public void ShowResultPanel(bool show)
        {
            StartCoroutine(FadePanel(resultPanel, show));
        }

        public void ShowPausePanel(bool show)
        {
            if (pausePanel != null)
                pausePanel.SetActive(show);
        }

        public void ShowAllLevelsComplete()
        {
            ShowGameUI(false);
            ShowResultPanel(false);
            ShowMainMenu(false);
            if (allLevelsCompletePanel != null)
                allLevelsCompletePanel.SetActive(true);
        }

        public void ShowAllLevelsCompletePanel(bool show)
        {
            if (allLevelsCompletePanel != null)
                allLevelsCompletePanel.SetActive(show);
        }

        public void ShowWinPanel(bool hasNextLevel, int levelNumber, int usedAttempts, int maxAttempts)
        {
            if (resultTitle != null)
                resultTitle.text = $"第 {levelNumber} 关通关";
            if (resultDetailText != null)
                resultDetailText.text = $"已在 {usedAttempts} / {maxAttempts} 次翻牌内完成挑战";
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(hasNextLevel);
            ShowGameUI(false);
            ShowPausePanel(false);
            ShowResultPanel(true);
        }

        public void ShowLosePanel(int levelNumber, int usedAttempts, int maxAttempts)
        {
            if (resultTitle != null)
                resultTitle.text = $"第 {levelNumber} 关失败";
            if (resultDetailText != null)
                resultDetailText.text = $"翻牌次数已用尽：{usedAttempts} / {maxAttempts}";
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
            ShowGameUI(false);
            ShowPausePanel(false);
            ShowResultPanel(true);
        }

        public void UpdateLevelText(int level)
        {
            if (levelText != null)
                levelText.text = $"第 {level} 关";
        }

        public void UpdateFlipText(int current, int max)
        {
            if (flipText != null)
                flipText.text = $"翻牌次数: {current} / {max}";
        }

        public void SetUndoButtonEnabled(bool enabled)
        {
            if (undoButton != null)
                undoButton.interactable = enabled;
        }

        private IEnumerator FadePanel(GameObject panel, bool show)
        {
            if (panel == null)
                yield break;

            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            if (show)
            {
                panel.SetActive(true);
                canvasGroup.alpha = 0f;
                float elapsed = 0f;
                while (elapsed < panelFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / panelFadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = 1f;
                float elapsed = 0f;
                while (elapsed < panelFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / panelFadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                panel.SetActive(false);
            }
        }
    }
}
