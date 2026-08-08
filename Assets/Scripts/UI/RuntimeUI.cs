using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 프리팹 없이 코드로 UI를 구성하기 위한 헬퍼.
/// 특성 선택·게임 오버·웨이브 안내처럼 씬 배치 없이 동작해야 하는 UI에 사용한다.
/// </summary>
public static class RuntimeUI
{
    static TMP_FontAsset cachedFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => cachedFont = null;

    /// 씬이 바뀌면 이전 씬에서 찾은 폰트가 파괴됐을 수 있으므로 캐시를 비운다.
    public static void ClearFontCache() => cachedFont = null;

    /// <summary>
    /// 한글이 표시되는 폰트를 찾는다.
    /// TMP 기본 폰트(LiberationSans)는 한글 글리프가 없으므로,
    /// 씬에서 이미 쓰고 있는 폰트를 우선 사용한다.
    /// </summary>
    public static TMP_FontAsset ResolveFont()
    {
        if (cachedFont != null) return cachedFont;

        TMP_FontAsset fallback = TMP_Settings.defaultFontAsset;

        // 1) 씬에 배치된 TMP 텍스트가 쓰는 폰트 (튜토리얼 대사 등)
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in texts)
        {
            if (text.font != null && text.font != fallback)
            {
                cachedFont = text.font;
                return cachedFont;
            }
        }

        // 2) 이미 로드된 폰트 에셋 중 기본 폰트가 아닌 것
        TMP_FontAsset[] loaded = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in loaded)
        {
            if (font != null && font != fallback && !font.name.Contains("Fallback"))
            {
                cachedFont = font;
                return cachedFont;
            }
        }

        cachedFont = fallback;
        return cachedFont;
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    public static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static Image CreatePanel(string name, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    public static Image CreateFullScreen(string name, Transform parent, Color color)
    {
        Image image = CreatePanel(name, parent, color);
        var rect = (RectTransform)image.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    public static TMP_Text CreateText(
        string name, Transform parent, string content,
        float fontSize, Color color, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(name, parent);

        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset font = ResolveFont();
        if (font != null) text.font = font;

        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    public static Button CreateButton(
        string name, Transform parent, Color normal, Color highlighted)
    {
        Image image = CreatePanel(name, parent, normal);
        var button = image.gameObject.AddComponent<Button>();

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = highlighted;
        colors.pressedColor = highlighted * 0.85f;
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.targetGraphic = image;

        return button;
    }

    public static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
