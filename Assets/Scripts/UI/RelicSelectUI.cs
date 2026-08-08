using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PulleyBun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 시작 전 특성 3택 1 선택 화면.
/// 선택지는 RelicManager.RollChoices 의 풀 규칙을 따른다.
/// 씬 배치 없이 자동 생성되며, 선택하는 동안 게임을 정지한다.
/// </summary>
public class RelicSelectUI : MonoBehaviour
{
    public static RelicSelectUI Instance { get; private set; }

    public static bool IsOpen { get; private set; }

    const int ChoiceCount = 3;

    Canvas canvas;
    Relic? picked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        var go = new GameObject("@RelicSelectUI");
        go.AddComponent<RelicSelectUI>();
    }

    /// 아직 생성되지 않았다면 만들어서 반환한다. (스크립트 실행 순서에 의존하지 않기 위함)
    public static RelicSelectUI Ensure()
    {
        if (Instance == null) Bootstrap();
        return Instance;
    }

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

    /// 선택이 끝날 때까지 대기한다. 제시할 특성이 없으면 즉시 반환한다.
    public async UniTask ShowAndWaitAsync()
    {
        RelicManager manager = RelicManager.Instance;
        if (manager == null) return;

        List<Relic> choices = manager.RollChoices(ChoiceCount);
        if (choices.Count == 0) return;

        picked = null;
        IsOpen = true;
        UIBlocker.Push();

        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Build(choices);

        await UniTask.WaitUntil(() => picked.HasValue, PlayerLoopTiming.Update);

        manager.Choose(picked.Value);

        Teardown();
        Time.timeScale = previousTimeScale;
        UIBlocker.Pop();
        IsOpen = false;
    }

    void Build(List<Relic> choices)
    {
        canvas = RuntimeUI.CreateCanvas("RelicSelectCanvas", 500);

        RuntimeUI.CreateFullScreen("Dim", canvas.transform, new Color(0f, 0f, 0f, 0.72f));

        TMP_Text title = RuntimeUI.CreateText(
            "Title", canvas.transform, "특성 선택",
            72f, Color.white, TextAlignmentOptions.Center);
        var titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -110f);
        titleRect.sizeDelta = new Vector2(900f, 100f);

        TMP_Text subtitle = RuntimeUI.CreateText(
            "Subtitle", canvas.transform, "하나를 골라 이번 런 동안 유지한다",
            30f, new Color(1f, 1f, 1f, 0.65f), TextAlignmentOptions.Center);
        var subtitleRect = (RectTransform)subtitle.transform;
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -210f);
        subtitleRect.sizeDelta = new Vector2(900f, 50f);

        const float cardWidth = 460f;
        const float cardHeight = 560f;
        const float gap = 40f;

        float totalWidth = choices.Count * cardWidth + (choices.Count - 1) * gap;
        float startX = -totalWidth * 0.5f + cardWidth * 0.5f;

        for (int i = 0; i < choices.Count; i++)
        {
            Relic relic = choices[i];
            RelicInfo info = RelicDatabase.Get(relic);

            Button button = RuntimeUI.CreateButton(
                $"Card_{relic}", canvas.transform,
                new Color(0.13f, 0.15f, 0.22f, 0.97f),
                new Color(0.85f, 0.92f, 1f, 1f));

            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);
            rect.anchoredPosition = new Vector2(startX + i * (cardWidth + gap), -60f);

            Relic captured = relic;
            button.onClick.AddListener(() => picked = captured);

            TMP_Text category = RuntimeUI.CreateText(
                "Category", rect, info?.CategoryLabel ?? "",
                26f, new Color(0.55f, 0.75f, 1f, 1f), TextAlignmentOptions.Center);
            PlaceFromTop((RectTransform)category.transform, 36f, 44f, cardWidth - 48f);

            TMP_Text name = RuntimeUI.CreateText(
                "Name", rect, info?.DisplayName ?? relic.ToString(),
                52f, Color.white, TextAlignmentOptions.Center);
            PlaceFromTop((RectTransform)name.transform, 88f, 76f, cardWidth - 48f);

            TMP_Text description = RuntimeUI.CreateText(
                "Description", rect, info?.Description ?? "",
                30f, new Color(0.88f, 0.9f, 0.95f, 1f), TextAlignmentOptions.Top);
            PlaceFromTop((RectTransform)description.transform, 190f, cardHeight - 250f, cardWidth - 64f);
        }
    }

    /// 부모의 위쪽 가장자리에서 offsetFromTop 만큼 내려온 위치에 배치한다.
    static void PlaceFromTop(RectTransform rect, float offsetFromTop, float height, float width)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(0f, -offsetFromTop);
    }

    void Teardown()
    {
        if (canvas != null) Destroy(canvas.gameObject);
        canvas = null;
    }
}
