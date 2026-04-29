using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace MemoryMatch.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Game UI")]
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text flipText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button hintButton;

        [Header("Result Panel")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitle;
        [SerializeField] private TMP_Text resultScoreText;
        [SerializeField] private TMP_Text resultComboText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMenuButton;

        [Header("All Levels Complete")]
        [SerializeField] private GameObject allLevelsCompletePanel;
        [SerializeField] private Button allLevelsMenuButton;

        [Header("Combo Effect")]
        [SerializeField] private GameObject comboEffectPrefab;
        [SerializeField] private Transform comboEffectParent;

        [Header("Animation")]
        [SerializeField] private float panelFadeDuration = 0.3f;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(() => GameManager.Instance.StartGame());
            
            if (restartButton != null)
                restartButton.onClick.AddListener(() => GameManager.Instance.RestartCurrentLevel());
            
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(() => GameManager.Instance.StartNextLevel());
            
            if (menuButton != null)
                menuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());
            
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => GameManager.Instance.OnPauseButtonClicked());
            
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => GameManager.Instance.OnResumeButtonClicked());
            
            if (pauseRestartButton != null)
                pauseRestartButton.onClick.AddListener(() => {
                    GameManager.Instance.OnResumeButtonClicked();
                    GameManager.Instance.RestartCurrentLevel();
                });
            
            if (pauseMenuButton != null)
                pauseMenuButton.onClick.AddListener(() => {
                    GameManager.Instance.OnResumeButtonClicked();
                    GameManager.Instance.ReturnToMainMenu();
                });
            
            if (undoButton != null)
                undoButton.onClick.AddListener(() => GameManager.Instance.UndoLastFlip());
            
            if (allLevelsMenuButton != null)
                allLevelsMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());

            if (quitButton != null)
                quitButton.onClick.AddListener(() => {
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                });
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

        public void ShowWinPanel(bool hasNextLevel, int score, int combo)
        {
            if (resultTitle != null) resultTitle.text = "通关成功！";
            if (resultScoreText != null) resultScoreText.text = $"得分: {score}";
            if (resultComboText != null) resultComboText.text = $"最高连击: {combo}";
            
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(hasNextLevel);
            
            ShowResultPanel(true);
        }

        public void ShowLosePanel(int score)
        {
            if (resultTitle != null) resultTitle.text = "挑战失败";
            if (resultScoreText != null) resultScoreText.text = $"得分: {score}";
            if (resultComboText != null) resultComboText.text = "再试一次吧！";
            
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
            
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
            {
                if (max > 0)
                    flipText.text = $"翻牌: {current} / {max}";
                else
                    flipText.text = $"翻牌: {current}";
            }
        }

        public void UpdateScoreText(int score)
        {
            if (scoreText != null)
                scoreText.text = $"得分: {score}";
        }

        public void UpdateComboText(int combo)
        {
            if (comboText != null)
            {
                comboText.text = combo > 1 ? $"连击 x{combo}" : "";
                comboText.gameObject.SetActive(combo > 1);
            }
        }

        public void ShowComboEffect(int combo)
        {
            if (comboEffectPrefab != null && combo > 1)
            {
                GameObject effect = Instantiate(comboEffectPrefab, comboEffectParent);
                TMP_Text effectText = effect.GetComponentInChildren<TMP_Text>();
                if (effectText != null)
                    effectText.text = $"x{combo} 连击!";
                
                Destroy(effect, 1.5f);
            }
        }

        public void SetUndoButtonEnabled(bool enabled)
        {
            if (undoButton != null)
                undoButton.interactable = enabled;
        }

        private IEnumerator FadePanel(GameObject panel, bool show)
        {
            if (panel == null) yield break;

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
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = elapsed / panelFadeDuration;
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
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = 1f - (elapsed / panelFadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                panel.SetActive(false);
            }
        }
    }
}