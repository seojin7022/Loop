using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        if(_instance == null)
            _instance = this;
        else if(_instance != this)
            Destroy(this);
        
        DontDestroyOnLoad(gameObject);

        playback.Enable();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Ingame")
        {
            if(!didTutorial)
                await tutorial.PlayTutorial();

            didTutorial = true;
            
            Debug.Log("Game Start");
            waveManager.RunWaveAsync().Forget();
        }
    }

    bool didTutorial;

    public Tutorial tutorial;
    public WaveManager waveManager;
    public EnemySpawner spawner;

    public void WaveCleared()
    {
        waveManager.FinishWave();
    }

    public InputAction playback;
    public float holdThrehold, playbackSpeed;

    bool didPlayback;
    float holdTime;

    void Update()
    {
        if(playback.IsPressed())
        {
            holdTime += Time.unscaledDeltaTime;
        }
        else
            holdTime = 0;
        
        if(holdTime > holdThrehold)
        {
            didPlayback = true;
            Time.timeScale = playbackSpeed;
        }
        else
            Time.timeScale = 1.0f;
        
        if(playback.WasReleasedThisFrame() && didPlayback)
            Tutorial.Trigger("Playback");
    }
}
