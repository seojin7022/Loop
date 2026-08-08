using UnityEngine;

public class GameManager : MonoBehaviour
{
    public WaveManager waveManager;
    public EnemySpawner spawner;

    public void WaveCleared()
    {
        waveManager.FinishWave();
    }
}
