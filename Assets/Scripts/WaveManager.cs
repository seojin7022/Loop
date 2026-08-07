using UnityEngine;
using System;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    RoomManager roomManager;

    [SerializeField]
    int wavesPerExpansion = 3;

    public int CurrentWave { get; private set; }

    public Action<int> OnWaveStarted;

    void Start()
    {
        StartNextWave();
    }

    public void FinishWave()
    {
        CurrentWave++;

        if(CurrentWave % wavesPerExpansion == 0)
        {
            roomManager.SpawnNextRoom();
        }

        StartNextWave();
    }

    void StartNextWave()
    {
        Debug.Log($"Wave {CurrentWave}");

        OnWaveStarted?.Invoke(CurrentWave);
    }
}