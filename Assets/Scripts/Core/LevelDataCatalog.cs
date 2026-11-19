
using UnityEngine;

public static class LevelDataCatalog
{
    private const string CatalogResourcePath = "LevelCatalog";

    private static LevelCatalogSO catalog;

    private static bool loadAttempted;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetCache()
    {
        catalog = null;
        loadAttempted = false;
    }

    private static LevelCatalogSO GetCatalog()
    {
        if (catalog != null)
            return catalog;

        if (loadAttempted)
            return null;

        loadAttempted = true;

        catalog = Resources.Load<LevelCatalogSO>(
            CatalogResourcePath
        );

        if (catalog == null)
        {
            Debug.LogError(
                "[LevelDataCatalog] LevelCatalog asset not found! " +
                "Expected path: Assets/Resources/LevelCatalog.asset"
            );
        }

        return catalog;
    }

    public static LevelDataSO GetLevel(int number)
    {
        LevelCatalogSO source = GetCatalog();

        if (source == null)
            return null;

        return source.GetLevel(number);
    }
}
