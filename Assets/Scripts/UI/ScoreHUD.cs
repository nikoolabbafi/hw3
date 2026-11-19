using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class ScoreHUD : MonoBehaviour
{
    [SerializeField]
    private string textPrefix = "SCORE: ";

    private TMP_Text scoreText;

    private void Awake()
    {
        scoreText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        GameEvents.ScoreChanged += UpdateScore;

        int initialScore = GameManager.Instance != null
            ? GameManager.Instance.Score
            : 0;

        UpdateScore(initialScore);
    }

    private void OnDisable()
    {
        GameEvents.ScoreChanged -= UpdateScore;
    }

    private void UpdateScore(int score)
    {
        if (scoreText == null)
            return;

        scoreText.text = $"{textPrefix}{score}";
    }
}