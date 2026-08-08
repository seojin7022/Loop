using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PulleyBun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 웨이브 시작 전 준비 페이즈.
/// 모든 방의 적 생성 위치와 이동 동선을 미리 표시하고, 플레이어가 거울을 배치한 뒤 시작하도록 대기한다.
/// 이동 경로는 Enemy.MoveToTarget 과 동일하게 X축 이동 → Y축 이동 순서로 그린다.
/// </summary>
public class WavePreview : MonoBehaviour
{
    public static WavePreview Instance { get; private set; }

    [Header("표시")]
    [SerializeField] float pathWidth = 0.08f;
    [SerializeField] Color pathColor = new(1f, 0.35f, 0.4f, 0.9f);
    [SerializeField] Color spawnMarkerColor = new(1f, 0.45f, 0.45f, 0.95f);
    [SerializeField] Color targetMarkerColor = new(1f, 0.85f, 0.35f, 0.9f);
    [SerializeField] float spawnMarkerSize = 0.42f;
    [SerializeField] float targetMarkerSize = 0.3f;

    [Header("동작")]
    [Tooltip("웨이브 시작 전 동선을 보여 주는 시간(초). 이 시간이 지나면 자동으로 웨이브가 시작된다. 0이면 대기 없이 바로 시작한다.")]
    [SerializeField] float leadInSeconds = 2f;

    [Tooltip("켜면 리드인 동안 게임을 멈춘다. 웨이브가 끊기지 않게 하려면 끈 채로 둔다.")]
    [SerializeField] bool pauseDuringPreview = false;

    [Tooltip("Space / Enter 로 남은 리드인 시간을 건너뛸 수 있게 한다. (필수 입력은 아니다)")]
    [SerializeField] bool allowSkip = true;

    [Tooltip("웨이브가 시작된 뒤에도 동선을 흐리게 남겨 둔다.")]
    [SerializeField] bool keepDuringWave = true;

    [SerializeField] float dimmedAlpha = 0.22f;

    readonly List<GameObject> markers = new();
    Canvas canvas;
    TMP_Text prompt;
    bool startRequested;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// 동선을 표시하고, 리드인 시간이 지나면 자동으로 반환한다. 플레이어 입력을 요구하지 않는다.
    public async UniTask ShowAndWaitAsync(int wave, int enemyCount, List<SpawnLane> lanes)
    {
        Clear();

        foreach (SpawnLane lane in lanes)
            CreateLaneVisual(lane);

        startRequested = false;

        if (leadInSeconds > 0f)
        {
            ShowPrompt(wave, enemyCount);

            float previousTimeScale = Time.timeScale;
            if (pauseDuringPreview) Time.timeScale = 0f;

            float remaining = leadInSeconds;

            await UniTask.WaitUntil(() =>
            {
                remaining -= Time.unscaledDeltaTime;

                if (remaining <= 0f) return true;
                if (startRequested) return true;

                if (allowSkip)
                {
                    Keyboard keyboard = Keyboard.current;
                    if (keyboard != null &&
                        (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                        return true;
                }

                UpdatePrompt(wave, enemyCount, remaining);
                return false;
            }, PlayerLoopTiming.Update);

            if (pauseDuringPreview) Time.timeScale = previousTimeScale;
        }

        HidePrompt();

        if (keepDuringWave) Dim();
        else Clear();
    }

    /// 남은 리드인 시간을 건너뛰고 즉시 웨이브를 시작한다.
    public void RequestStart() => startRequested = true;

    void CreateLaneVisual(SpawnLane lane)
    {
        Vector3 spawn = lane.SpawnWorld;
        Vector3 target = lane.TargetWorld;
        Vector3 corner = new(target.x, spawn.y, spawn.z);

        var pathObject = new GameObject("WavePreviewPath");
        pathObject.transform.SetParent(transform, false);

        var line = pathObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.material = RuntimeGfx.LineMaterial;
        line.widthMultiplier = pathWidth;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.startColor = line.endColor = pathColor;
        line.sortingOrder = 1;

        // Enemy 는 X축을 먼저 이동한 뒤 Y축을 이동한다.
        bool needsCorner = !Mathf.Approximately(spawn.y, target.y) && !Mathf.Approximately(spawn.x, target.x);
        if (needsCorner)
        {
            line.positionCount = 3;
            line.SetPositions(new[] { spawn, corner, target });
        }
        else
        {
            line.positionCount = 2;
            line.SetPositions(new[] { spawn, target });
        }

        markers.Add(pathObject);
        markers.Add(CreateMarker("WavePreviewSpawn", spawn, spawnMarkerSize, spawnMarkerColor, 2));
        markers.Add(CreateMarker("WavePreviewTarget", target, targetMarkerSize, targetMarkerColor, 2));
    }

    GameObject CreateMarker(string name, Vector3 position, float size, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * size;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeGfx.Circle;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        return go;
    }

    void ShowPrompt(int wave, int enemyCount)
    {
        canvas = RuntimeUI.CreateCanvas("WavePreviewCanvas", 400);

        prompt = RuntimeUI.CreateText(
            "Prompt", canvas.transform, "",
            44f, Color.black, TextAlignmentOptions.Center);

        UpdatePrompt(wave, enemyCount, leadInSeconds);

        var rect = (RectTransform)prompt.transform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 70f);
        rect.sizeDelta = new Vector2(1200f, 160f);
    }

    void UpdatePrompt(int wave, int enemyCount, float remaining)
    {
        if (prompt == null) return;

        string hint = "<size=70%><color=#2a4366>Space 를 눌러 시작</color></size>";

        prompt.text = $"웨이브 {wave + 1}  ·  적 {enemyCount}마리\n{hint}";
    }

    void HidePrompt()
    {
        if (canvas != null) Destroy(canvas.gameObject);
        canvas = null;
        prompt = null;
    }

    void Dim()
    {
        foreach (GameObject marker in markers)
        {
            if (marker == null) continue;

            if (marker.TryGetComponent(out LineRenderer line))
            {
                Color c = line.startColor;
                c.a = dimmedAlpha;
                line.startColor = line.endColor = c;
            }

            if (marker.TryGetComponent(out SpriteRenderer sprite))
            {
                Color c = sprite.color;
                c.a = dimmedAlpha;
                sprite.color = c;
            }
        }
    }

    public void Clear()
    {
        foreach (GameObject marker in markers)
            if (marker != null)
                Destroy(marker);

        markers.Clear();
        HidePrompt();
    }

    void Update()
    {
        if (prompt == null) return;
        if (Tutorial.Shown()) prompt.alpha = 0f;
        else prompt.alpha = 1f;
    }
}
