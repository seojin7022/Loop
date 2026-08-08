using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour
{
    /// 살아있는 적 목록. 거울 오라·하수인이 물리 쿼리 없이 대상을 찾는 데 사용한다.
    public static readonly List<Enemy> All = new();

    // Enter Play Mode 시 도메인 리로드를 끈 경우에도 목록이 남지 않도록 초기화한다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    [Header("타격 피드백")]
    [SerializeField] Color hitFlashColor = Color.white;
    [SerializeField] float hitFlashDuration = 0.09f;
    [SerializeField] Color hitParticleColor = new(1f, 0.92f, 0.6f, 1f);
    [SerializeField] Color deathParticleColor = new(1f, 0.55f, 0.45f, 1f);
    [SerializeField] Color damageParticleColor = new(0f, 0.45f, 1f, 0.5f);
    [SerializeField] int hitParticleCount = 6;
    [SerializeField] int deathParticleCount = 14;
    [SerializeField] int damageParticleCount = 17;

    [Tooltip("처치 시 터지는 링의 최종 반지름")]
    [SerializeField] float deathRingRadius = 0.9f;
    [SerializeField] float damageRingRadius = 1.4f;

    [SerializeField] Transform spriteTransform;

    Vector3 spawn, target;
    float speed, hp, maxHp;
    EnemyHealthBar healthBar;
    bool isDestroyed;

    /// 오라 중첩 방지용. 여러 거울의 오라가 있어도 이 간격보다 자주 피해를 받지 않는다.
    float lastAuraDamageTime = float.NegativeInfinity;

    public bool IsAlive => !isDestroyed;

    void Awake()
    {
        healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar == null)
            healthBar = gameObject.AddComponent<EnemyHealthBar>();
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public void SetEnemy(Vector3 spawnPos, Vector3 targetPos, float speed, float hp)
    {
        spawn = spawnPos;
        target = targetPos;
        this.speed = speed;
        maxHp = Mathf.Max(0.01f, hp);
        this.hp = maxHp;
        healthBar.SetMaxHealth(maxHp);
        spriteTransform.localScale = new Vector3(0f, 0f, 1f);

        MoveToTarget().Forget();
        SpawnScaleUp().Forget();
    }

    public async UniTask MoveToTarget()
    {
        var token = this.GetCancellationTokenOnDestroy();

        float xTime = Mathf.Abs(target.x - spawn.x) / speed;
        await transform.DOMoveX(target.x, xTime).SetEase(Ease.Linear).WithCancellation(token);

        float yTime = Mathf.Abs(target.y - spawn.y) / speed;
        await transform.DOMoveY(target.y, yTime).SetEase(Ease.Linear).WithCancellation(token);

        if (isDestroyed) return;

        isDestroyed = true;
        EventBus.Publish("PlayerDamage");
        PlayDamageFeedback();
        Destroy(gameObject);
    }

    public async UniTask SpawnScaleUp()
    {
        await spriteTransform.DOScale(1f, 0.5f).SetEase(Ease.OutExpo);
    }

    /// 탄환·하수인·오라 등 모든 피해 진입점.
    public void TakeDamage(float amount) => TakeDamage(amount, transform.position);

    /// <param name="hitPoint">파티클이 튈 방향을 정하는 데 쓰는 실제 타격 지점.</param>
    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (isDestroyed) return;

        hp = Mathf.Max(0f, hp - amount);
        healthBar.ShowHealth(hp / maxHp);

        if (hp > 0f)
        {
            PlayHitFeedback(hitPoint);
            return;
        }

        isDestroyed = true;
        PlayDeathFeedback();

        EventBus.Publish("EnemyDie");
        Destroy(gameObject);
    }

    /// 살아남았을 때: 흰색 플래시 + 작은 파티클.
    void PlayHitFeedback(Vector3 hitPoint)
    {
        FlashOverlay.Play(gameObject, hitFlashColor, hitFlashDuration);

        // 맞은 쪽 반대 방향으로 살짝 쏠리게 튄다.
        Vector3 bias = transform.position - hitPoint;

        Fx.HitBurst(
            hitPoint, hitParticleColor, hitParticleCount,
            speed: 3.6f, size: 0.13f, lifetime: 0.22f,
            bias: bias, biasWeight: 0.35f);

        Sfx.EnemyHit(transform.position);
    }

    /// 처치됐을 때: 큰 파티클 + 충격파 링. (오브젝트가 사라지므로 플래시는 생략)
    void PlayDeathFeedback()
    {
        Vector3 position = transform.position;

        Fx.HitBurst(
            position, deathParticleColor, deathParticleCount,
            speed: 5.5f, size: 0.17f, lifetime: 0.32f);

        Fx.Ring(position, 0.1f, deathRingRadius, deathParticleColor, width: 0.12f, duration: 0.28f);

        Sfx.EnemyDie(position);
    }

    void PlayDamageFeedback()
    {
        Vector3 position = transform.position;

        Fx.HitBurst(
            position, damageParticleColor, damageParticleCount,
            speed: 5.5f, size: 0.17f, lifetime: 0.32f);

        Fx.Ring(position, 0.1f, damageRingRadius, damageParticleColor, width: 0.12f, duration: 0.28f);

        // TODO: SFX
    }

    /// 거울 오라 전용 피해. interval 안에 이미 오라 피해를 받았다면 무시해 중첩을 막는다.
    public bool TryAuraDamage(float amount, float interval)
    {
        if (isDestroyed) return false;
        if (Time.time - lastAuraDamageTime < interval) return false;

        lastAuraDamageTime = Time.time;
        TakeDamage(amount);
        return true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        if (collision.gameObject.TryGetComponent(out PulleyBun.Ball ball))
        {
            TakeDamage(ball.Damage, ball.transform.position);
        }
    }
}
