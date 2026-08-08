using System.Collections.Generic;
using UnityEngine;

namespace PulleyBun
{
    /// <summary>
    /// 탄환의 반사 예상 궤적을 노란 점선으로 그린다.
    /// 번식(Duplicate) 특성을 보유하면 첫 반사 지점에서 두 갈래로 갈라지는 경로까지 함께 표시한다.
    /// </summary>
    public class Preview : MonoBehaviour
    {
        public static readonly float RayMargin = 0.01f;

        [SerializeField] float predictDepth = 60f;
        [SerializeField] float predictLength = 100f;

        [Tooltip("메인 경로를 그릴 LineRenderer. 분기 경로는 런타임에 복제해서 만든다.")]
        [SerializeField] LineRenderer lineRenderer;

        [Header("표시")]
        [SerializeField] Color color = new(1f, 0.85f, 0.15f, 0.85f);
        [SerializeField] float width = 0.05f;

        [Tooltip("공(3)보다 낮게 두어야 예상선이 공을 가리지 않는다.")]
        [SerializeField] int sortingOrder = 1;

        [SerializeField] bool dashed = true;

        [Tooltip("점선 한 주기의 길이 (월드 단위). 작을수록 촘촘해진다.")]
        [SerializeField] float dashLength = 0.3f;

        [Header("번식 특성")]
        [Tooltip("분열 각도. 부모의 BallMaker 에서 실제 탄환 값을 찾으면 그 값으로 덮어쓴다.")]
        [SerializeField] float duplicateAngle = 10f;

        [Tooltip("갈라진 두 경로의 투명도 배수")]
        [Range(0f, 1f)]
        [SerializeField] float branchAlpha = 0.7f;

        int layerMask;

        LineRenderer branchLeft, branchRight;

        readonly List<Vector3> trunk = new();
        readonly List<Vector3> left = new();
        readonly List<Vector3> right = new();

        void Awake()
        {
            layerMask = LayerMask.GetMask("Line") | LayerMask.GetMask("LinePreview");

            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            ApplyStyle(lineRenderer, color);

            // 실제 탄환의 분열 각도를 그대로 따라가서, 값이 어긋나 예상선이 거짓말하는 걸 막는다.
            BallMaker maker = GetComponentInParent<BallMaker>();
            if (maker != null && maker.BallPrefab != null)
                duplicateAngle = maker.BallPrefab.DuplicateAngle;
        }

        void ApplyStyle(LineRenderer line, Color lineColor)
        {
            if (line == null) return;

            line.useWorldSpace = true;
            line.widthMultiplier = width;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            line.sortingOrder = sortingOrder;
            line.startColor = line.endColor = lineColor;

            if (dashed)
            {
                // sharedMaterial 로 넣어야 LineRenderer 마다 머티리얼 사본이 생기지 않는다.
                // 색은 정점 색으로, 점선 밀도는 textureScale 로 각자 조절한다.
                line.sharedMaterial = RuntimeGfx.DashLineMaterial;
                line.textureMode = LineTextureMode.Tile;
                line.textureScale = new Vector2(1f / Mathf.Max(0.01f, dashLength), 1f);
            }
        }

        LineRenderer CreateBranch(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            Color branchColor = color;
            branchColor.a *= branchAlpha;

            var line = go.AddComponent<LineRenderer>();
            ApplyStyle(line, branchColor);

            return line;
        }

        void Update()
        {
            if (lineRenderer == null) return;

            bool duplicate = RelicManager.Has(Relic.Duplicate);
            int depth = Mathf.Max(1, (int)predictDepth);

            // 번식이 있으면 첫 반사까지만 그리고 거기서 두 갈래로 나눈다.
            bool bounced = Trace(
                transform.position, transform.right,
                duplicate ? 1 : depth,
                trunk, out Vector3 splitPosition, out Vector3 splitDirection);

            Draw(lineRenderer, trunk);

            bool showBranches = duplicate && bounced;

            if (showBranches)
            {
                if (branchLeft == null) branchLeft = CreateBranch("PreviewBranchLeft");
                if (branchRight == null) branchRight = CreateBranch("PreviewBranchRight");

                Vector3 leftDirection = Quaternion.Euler(0f, 0f, duplicateAngle) * splitDirection;
                Vector3 rightDirection = Quaternion.Euler(0f, 0f, -duplicateAngle) * splitDirection;

                Trace(splitPosition, leftDirection, depth, left, out _, out _);
                Trace(splitPosition, rightDirection, depth, right, out _, out _);

                Draw(branchLeft, left);
                Draw(branchRight, right);
            }

            SetBranchVisible(branchLeft, showBranches);
            SetBranchVisible(branchRight, showBranches);
        }

        static void SetBranchVisible(LineRenderer line, bool visible)
        {
            if (line != null && line.enabled != visible) line.enabled = visible;
        }

        /// <summary>
        /// 반사 경로를 추적해 points 를 채운다.
        /// </summary>
        /// <returns>maxBounces 안에서 실제로 반사가 일어났는지 여부</returns>
        bool Trace(
            Vector3 origin, Vector3 direction, int maxBounces,
            List<Vector3> points, out Vector3 endPosition, out Vector3 endDirection)
        {
            points.Clear();
            points.Add(origin);

            Vector3 position = origin;
            endPosition = origin;
            endDirection = direction;

            bool bounced = false;

            for (int i = 0; i < maxBounces; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(position, direction, predictLength, layerMask);

                if (!hit)
                {
                    points.Add(position + direction * predictLength);
                    return bounced;
                }

                points.Add(hit.point);

                direction = Vector2.Reflect(direction, hit.normal);
                position = (Vector3)hit.point + direction * RayMargin;

                endPosition = position;
                endDirection = direction;
                bounced = true;
            }

            return bounced;
        }

        static void Draw(LineRenderer line, List<Vector3> points)
        {
            if (line == null) return;

            line.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                line.SetPosition(i, points[i]);
        }
    }
}
