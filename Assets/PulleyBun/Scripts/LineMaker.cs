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
        public int LineCount { get; private set; } = 0;

        public static LineMaker Instance;

        bool isDragging = false;
        Vector2 start;
        GameObject linePreview;

        void Awake()
        {
            drag.Enable();
            mousePosition.Enable();
            Instance = this;
        }

        public void OnAddLine()
        {
            LineCount++;
        }
        public void OnRemoveLine()
        {
            LineCount--;
        }

        void OnClick(Vector2 position)
        {
            if (isDragging) return;
            int realMaxLines = RelicManager.Instance.HasRelic(Relic.MoreMirror) ? maxLines * 2 : maxLines;
            if (LineCount >= realMaxLines) return;
            isDragging = true;

            start = position;
            linePreview = Instantiate(linePreviewPrefab, start, Quaternion.identity);
        }

        float GetMaxLength()
        {
            return RelicManager.Instance.HasRelic(Relic.BigMirror) ? maxLength * 2 : maxLength;
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
            var line = Instantiate(linePrefab, start, Quaternion.identity);
            line.transform.position = start + vector * 0.5f;
            line.transform.right = vector.normalized;
            line.transform.localScale = new Vector3(vector.magnitude, 1f, 1f);
            OnAddLine();
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
