
using UnityEngine;

public class PlayerHealthTestListener : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.PlayerDamaged += HandlePlayerDamaged;
        GameEvents.PlayerLivesChanged += HandleLivesChanged;
        GameEvents.PlayerDied += HandlePlayerDied;

        Debug.Log("[Health Test] Listener Subscribed!");
    }

    private void OnDisable()
    {
        GameEvents.PlayerDamaged -= HandlePlayerDamaged;
        GameEvents.PlayerLivesChanged -= HandleLivesChanged;
        GameEvents.PlayerDied -= HandlePlayerDied;
    }

    private void HandlePlayerDamaged()
    {
        Debug.Log("[Health Test] Player Damaged!");
    }

    private void HandleLivesChanged(int lives)
    {
        Debug.Log("[Health Test] Remaining Lives: " + lives);
    }

    private void HandlePlayerDied()
    {
        Debug.Log("[Health Test] PLAYER DIED!");
    }
}
