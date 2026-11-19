
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static GameFlowState State { get; private set; }


    // DEFAULT VALUES

    private const int DefaultLives = 3;
    private const int DefaultWeaponLevel = 1;
    private const int DefaultBombCount = 2;


    // PLAYER PROGRESS

    public int Score { get; private set; }

    public int PlayerLives { get; private set; } = DefaultLives;

    public int WeaponPowerLevel { get; private set; } = DefaultWeaponLevel;

    public int BombCount { get; private set; } = DefaultBombCount;


    // RESET STATIC DATA

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        State = GameFlowState.MainMenu;
    }


    // CREATE GAME MANAGER

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
            return;

        GameObject managerObject =
            new GameObject("GameManager");

        managerObject.AddComponent<GameManager>();
    }


    // AWAKE

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);


        // PLAYER EVENTS

        GameEvents.PlayerLivesChanged += HandlePlayerLivesChanged;
        GameEvents.PlayerDied += HandlePlayerDied;


        // WEAPON EVENTS

        GameEvents.WeaponLevelChanged += HandleWeaponLevelChanged;

        GameEvents.BombCountChanged += HandleBombCountChanged;


        // SCORE EVENTS

        GameEvents.ScoreAwarded += HandleScoreAwarded;


        // SCENE EVENTS

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }


    // NEW GAME

    public void BeginNewGame()
    {
        Score = 0;

        PlayerLives = DefaultLives;

        WeaponPowerLevel = DefaultWeaponLevel;

        BombCount = DefaultBombCount;

        State = GameFlowState.Playing;
        Time.timeScale = 1f;


        GameEvents.RaiseScoreChanged(Score);

        GameEvents.RaisePlayerLivesChanged(PlayerLives);

        GameEvents.RaiseWeaponLevelChanged(WeaponPowerLevel);

        GameEvents.RaiseBombCountChanged(BombCount);


        Debug.Log(
            "[GameManager] New Game Started! " +
            $"Score={Score}, Lives={PlayerLives}, " +
            $"Weapon={WeaponPowerLevel}, Bombs={BombCount}",
            this);
    }


    // RETRY CURRENT LEVEL

    public void ResetForRetry()
    {
        Score = 0;
        PlayerLives = DefaultLives;
        WeaponPowerLevel = DefaultWeaponLevel;
        BombCount = DefaultBombCount;

        State = GameFlowState.Playing;
        Time.timeScale = 1f;

        GameEvents.RaiseScoreChanged(Score);
        GameEvents.RaisePlayerLivesChanged(PlayerLives);
        GameEvents.RaiseWeaponLevelChanged(WeaponPowerLevel);
        GameEvents.RaiseBombCountChanged(BombCount);

        Debug.Log(
            "[GameManager] Retry prepared with default player resources.",
            this);
    }


    private void HandlePlayerDied()
    {
        State = GameFlowState.GameOver;
        Time.timeScale = 0f;

        Debug.Log(
            "[GameManager] GAME OVER. Gameplay paused.",
            this);
    }


    // GAME STATE

    public static void SetState(GameFlowState state)
    {
        State = state;
    }


    // ADD SCORE

    public void AddScore(int amount)
    {
        if (amount == 0)
            return;

        int newScore = Mathf.Max(0, Score + amount);

        if (newScore == Score)
            return;

        Score = newScore;

        GameEvents.RaiseScoreChanged(Score);
    }


    // SCORE EVENT LISTENER

    private void HandleScoreAwarded(int amount)
    {
        if (amount <= 0)
            return;

        AddScore(amount);

        Debug.Log(
            $"[GameManager] Score Awarded: +{amount} | Total: {Score}",
            this);
    }


    // PLAYER PROGRESS

    public void SetPlayerProgress(
        int lives,
        int weaponLevel,
        int bombs)
    {
        PlayerLives = Mathf.Max(0, lives);

        WeaponPowerLevel = Mathf.Max(1, weaponLevel);

        BombCount = Mathf.Max(0, bombs);


        GameEvents.RaisePlayerLivesChanged(PlayerLives);

        GameEvents.RaiseWeaponLevelChanged(WeaponPowerLevel);

        GameEvents.RaiseBombCountChanged(BombCount);
    }


    // PLAYER EVENT LISTENER

    private void HandlePlayerLivesChanged(int lives)
    {
        PlayerLives = Mathf.Max(0, lives);
    }


    // WEAPON EVENT LISTENER

    private void HandleWeaponLevelChanged(int level)
    {
        WeaponPowerLevel = Mathf.Max(1, level);
    }


    private void HandleBombCountChanged(int bombs)
    {
        BombCount = Mathf.Max(0, bombs);

        Debug.Log(
            $"[GameManager] Bomb inventory updated: {BombCount}",
            this);
    }


    // SCENE EVENT LISTENER

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        State = scene.name == "MainMenu"
            ? GameFlowState.MainMenu
            : GameFlowState.Playing;
    }


    // CLEANUP

    private void OnDestroy()
    {
        if (Instance != this)
            return;


        GameEvents.PlayerLivesChanged -= HandlePlayerLivesChanged;
        GameEvents.PlayerDied -= HandlePlayerDied;

        GameEvents.WeaponLevelChanged -= HandleWeaponLevelChanged;

        GameEvents.BombCountChanged -= HandleBombCountChanged;

        GameEvents.ScoreAwarded -= HandleScoreAwarded;


        SceneManager.sceneLoaded -= HandleSceneLoaded;


        Instance = null;
    }
}
