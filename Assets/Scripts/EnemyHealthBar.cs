using DG.Tweening;
using UnityEngine;

/// <summary>
/// 피격 순간에만 나타나는 적 개별 체력 바.
/// </summary>
public sealed class EnemyHealthBar : MonoBehaviour
{
    const float Width = 0.82f;
    const float Height = 0.075f;

    static Sprite sharedSprite;

    SpriteRenderer background;
    SpriteRenderer fill;
    Transform barRoot;
    Sequence fadeSequence;

    void Awake()
    {
        BuildIfNeeded();
        HideImmediately();
    }

    public void SetMaxHealth(float maxHealth)
    {
        BuildIfNeeded();
        SetFill(1f, true);
        HideImmediately();
    }

    public void ShowHealth(float normalizedHealth)
    {
        BuildIfNeeded();

        normalizedHealth = Mathf.Clamp01(normalizedHealth);
        barRoot.gameObject.SetActive(true);

        fadeSequence?.Kill();
        background.DOKill();
        fill.DOKill();
        barRoot.DOKill();

        SetOpacity(1f);
        AnimateFill(normalizedHealth);

        barRoot.localScale = Vector3.one * 0.78f;
        barRoot.DOScale(1.12f, 0.10f).SetEase(Ease.OutBack)
            .OnComplete(() => barRoot.DOScale(1f, 0.12f).SetEase(Ease.OutQuad));

        fadeSequence = DOTween.Sequence()
            .AppendInterval(1.0f)
            .Append(background.DOFade(0f, 0.22f))
            .Join(fill.DOFade(0f, 0.22f))
            .OnComplete(() =>
            {
                if (barRoot != null)
                    barRoot.gameObject.SetActive(false);
            });
    }

    void BuildIfNeeded()
    {
        if (barRoot != null) return;

        barRoot = new GameObject("HealthBar").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = new Vector3(0f, 0.52f, -0.05f);

        background = CreateRenderer("Background", new Color(0.08f, 0.09f, 0.13f, 0.82f), 110);
        background.transform.localScale = new Vector3(Width + 0.04f, Height + 0.04f, 1f);

        fill = CreateRenderer("Fill", new Color(0.35f, 1f, 0.48f, 1f), 111);
        SetFill(1f, true);
    }

    SpriteRenderer CreateRenderer(string objectName, Color color, int sortingOrder)
    {
        var child = new GameObject(objectName);
        child.transform.SetParent(barRoot, false);

        var renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    void AnimateFill(float normalized)
    {
        float targetWidth = Mathf.Max(0.01f, Width * normalized);
        float targetX = -(Width - targetWidth) * 0.5f;

        fill.transform.DOScaleX(targetWidth, 0.18f).SetEase(Ease.OutCubic);
        fill.transform.DOLocalMoveX(targetX, 0.18f).SetEase(Ease.OutCubic);
        fill.color = Color.Lerp(new Color(1f, 0.32f, 0.32f, 1f), new Color(0.35f, 1f, 0.48f, 1f), normalized);
    }

    void SetFill(float normalized, bool immediate)
    {
        float width = Mathf.Max(0.01f, Width * Mathf.Clamp01(normalized));
        fill.transform.localScale = new Vector3(width, Height, 1f);
        fill.transform.localPosition = new Vector3(-(Width - width) * 0.5f, 0f, 0f);
    }

    void SetOpacity(float alpha)
    {
        var backColor = background.color;
        backColor.a = 0.82f * alpha;
        background.color = backColor;

        var fillColor = fill.color;
        fillColor.a = alpha;
        fill.color = fillColor;
    }

    void HideImmediately()
    {
        if (barRoot != null)
            barRoot.gameObject.SetActive(false);
    }

    static Sprite GetSprite()
    {
        if (sharedSprite != null) return sharedSprite;

        sharedSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        return sharedSprite;
    }

    void OnDestroy()
    {
        fadeSequence?.Kill();
    }
}