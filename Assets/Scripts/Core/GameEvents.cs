
using System;
using UnityEngine;

public static class GameEvents
{
    // PLAYER

    public static event Action PlayerDamaged;
    public static event Action<int> PlayerLivesChanged;
    public static event Action PlayerDied;


    // WEAPON

    public static event Action<int> WeaponLevelChanged;
    public static event Action<int> BombCountChanged;


    // SCORE

    public static event Action<int> ScoreAwarded;
    public static event Action<int> ScoreChanged;


    // ENEMY

    public static event Action EnemyDefeated;
    public static event Action AsteroidDestroyed;


    // WAVE

    public static event Action<int> WaveStarted;
    public static event Action<int> WaveCompleted;
    public static event Action AllWavesCompleted;


    // PLAYER EVENTS

    public static void RaisePlayerDamaged()
    {
        PlayerDamaged?.Invoke();
    }

    public static void RaisePlayerLivesChanged(int lives)
    {
        PlayerLivesChanged?.Invoke(lives);
    }

    public static void RaisePlayerDied()
    {
        PlayerDied?.Invoke();
    }


    // WEAPON EVENTS

    public static void RaiseWeaponLevelChanged(int level)
    {
        WeaponLevelChanged?.Invoke(level);
    }

    public static void RaiseBombCountChanged(int bombs)
    {
        BombCountChanged?.Invoke(bombs);
    }


    // SCORE EVENTS

    public static void RaiseScoreAwarded(int amount)
    {
        if (amount <= 0)
            return;

        ScoreAwarded?.Invoke(amount);
    }

    public static void RaiseScoreChanged(int score)
    {
        ScoreChanged?.Invoke(score);
    }


    // ENEMY EVENTS

    public static void RaiseEnemyDefeated()
    {
        EnemyDefeated?.Invoke();
    }

    public static void RaiseAsteroidDestroyed()
    {
        AsteroidDestroyed?.Invoke();
    }


    // WAVE EVENTS

    public static void RaiseWaveStarted(int waveNumber)
    {
        WaveStarted?.Invoke(waveNumber);
    }

    public static void RaiseWaveCompleted(int waveNumber)
    {
        WaveCompleted?.Invoke(waveNumber);
    }

    public static void RaiseAllWavesCompleted()
    {
        AllWavesCompleted?.Invoke();
    }


    // RESET EVENTS

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents()
    {
        PlayerDamaged = null;
        PlayerLivesChanged = null;
        PlayerDied = null;

        WeaponLevelChanged = null;
        BombCountChanged = null;

        ScoreAwarded = null;
        ScoreChanged = null;

        EnemyDefeated = null;
        AsteroidDestroyed = null;

        WaveStarted = null;
        WaveCompleted = null;
        AllWavesCompleted = null;
    }
}
