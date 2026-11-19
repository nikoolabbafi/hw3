using UnityEngine;
using UnityEngine.SceneManagement;


public static class LevelTransitionManager
{
    private const int FinalLevelNumber = 3;

    private static LevelTransitionRunner runner;
    private static bool transitionInProgress;
    private static bool victoryAlreadyShown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        runner = null;
        transitionInProgress = false;
        victoryAlreadyShown = false;
    }

    public static void LoadLevel(LevelDataSO level)
    {
        if (level == null)
        {
            Debug.LogError("[LevelTransitionManager] LevelDataSO is null.");
            return;
        }

        LoadScene(level.SceneName);
    }

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[LevelTransitionManager] Scene name is empty.");
            return;
        }

        if (transitionInProgress)
            return;

        if (SceneManager.GetActiveScene().name == sceneName)
            return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[LevelTransitionManager] Scene '{sceneName}' cannot be loaded. " +
                "Check its name and add it to Build Profiles / Scene List.");
            return;
        }

        transitionInProgress = true;
        Time.timeScale = 1f;
        EnsureRunner().LoadScene(sceneName);
    }

    
    public static void ReloadCurrentScene()
    {
        if (transitionInProgress)
            return;

        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid() ||
            string.IsNullOrWhiteSpace(activeScene.name))
        {
            Debug.LogError(
                "[LevelTransitionManager] There is no valid active scene to reload.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(activeScene.name))
        {
            Debug.LogError(
                $"[LevelTransitionManager] Active scene '{activeScene.name}' " +
                "cannot be reloaded. Check Build Profiles / Scene List.");
            return;
        }

        transitionInProgress = true;
        Time.timeScale = 1f;
        EnsureRunner().LoadScene(activeScene.name);
    }

    public static void CompleteLevel(LevelDataSO completedLevel)
    {
        if (completedLevel == null || transitionInProgress || victoryAlreadyShown)
            return;

        int nextLevelNumber = completedLevel.LevelNumber + 1;
        LevelDataSO nextLevel = LevelDataCatalog.GetLevel(nextLevelNumber);

        if (nextLevel != null)
        {
            if (!Application.CanStreamedLevelBeLoaded(nextLevel.SceneName))
            {
                Debug.LogError(
                    $"[LevelTransitionManager] Next scene '{nextLevel.SceneName}' " +
                    "is not in Build Profiles / Scene List.");
                return;
            }

            transitionInProgress = true;

            EnsureRunner().CompleteAndLoad(
                completedLevel.LevelNumber,
                nextLevel.LevelNumber,
                nextLevel.SceneName);

            return;
        }

        if (completedLevel.LevelNumber != FinalLevelNumber)
        {
            Debug.LogError(
                $"[LevelTransitionManager] Level {nextLevelNumber} data was not found. " +
                "Victory will not be shown because this is not the configured final level.");
            return;
        }

        victoryAlreadyShown = true;

        EnsureRunner().ShowFinalVictoryAfterDelay();

        Debug.Log(
            "[LevelTransitionManager] Final level completed. " +
            "Victory will be shown after the boss-defeat delay.");
    }

    private static LevelTransitionRunner EnsureRunner()
    {
        if (runner != null)
            return runner;

        GameObject runnerObject = new GameObject("LevelTransitionRunner");
        Object.DontDestroyOnLoad(runnerObject);
        runner = runnerObject.AddComponent<LevelTransitionRunner>();
        return runner;
    }

    internal static void NotifySceneLoaded()
    {
        transitionInProgress = false;
        victoryAlreadyShown = false;
        Time.timeScale = 1f;
        GameFlowOverlay.Hide();
    }
}
