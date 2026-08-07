using UnityEngine;
using System;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    RoomManager roomManager;

    public int wavePeriod;

    int currrentWave;

    public Action<int> OnWaveStarted;

    void Start()
    {
        StartNextWave();
    }

    public void FinishWave()
    {
        currrentWave++;

        if(currrentWave % wavePeriod == 0)
        {
            roomManager.SpawnNextRoom();
        }

        StartNextWave();
    }

    void StartNextWave()
    {
        Debug.Log($"Wave {currrentWave}");

        OnWaveStarted?.Invoke(currrentWave);
    }
}