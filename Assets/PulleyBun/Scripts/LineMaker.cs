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

        bool isDragging = false;
        Vector2 start;
        GameObject linePreview;

        void Awake()
        {
            drag.Enable();
            mousePosition.Enable();
        }

        void OnClick(Vector2 position)
        {
            if (isDragging) return;
            isDragging = true;

            start = position;
            linePreview = Instantiate(linePreviewPrefab, start, Quaternion.identity);
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
            if (vector.magnitude > maxLength)
            {
                vector = vector.normalized * maxLength;
            }
            var line = Instantiate(linePrefab, start, Quaternion.identity);
            line.transform.position = start + vector * 0.5f;
            line.transform.right = vector.normalized;
            line.transform.localScale = new Vector3(vector.magnitude, 1f, 1f);
        }

        void OnDrag(Vector2 position)
        {
            if (!isDragging || linePreview == null) return;

            var vector = position - start;
            if (vector.magnitude > maxLength)
            {
                vector = vector.normalized * maxLength;
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
