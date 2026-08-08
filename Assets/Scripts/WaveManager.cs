using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    RoomManager roomManager;
    [SerializeField]
    EnemySpawner spawner;
    [SerializeField]
    int hp;

    public int wavePeriod;
    public int maxExpansionStage;

    [Header("페이즈")]
    [Tooltip("스테이지 시작 전 특성 3택 1 선택 화면을 띄운다.")]
    [SerializeField] bool relicSelectionEnabled = true;

    [Tooltip("웨이브 시작 전 적 생성 위치와 이동 동선을 표시하고 대기한다.")]
    [SerializeField] bool wavePreviewEnabled = true;

    int currrentWave;
    int nowEnemyNum;
    int maxHp;
    bool isGameOver;
    bool waveRunning;

    public static WaveManager Instance { get; private set; }

    public int Hp => hp;
    public int MaxHp => maxHp;
    public int CurrentWave => currrentWave;
    public int RemainingEnemies => nowEnemyNum;
    public bool IsGameOver => isGameOver;
    public bool IsWaveRunning => waveRunning;
    public int CurrentStage => StageTable.StageOfWave(currrentWave, SafePeriod);

    int SafePeriod => Mathf.Max(1, wavePeriod);

    public void Awake()
    {
        Instance = this;
        maxHp = hp;

        EventBus.OnEvent("EnemyDie")
                .Subscribe(_ => EnemyDie())
                .AddTo(this);

        EventBus.OnEvent("PlayerDamage")
                .Subscribe(_ => PlayerDamage())
                .AddTo(this);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        RunWaveAsync().Forget();
    }

    public void EnemyDie()
    {
        if (isGameOver || !waveRunning) return;

        nowEnemyNum -= 1;
        if (nowEnemyNum <= 0) FinishWave();
    }

    public void PlayerDamage()
    {
        if (isGameOver) return;

        hp -= 1;
        if (waveRunning) nowEnemyNum -= 1;

        Sfx.PlayerDamage(Vector3.zero);

        if (hp <= 0)
        {
            GameOver();
            return;
        }

        if (waveRunning && nowEnemyNum <= 0) FinishWave();
    }

    /// 특성 선택 → 동선 사전 표시 → 웨이브 시작 순서로 진행한다.
    async UniTaskVoid RunWaveAsync()
    {
        if (isGameOver || spawner == null) return;

        // RoomManager.Start() 등 다른 초기화가 끝난 뒤에 방/스폰 정보를 읽는다.
        await UniTask.Yield(PlayerLoopTiming.Update);
        if (this == null || isGameOver) return;

        if (relicSelectionEnabled && currrentWave % SafePeriod == 0)
        {
            RelicSelectUI ui = RelicSelectUI.Ensure();
            if (ui != null) await ui.ShowAndWaitAsync();
        }

        if (this == null || isGameOver) return;

        // 방 증축과 함께 내부 반사벽도 스테이지에 맞게 다시 배치한다.
        if (roomManager != null)
            roomManager.DrawAllRooms();

        List<SpawnLane> lanes = spawner.BuildLanes(currrentWave, SafePeriod);
        if (this == null || isGameOver) return;


        int expected = spawner.GetWaveEnemyCount(currrentWave, SafePeriod, lanes.Count);

        if (wavePreviewEnabled && lanes.Count > 0)
        {
            var preview = WavePreview.Instance;
            if (preview != null) await preview.ShowAndWaitAsync(currrentWave, expected, lanes);
        }

        if (this == null || isGameOver) return;

        StartWave();
    }

    void StartWave()
    {
        Debug.Log($"[Wave] 스테이지 {CurrentStage} / 웨이브 {currrentWave + 1} 시작");

        waveRunning = true;
        nowEnemyNum = spawner.SpawnWave(currrentWave, SafePeriod);

        if (nowEnemyNum <= 0)
        {
            // 진입 라인이 없거나 처치 목표가 0이면 더 진행할 수 없다. 무한 루프를 막고 알린다.
            waveRunning = false;
            Debug.LogError("[Wave] 이번 웨이브에 스폰할 적이 없습니다. RoomManager 의 Room Data(스폰 포인트)와 스테이지 표를 확인하세요.");
            return;
        }

        Sfx.WaveStart();
        Tutorial.Trigger("wave_started");
    }

    public void FinishWave()
    {
        if (isGameOver || !waveRunning) return;

        waveRunning = false;
        nowEnemyNum = 0;

        Sfx.WaveClear();
        Tutorial.Trigger("wave_cleared");

        currrentWave++;

        // 일정 웨이브마다 방을 확장한다.
        if (roomManager != null && currrentWave % SafePeriod == 0 && CurrentStage <= maxExpansionStage)
        {
            roomManager.SpawnNextRoom();
            Sfx.RoomAdded();
        }

        RunWaveAsync().Forget();
    }

    void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        waveRunning = false;

        EventBus.Publish("PlayerDie");
        Sfx.GameOver();

        GameOverUI ui = GameOverUI.Ensure();
        if (ui != null) ui.Show(currrentWave, CurrentStage);
    }
}
