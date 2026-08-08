using UnityEngine;

namespace PulleyBun
{
    /// <summary>
    /// 거울 오라 특성(Relic.MirrorSplash).
    /// 설치된 거울 주변 일정 범위의 적에게 초당 1 피해를 준다.
    /// 여러 거울의 오라가 같은 적에게 중첩되지 않도록, 피해 판정은 Enemy 쪽 쿨다운으로 제어한다.
    /// 거울이 삭제되면 이 컴포넌트도 함께 사라진다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MirrorAura : MonoBehaviour
    {
        [SerializeField] float radius = 1.6f;
        [SerializeField] float damage = 1f;
        [SerializeField] float tickInterval = 1f;
        [SerializeField] Color color = new(0.45f, 0.85f, 1f, 0.55f);

        LineRenderer outline;

        public float Radius
        {
            get => radius;
            set { radius = value; dirty = true; }
        }

        bool dirty = true;

        void Start()
        {
            CreateOutline();
        }

        void OnDestroy()
        {
            // 특성이 제거되어 컴포넌트만 떼어낼 때, 시각 표시도 함께 정리한다.
            if (outline != null) Destroy(outline.gameObject);
        }

        void CreateOutline()
        {
            var go = new GameObject("AuraOutline");
            go.transform.SetParent(transform, false);

            outline = go.AddComponent<LineRenderer>();
            outline.useWorldSpace = true;
            outline.loop = false;
            outline.widthMultiplier = 0.06f;
            outline.material = RuntimeGfx.LineMaterial;
            outline.startColor = outline.endColor = color;
            outline.textureMode = LineTextureMode.Stretch;
            outline.numCapVertices = 2;
            outline.sortingOrder = -1;
        }

        void GetSegment(out Vector2 a, out Vector2 b)
        {
            // 거울은 localScale.x 를 길이로 사용하고, transform.right 방향으로 뻗는다.
            float halfLength = transform.lossyScale.x * 0.5f;
            Vector2 center = transform.position;
            Vector2 dir = transform.right;

            a = center - dir * halfLength;
            b = center + dir * halfLength;
        }

        void Update()
        {
            GetSegment(out Vector2 a, out Vector2 b);

            if (outline != null && (dirty || transform.hasChanged))
            {
                Vector3[] points = RuntimeGfx.CapsuleOutline(a, b, radius);
                outline.positionCount = points.Length;
                outline.SetPositions(points);
                transform.hasChanged = false;
                dirty = false;
            }

            // 뒤에서 앞으로 순회: 피해로 적이 제거되며 목록이 줄어들 수 있다.
            for (int i = Enemy.All.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || !enemy.IsAlive) continue;

                if (RuntimeGfx.DistanceToSegment(enemy.transform.position, a, b) <= radius)
                    enemy.TryAuraDamage(damage, tickInterval);
            }
        }
    }
}
