using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PulleyBun
{
    public class LineMaker : MonoBehaviour
    {
        [SerializeField] GameObject linePrefab;
        [SerializeField] GameObject linePreviewPrefab;
        [SerializeField] float minLength = 0.5f;
        [SerializeField] float maxLength = 5f;
        [SerializeField] InputAction drag;
        [SerializeField] InputAction mousePosition;
        [SerializeField] int maxLines = 5;

        [Header("설치 이펙트")]
        [SerializeField] Color placeColor = new(0.6f, 0.92f, 1f, 1f);
        [SerializeField] Color unavailablePlaceColor = new(0.6f, 0.2f, 0.2f, 1f);

        [Tooltip("거울 중심에서 퍼져 나가는 충격파 링의 최종 반지름")]
        [SerializeField] float placeRingRadius = 1.5f;
        [SerializeField] float placeRingDuration = 0.34f;

        [Tooltip("거울 선분을 따라 번쩍이는 펄스의 두께 배수")]
        [SerializeField] float placePulseWidth = 0.4f;
        [SerializeField] int placeSparkCount = 8;

        public int LineCount => lines.Count;

        public static LineMaker Instance;

        /// 현재 설치되어 있는 거울 목록.
        readonly List<GameObject> lines = new();
        public IReadOnlyList<GameObject> Lines => lines;

        /// 거울이 새로 설치되었을 때. 거울 부착 효과(오라·하수인) 설치에 사용한다.
        public static event Action<GameObject> LineCreated;

        /// 거울이 제거되었을 때. 부착물은 거울의 자식이므로 함께 사라진다.
        public static event Action<GameObject> LineRemoved;

        bool isDragging = false;
        Vector2 start;
        GameObject linePreview;

        void Awake()
        {
            drag.Enable();
            mousePosition.Enable();
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public int MaxLines =>
            RelicManager.Has(Relic.MoreMirror) ? maxLines * 2 : maxLines;

        public void OnAddLine(GameObject line)
        {
            if (line == null || lines.Contains(line)) return;

            lines.Add(line);
            LineCreated?.Invoke(line);
        }

        public void OnRemoveLine() => OnRemoveLine(null);

        public void OnRemoveLine(GameObject line)
        {
            if (line != null)
                lines.Remove(line);

            // 파괴되어 null이 된 항목 정리 (씬 전환·프레임 지연 대비)
            lines.RemoveAll(l => l == null);

            LineRemoved?.Invoke(line);
        }

        public bool CanPlaceLine()
        {
            return LineCount < MaxLines;
        }

        void OnClick(Vector2 position)
        {
            if (isDragging) return;
            isDragging = true;

            start = position;
            linePreview = Instantiate(linePreviewPrefab, start, Quaternion.identity);
        }

        float GetMaxLength()
        {
            return RelicManager.Has(Relic.BigMirror) ? maxLength * 2 : maxLength;
        }

        void OnRelease(Vector2 position)
        {
            if (!isDragging) return;
            isDragging = false;
            Destroy(linePreview);
            linePreview = null;

            var vector = position - start;
            if (vector.magnitude < minLength)
            {
                return;
            }
            if (vector.magnitude > GetMaxLength())
            {
                vector = vector.normalized * GetMaxLength();
            }
            if (!CanPlaceLine())
            {
                PlayUnavailablePlaceEffect(start, start + vector);
                return;
            }
            var line = Instantiate(linePrefab, start, Quaternion.identity);
            line.transform.position = start + vector * 0.5f;
            line.transform.right = vector.normalized;
            line.transform.localScale = new Vector3(vector.magnitude, 1f, 1f);
            OnAddLine(line);

            PlayPlaceEffect(start, start + vector);
        }

        /// 거울이 설치됐다는 것을 알리는 충격파 링 + 선분 펄스 + 잔불꽃.
        void PlayPlaceEffect(Vector2 a, Vector2 b)
        {
            Vector3 center = (a + b) * 0.5f;

            Fx.Ring(center, 0.15f, placeRingRadius, placeColor,
                width: 0.13f, duration: placeRingDuration);

            Fx.Segment(a, b, placeColor,
                width: placePulseWidth, duration: 0.22f);

            Fx.HitBurst(center, placeColor, placeSparkCount,
                speed: 4f, size: 0.12f, lifetime: 0.28f);

            Sfx.MirrorPlaced(center);
        }

        void PlayUnavailablePlaceEffect(Vector2 a, Vector2 b)
        {
            Vector3 center = (a + b) * 0.5f;

            Fx.Segment(a, b, unavailablePlaceColor,
                width: 0, duration: 0.22f);
        }

        void OnDrag(Vector2 position)
        {
            if (!isDragging || linePreview == null) return;

            var vector = position - start;
            if (vector.magnitude > GetMaxLength())
            {
                vector = vector.normalized * GetMaxLength();
            }
            linePreview.transform.position = start + vector * 0.5f;
            linePreview.transform.right = vector.normalized;
            linePreview.transform.localScale = new Vector3(vector.magnitude, 1f, 1f);
        }

        void Update()
        {
            // 특성 선택·게임 오버 등 전체 화면 UI가 떠 있으면 거울을 설치하지 않는다.
            if (UIBlocker.IsBlocking)
            {
                if (isDragging)
                {
                    isDragging = false;
                    if (linePreview != null) Destroy(linePreview);
                    linePreview = null;
                }
                return;
            }

            if (drag.WasPressedThisFrame())
            {
                OnClick(Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>()));
            }
            else if (drag.WasReleasedThisFrame())
            {
                OnRelease(Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>()));
            }
            else if (drag.IsPressed())
            {
                OnDrag(Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>()));
            }
        }
    }
}
