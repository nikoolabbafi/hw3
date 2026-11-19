
using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewLevelData",
    menuName = "Chicken Shooter/Data/Level Data"
)]
public class LevelDataSO : ScriptableObject
{
    [Header("Level Identity")]

    [SerializeField, Min(1)]
    private int levelNumber = 1;

    [SerializeField]
    private string sceneName = "Level1";


    [Header("Environment")]

    [SerializeField]
    private AudioClip backgroundMusic;

    [SerializeField]
    private EnvironmentTheme environmentTheme =
        EnvironmentTheme.EarthOrbit;


    [Header("Round Configuration")]

    [Tooltip("Expected number of rounds in this level.")]
    [SerializeField, Min(1)]
    private int expectedRoundCount = 5;

    [SerializeField]
    private WaveDataSO[] rounds = new WaveDataSO[5];


    

    public int LevelNumber => levelNumber;

    public string SceneName => sceneName;

    public AudioClip BackgroundMusic => backgroundMusic;

    public EnvironmentTheme Theme => environmentTheme;

    public int ExpectedRoundCount => expectedRoundCount;

    public int RoundCount => rounds == null ? 0 : rounds.Length;

    public WaveDataSO[] Rounds =>
        rounds ?? Array.Empty<WaveDataSO>();




    public WaveDataSO GetRound(int roundNumber)
    {
        if (rounds == null ||
            roundNumber < 1 ||
            roundNumber > rounds.Length)
        {
            return null;
        }

        return rounds[roundNumber - 1];
    }


 

    [ContextMenu("Resize Rounds Array to Expected Count")]
    private void ResizeRoundsArray()
    {
        expectedRoundCount = Mathf.Max(1, expectedRoundCount);

        if (rounds == null)
            rounds = Array.Empty<WaveDataSO>();

        Array.Resize(ref rounds, expectedRoundCount);

        Debug.Log(
            $"[LevelDataSO] Rounds array resized to {expectedRoundCount}.",
            this
        );
    }


    

    [ContextMenu("Validate Level Data")]
    private void ValidateLevelData()
    {
        bool isValid = true;

        if (levelNumber < 1)
        {
            Debug.LogError(
                "[LevelDataSO] Level number must be at least 1.",
                this
            );

            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "[LevelDataSO] Scene name is missing.",
                this
            );

            isValid = false;
        }

        if (rounds == null || rounds.Length == 0)
        {
            Debug.LogError(
                "[LevelDataSO] No rounds configured.",
                this
            );

            isValid = false;
        }
        else
        {
            if (rounds.Length != expectedRoundCount)
            {
                Debug.LogError(
                    $"[LevelDataSO] Expected {expectedRoundCount} rounds, " +
                    $"but found {rounds.Length}.",
                    this
                );

                isValid = false;
            }

            for (int i = 0; i < rounds.Length; i++)
            {
                if (rounds[i] == null)
                {
                    Debug.LogError(
                        $"[LevelDataSO] Round {i + 1} is missing.",
                        this
                    );

                    isValid = false;
                }
            }

            WaveDataSO lastRound = rounds[rounds.Length - 1];

            if (lastRound != null && lastRound.BossPrefab == null)
            {
                Debug.LogError(
                    "[LevelDataSO] Final round has no Boss Prefab.",
                    this
                );

                isValid = false;
            }
        }

        if (isValid)
        {
            Debug.Log(
                $"[LevelDataSO] Level {levelNumber} validated successfully. " +
                $"Rounds: {rounds.Length}, Scene: {sceneName}",
                this
            );
        }
    }


    private void OnValidate()
    {
        levelNumber = Mathf.Max(1, levelNumber);
        expectedRoundCount = Mathf.Max(1, expectedRoundCount);

        if (rounds == null)
            rounds = new WaveDataSO[expectedRoundCount];
    }
}













































