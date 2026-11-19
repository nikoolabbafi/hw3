
using UnityEngine;
using UnityEngine.Pool;

public class PooledObject : MonoBehaviour
{
    private ObjectPool<GameObject> ownerPool;

    internal void SetOwner(ObjectPool<GameObject> pool)
    {
        ownerPool = pool;
    }

    public void ReturnToPool()
    {
        
        if (!gameObject.activeSelf)
            return;

        if (ownerPool == null)
        {
            Debug.LogWarning(
                name + " has no pool assigned."
            );

            gameObject.SetActive(false);
            return;
        }

        ownerPool.Release(gameObject);
    }
}
