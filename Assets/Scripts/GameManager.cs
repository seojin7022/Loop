using UnityEngine;

public class GameManager : MonoBehaviour
{
    public WaveManager waveManager;
    public EnemySpawner spawner;

    void Awake()
    {
        waveManager.OnWaveStarted += OnWaveStarted;
    }

    void OnDestroy()
    {
        waveManager.OnWaveStarted -= OnWaveStarted;
    }

    void OnWaveStarted(int wave)
    {
        spawner.SpawnWave(wave, waveManager.wavePeriod);
    }

    public void WaveCleared()
    {
        waveManager.FinishWave();
    }
}
