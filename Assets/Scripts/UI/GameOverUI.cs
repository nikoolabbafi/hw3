using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool hasShown;

    private void Awake()
    {
        if (gameOverPanel == null)
        {
            Debug.LogError(
                "[GameOverUI] Game Over Panel is not assigned.",
                this
            );
            return;
        }

        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.PlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.PlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        
        if (GameManager.State == GameFlowState.GameOver)
        {
            ShowGameOver();
        }
    }

    private void HandlePlayerDied()
    {
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        if (hasShown)
            return;

        hasShown = true;

        GameManager.SetState(GameFlowState.GameOver);
        Time.timeScale = 0f;

        if (finalScoreText != null)
        {
            int score = GameManager.Instance != null
                ? GameManager.Instance.Score
                : 0;

            finalScoreText.text = $"SCORE: {score}";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("[GameOverUI] Game Over panel displayed.", this);
    }

   
    public void RetryCurrentLevel()
    {
        if (GameManager.State != GameFlowState.GameOver)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetForRetry();
        }
        else
        {
            Debug.LogError(
                "[GameOverUI] GameManager is missing; Retry was cancelled.",
                this
            );
            return;
        }

        Time.timeScale = 1f;
        LevelTransitionManager.ReloadCurrentScene();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.SetState(GameFlowState.MainMenu);
        LevelTransitionManager.LoadScene(mainMenuSceneName);
    }
}
