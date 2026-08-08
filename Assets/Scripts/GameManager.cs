using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public WaveManager waveManager;
    public EnemySpawner spawner;

    public void WaveCleared()
    {
        waveManager.FinishWave();
    }

    public InputAction playback;
    public float holdThrehold, playbackSpeed;

    float holdTime;

    void Awake()
    {
        playback.Enable();
    }

    void Update()
    {
        if(playback.IsPressed())
        {
            holdTime += Time.unscaledDeltaTime;
        }
        else
            holdTime = 0;
        
        if(holdTime > holdThrehold)
            Time.timeScale = playbackSpeed;
        else
            Time.timeScale = 1.0f;
    }
}
