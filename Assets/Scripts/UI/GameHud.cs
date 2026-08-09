using System.Text;
using PulleyBun;
using TMPro;
using UnityEngine;

/// <summary>
/// HP·스테이지·웨이브·남은 적·거울 수·보유 특성을 표시하는 간단한 HUD.
/// 프로토타입 확인용이며 씬 배치 없이 자동 생성된다.
/// </summary>
public class GameHud : MonoBehaviour
{
    public static GameHud Instance { get; private set; }

    Canvas canvas;
    TMP_Text label;
    readonly StringBuilder builder = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        if (WaveManager.Instance == null) return;   // 인게임 씬에서만 표시

        var go = new GameObject("@GameHud");
        go.AddComponent<GameHud>();
    }

    /// 아직 없으면 만들어서 반환한다. 인게임 씬(WaveManager 존재)에서만 생성된다.
    public static GameHud Ensure()
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

        canvas = RuntimeUI.CreateCanvas("GameHudCanvas", 300);

        label = RuntimeUI.CreateText(
            "Hud", canvas.transform, "",
            30f, Color.black, TextAlignmentOptions.TopLeft);

        var rect = (RectTransform)label.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(36f, -30f);
        rect.sizeDelta = new Vector2(720f, 260f);
    }

    void OnDestroy()
    {
        if (canvas != null) Destroy(canvas.gameObject);
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        WaveManager wave = WaveManager.Instance;
        if (label == null || wave == null) return;

        builder.Clear();
        builder.Append("HP ").Append(Mathf.Max(0, wave.Hp)).Append(" / ").Append(wave.MaxHp)
            .Append("   거울 ").Append(Mathf.Max(0, LineMaker.Instance.LineCount)).Append(" / ").Append(LineMaker.Instance.MaxLines)
            .Append("   스테이지 ").Append(WaveManager.Instance.CurrentStage);

        RelicManager relics = RelicManager.Instance;
        if (relics != null && relics.Relics.Count > 0)
        {
            builder.Append("\n특성: ");
            for (int i = 0; i < relics.Relics.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(RelicDatabase.NameOf(relics.Relics[i]));
            }
        }

        label.text = builder.ToString();
    }
}
