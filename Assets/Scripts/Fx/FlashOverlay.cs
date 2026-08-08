using UnityEngine;

/// <summary>
/// 적을 맞췄을 때 순간적으로 흰색 실루엣을 겹쳐 그리는 히트 플래시.
/// 본체 SpriteRenderer 의 머티리얼(라이팅 포함)은 건드리지 않고,
/// 같은 스프라이트를 Loop/SpriteFlash 셰이더로 한 장 더 그린다.
/// 셰이더를 찾지 못하면 본체 색을 잠깐 물들이는 방식으로 자동 폴백한다.
/// </summary>
[DisallowMultipleComponent]
public class FlashOverlay : MonoBehaviour
{
    const string ShaderName = "Loop/SpriteFlash";

    static Material sharedMaterial;
    static bool materialResolved;

    static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    SpriteRenderer source;
    SpriteRenderer overlay;
    MaterialPropertyBlock block;

    Color flashColor = Color.white;
    float duration = 0.09f;
    float age;
    bool playing;
    bool useFallback;
    Color originalColor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        sharedMaterial = null;
        materialResolved = false;
    }

    static Material SharedMaterial
    {
        get
        {
            if (!materialResolved)
            {
                materialResolved = true;

                Shader shader = Shader.Find(ShaderName);
                if (shader != null)
                {
                    sharedMaterial = new Material(shader) { name = "SpriteFlash (runtime)" };
                }
                else
                {
                    Debug.LogWarning(
                        $"[FlashOverlay] '{ShaderName}' 셰이더를 찾지 못했습니다. 색 틴트 방식으로 대체합니다.");
                }
            }

            return sharedMaterial;
        }
    }

    /// 대상 오브젝트에 히트 플래시를 재생한다. 컴포넌트가 없으면 자동으로 붙인다.
    public static void Play(GameObject target, Color color, float duration)
    {
        if (target == null) return;

        if (!target.TryGetComponent(out FlashOverlay flash))
            flash = target.AddComponent<FlashOverlay>();

        flash.Begin(color, duration);
    }

    void Begin(Color color, float duration)
    {
        if (source == null) source = GetComponent<SpriteRenderer>();
        if (source == null) return;

        flashColor = color;
        this.duration = Mathf.Max(0.01f, duration);
        age = 0f;
        playing = true;

        if (SharedMaterial != null)
        {
            EnsureOverlay();
            useFallback = false;
            overlay.enabled = true;
        }
        else
        {
            if (!useFallback) originalColor = source.color;
            useFallback = true;
        }

        Apply(0f);
    }

    void EnsureOverlay()
    {
        if (overlay != null) return;

        var go = new GameObject("HitFlash");
        go.transform.SetParent(transform, false);

        overlay = go.AddComponent<SpriteRenderer>();
        overlay.sharedMaterial = SharedMaterial;

        block = new MaterialPropertyBlock();
    }

    void SyncOverlay()
    {
        overlay.sprite = source.sprite;
        overlay.flipX = source.flipX;
        overlay.flipY = source.flipY;
        overlay.drawMode = source.drawMode;

        // size 는 Sliced/Tiled 일 때만 설정할 수 있다.
        if (source.drawMode != SpriteDrawMode.Simple)
            overlay.size = source.size;

        overlay.sortingLayerID = source.sortingLayerID;
        overlay.sortingOrder = source.sortingOrder + 1;
        overlay.maskInteraction = source.maskInteraction;
    }

    void Update()
    {
        if (!playing) return;

        age += Time.deltaTime;

        float t = age / duration;
        if (t >= 1f)
        {
            Stop();
            return;
        }

        Apply(t);
    }

    void Apply(float t)
    {
        // 처음엔 꽉 찬 흰색, 뒤로 갈수록 빠르게 사라진다.
        float strength = 1f - t * t;

        if (useFallback)
        {
            source.color = Color.Lerp(originalColor, flashColor, strength * 0.85f);
            return;
        }

        if (overlay == null) return;

        SyncOverlay();

        Color color = flashColor;
        color.a = flashColor.a * strength;

        overlay.GetPropertyBlock(block);
        block.SetColor(FlashColorId, color);
        block.SetFloat(FlashAmountId, 1f);
        overlay.SetPropertyBlock(block);
    }

    void Stop()
    {
        playing = false;

        if (useFallback)
        {
            if (source != null) source.color = originalColor;
            return;
        }

        if (overlay != null) overlay.enabled = false;
    }
}
