using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        LevelDataSO firstLevel =
            LevelDataCatalog.GetLevel(1);

        if (firstLevel == null)
        {
            Debug.LogError(
                "[MainMenuController] Level 1 not found!"
            );
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[MainMenuController] GameManager not found!"
            );
            return;
        }

        GameManager.Instance.BeginNewGame();

        LevelTransitionManager.LoadLevel(firstLevel);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
