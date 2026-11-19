
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelCatalog",
    menuName = "Chicken Shooter/Data/Level Catalog"
)]
public class LevelCatalogSO : ScriptableObject
{
    [Header("Available Levels")]

    [SerializeField]
    private LevelDataSO[] levels = new LevelDataSO[0];

    public LevelDataSO GetLevel(int levelNumber)
    {
        if (levels == null || levelNumber < 1)
            return null;

        foreach (LevelDataSO level in levels)
        {
            if (level != null &&
                level.LevelNumber == levelNumber)
            {
                return level;
            }
        }

        return null;
    }

    private void OnValidate()
    {
        if (levels == null)
            return;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == null)
                continue;

            for (int j = i + 1; j < levels.Length; j++)
            {
                if (levels[j] != null &&
                    levels[i].LevelNumber == levels[j].LevelNumber)
                {
                    Debug.LogWarning(
                        $"Duplicate Level Number: {levels[i].LevelNumber}",
                        this
                    );
                }
            }
        }
    }
}
