using UnityEngine;
using UnityEngine.InputSystem;

namespace PulleyBun
{
    public class LineDelete : MonoBehaviour
    {
        [SerializeField] InputAction rightClick;
        [SerializeField] InputAction mousePosition;
        [SerializeField] Collider2D collider;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Color highlightColor;
        Color originalColor;

        void Awake()
        {
            rightClick.Enable();
            mousePosition.Enable();
        }

        void Start()
        {
            originalColor = spriteRenderer.color;
        }

        void OnRightClick()
        {
            Sfx.MirrorRemoved(transform.position);
            Vector3 center = transform.position;

            Fx.Ring(center, 0.15f, 0.2f, highlightColor,
                width: 0.13f, duration: 0.35f);

            Fx.Segment(transform.position - transform.right * transform.localScale.x * 0.5f,
                transform.position + transform.right * transform.localScale.x * 0.5f, highlightColor,
                width: 0.35f, duration: 0.25f);

            Fx.HitBurst(center, highlightColor, 8,
                speed: 4f, size: 0.12f, lifetime: 0.28f);

            Tutorial.Trigger("RemoveMirror");

            Destroy(gameObject);
        }

        void Update()
        {
            if (UIBlocker.IsBlocking) return;

            var position = Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>());
            bool hit = collider.OverlapPoint(position);

            if (hit)
            {
                if (rightClick.WasPressedThisFrame())
                {
                    OnRightClick();
                }
                spriteRenderer.color = highlightColor;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }

        void OnDestroy()
        {
            // 거울에 딸린 오라·하수인은 자식 오브젝트이므로 이 오브젝트와 함께 제거된다.
            if (LineMaker.Instance != null)
                LineMaker.Instance.OnRemoveLine(gameObject);
        }
    }
}
