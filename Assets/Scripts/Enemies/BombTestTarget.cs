using UnityEngine;

public class BombTestTarget : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float health = 5f;

    public void TakeDamage(float damage)
    {
        if (health <= 0f)
            return;

        health = Mathf.Max(0f, health - damage);

        Debug.Log(
            $"[Bomb Test] Damage: {damage} | Remaining: {health}"
        );

        if (health <= 0f)
        {
            Debug.Log("[Bomb Test] Target Destroyed!");
            gameObject.SetActive(false);
        }
    }
}