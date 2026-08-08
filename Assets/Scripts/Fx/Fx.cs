using System.Collections.Generic;
using PulleyBun;
using UnityEngine;

/// <summary>
/// 프리팹·에셋 없이 코드로 돌아가는 가벼운 이펙트 매니저.
/// 파티클 버스트, 충격파 링, 선분 펄스를 풀링해서 재사용한다.
/// 씬에 배치할 필요 없이 자동 생성된다.
/// </summary>
public class Fx : MonoBehaviour
{
    public static Fx Instance { get; private set; }

    [Header("풀 상한 (동시에 살아 있을 수 있는 개수)")]
    [SerializeField] int maxParticles = 200;
    [SerializeField] int maxLineEffects = 32;

    [Header("정렬 순서")]
    [SerializeField] int particleSortingOrder = 6;
    [SerializeField] int lineSortingOrder = 6;

    readonly Stack<FxParticle> particlePool = new();
    readonly Stack<FxLineEffect> linePool = new();

    int liveParticles;
    int liveLineEffects;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        var go = new GameObject("@Fx");
        go.AddComponent<Fx>();
    }

    public static Fx Ensure()
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

    // ---------------------------------------------------------------- 공개 API

    /// 한 지점에서 사방으로 튀는 짧은 파티클 버스트.
    public static void HitBurst(
        Vector3 position, Color color, int count = 8,
        float speed = 4.5f, float size = 0.16f, float lifetime = 0.26f,
        float speedVariance = 0.45f, Vector3 bias = default, float biasWeight = 0f)
    {
        Fx fx = Ensure();
        if (fx == null) return;

        for (int i = 0; i < count; i++)
        {
            FxParticle particle = fx.RentParticle();
            if (particle == null) return;

            float angle = Mathf.PI * 2f * (i + Random.value) / count;
            Vector3 direction = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

            if (biasWeight > 0f && bias.sqrMagnitude > 0.0001f)
                direction = Vector3.Slerp(direction, bias.normalized, Mathf.Clamp01(biasWeight)).normalized;

            float particleSpeed = speed * Random.Range(1f - speedVariance, 1f + speedVariance);

            particle.Play(
                RuntimeGfx.Square,
                position,
                direction * particleSpeed,
                color,
                size * Random.Range(0.7f, 1.3f),
                lifetime * Random.Range(0.75f, 1.25f),
                drag: 6f,
                spin: Random.Range(-360f, 360f),
                fx.particleSortingOrder);
        }
    }

    /// 한 점에서 퍼져 나가는 충격파 링.
    public static void Ring(
        Vector3 position, float startRadius, float endRadius, Color color,
        float width = 0.14f, float duration = 0.35f)
    {
        Fx fx = Ensure();
        if (fx == null) return;

        FxLineEffect effect = fx.RentLineEffect();
        if (effect == null) return;

        effect.PlayRing(position, startRadius, endRadius, color, width, duration, fx.lineSortingOrder);
    }

    /// 선분을 따라 번쩍였다 사라지는 펄스.
    public static void Segment(
        Vector3 a, Vector3 b, Color color, float width = 0.35f, float duration = 0.25f)
    {
        Fx fx = Ensure();
        if (fx == null) return;

        FxLineEffect effect = fx.RentLineEffect();
        if (effect == null) return;

        effect.PlaySegment(a, b, color, width, duration, fx.lineSortingOrder);
    }

    // ------------------------------------------------------------------- 풀링

    FxParticle RentParticle()
    {
        if (particlePool.Count > 0)
        {
            liveParticles++;
            return particlePool.Pop();
        }

        if (liveParticles >= maxParticles) return null;

        var go = new GameObject("FxParticle");
        go.transform.SetParent(transform, false);
        go.AddComponent<SpriteRenderer>();

        liveParticles++;
        return go.AddComponent<FxParticle>();
    }

    FxLineEffect RentLineEffect()
    {
        if (linePool.Count > 0)
        {
            liveLineEffects++;
            return linePool.Pop();
        }

        if (liveLineEffects >= maxLineEffects) return null;

        var go = new GameObject("FxLineEffect");
        go.transform.SetParent(transform, false);
        go.AddComponent<LineRenderer>();

        liveLineEffects++;
        return go.AddComponent<FxLineEffect>();
    }

    public static void Release(FxParticle particle)
    {
        if (particle == null) return;

        particle.gameObject.SetActive(false);

        if (Instance == null)
        {
            Destroy(particle.gameObject);
            return;
        }

        Instance.liveParticles--;
        Instance.particlePool.Push(particle);
    }

    public static void Release(FxLineEffect effect)
    {
        if (effect == null) return;

        effect.gameObject.SetActive(false);

        if (Instance == null)
        {
            Destroy(effect.gameObject);
            return;
        }

        Instance.liveLineEffects--;
        Instance.linePool.Push(effect);
    }
}
