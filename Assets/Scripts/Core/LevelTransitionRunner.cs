using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class LevelTransitionRunner : MonoBehaviour
{
  
    private const float BossDefeatDisplayDuration = 2.0f;


    private const float LoadingScreenDuration = 3.0f;

    private bool routineRunning;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        if (routineRunning)
            return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void CompleteAndLoad(int completedLevel, int nextLevel, string nextSceneName)
    {
        if (routineRunning)
            return;

        StartCoroutine(CompleteAndLoadRoutine(
            completedLevel,
            nextLevel,
            nextSceneName));
    }

    public void ShowFinalVictoryAfterDelay()
    {
        if (routineRunning)
            return;

        StartCoroutine(ShowFinalVictoryAfterDelayRoutine());
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        routineRunning = true;
        yield return null;

        SceneManager.LoadScene(sceneName);
        routineRunning = false;
    }

    private IEnumerator CompleteAndLoadRoutine(
        int completedLevel,
        int nextLevel,
        string nextSceneName)
    {
        routineRunning = true;

        Time.timeScale = 1f;
        Debug.Log(
            $"[LevelTransitionRunner] Level {completedLevel} completed. " +
            $"Showing boss defeat for {BossDefeatDisplayDuration:0.0} seconds.");

        yield return new WaitForSecondsRealtime(BossDefeatDisplayDuration);

        GameFlowOverlay.ShowLevelComplete(completedLevel, nextLevel);

        Debug.Log(
            $"[LevelTransitionRunner] Loading Level {nextLevel} in " +
            $"{LoadingScreenDuration:0.0} seconds.");

        yield return new WaitForSecondsRealtime(LoadingScreenDuration);

        Time.timeScale = 1f;
        GameFlowOverlay.Hide();

        SceneManager.LoadScene(nextSceneName);
        routineRunning = false;
    }

    private IEnumerator ShowFinalVictoryAfterDelayRoutine()
    {
        routineRunning = true;

        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(BossDefeatDisplayDuration);

        GameManager.SetState(GameFlowState.Victory);
        GameFlowOverlay.ShowVictory();

        Debug.Log("[LevelTransitionRunner] Final boss defeat delay finished. Victory shown.");

        routineRunning = false;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        routineRunning = false;
        LevelTransitionManager.NotifySceneLoaded();
        Debug.Log($"[LevelTransitionRunner] Loaded scene: {scene.name}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
}
