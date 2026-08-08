using UnityEngine;

namespace PulleyBun
{
    /// <summary>
    /// 거울 하수인 소환 특성(Relic.MirrorTurret).
    /// 설치된 각 거울에 하수인 1기를 배치하고, 사거리 안의 가장 가까운 적에게 주기적으로 투사체를 발사한다.
    /// 거울이 삭제되면 하수인도 함께 사라진다. 활성 하수인 수는 RelicManager.MaxActiveMinions 로 제한된다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MirrorMinion : MonoBehaviour
    {
        // 기획서 권장 수치는 픽셀 기준(사거리 250, 거울 최대 길이 170)이다.
        // 이 프로젝트의 거울 최대 길이는 월드 5 유닛이므로 250 * (5 / 170) ≈ 7.35 로 환산했다.
        [SerializeField] float range = 7.35f;
        [SerializeField] float fireInterval = 1f;
        [SerializeField] float damage = 1f;

        [Header("Projectile")]
        [SerializeField] float projectileSpeed = 9f;
        [SerializeField] float projectileLifetime = 3f;
        [SerializeField] float projectileHitRadius = 0.28f;
        [SerializeField] float projectileSize = 0.22f;
        [SerializeField] Color projectileColor = new(1f, 0.85f, 0.35f, 1f);

        [Header("Visual")]
        [SerializeField] float bodySize = 0.32f;
        [SerializeField] float bodyOffset = 0.35f;
        [SerializeField] Color bodyColor = new(1f, 0.78f, 0.3f, 1f);

        /// 현재 살아있는 하수인 수. MirrorAttachments 가 최대치 판단에 사용한다.
        public static int ActiveCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => ActiveCount = 0;

        float fireTimer;
        Transform body;

        void OnEnable() => ActiveCount++;
        void OnDisable() => ActiveCount--;

        void OnDestroy()
        {
            // 특성이 제거되어 컴포넌트만 떼어낼 때, 하수인 표시도 함께 정리한다.
            if (body != null) Destroy(body.gameObject);
        }

        void Start()
        {
            CreateBody();
            // 설치 직후 즉시 발사되지 않도록 살짝 지연을 준다.
            fireTimer = fireInterval * 0.5f;
        }

        void CreateBody()
        {
            var go = new GameObject("Minion");
            go.transform.SetParent(transform, false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeGfx.Circle;
            renderer.color = bodyColor;
            renderer.sortingOrder = 4;

            body = go.transform;
            ApplyBodyTransform();
        }

        /// 거울은 localScale.x 를 길이로 쓰기 때문에, 자식은 역스케일로 왜곡을 보정한다.
        void ApplyBodyTransform()
        {
            if (body == null) return;

            Vector3 parentScale = transform.lossyScale;
            float sx = Mathf.Approximately(parentScale.x, 0f) ? 1f : parentScale.x;
            float sy = Mathf.Approximately(parentScale.y, 0f) ? 1f : parentScale.y;

            body.localScale = new Vector3(bodySize / sx, bodySize / sy, 1f);
            body.localPosition = new Vector3(0f, bodyOffset / sy, 0f);
        }

        void Update()
        {
            if (transform.hasChanged)
            {
                ApplyBodyTransform();
                transform.hasChanged = false;
            }

            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f) return;

            Enemy target = FindNearestEnemy();
            if (target == null) return;

            fireTimer = fireInterval;

            Vector3 origin = body != null ? body.position : transform.position;
            MinionProjectile.Spawn(
                origin, target, projectileSpeed, damage,
                projectileLifetime, projectileHitRadius, projectileSize, projectileColor);
        }

        Enemy FindNearestEnemy()
        {
            Enemy nearest = null;
            float nearestDistance = range;
            Vector2 origin = body != null ? (Vector2)body.position : (Vector2)transform.position;

            for (int i = 0; i < Enemy.All.Count; i++)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || !enemy.IsAlive) continue;

                float distance = Vector2.Distance(origin, enemy.transform.position);
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}
