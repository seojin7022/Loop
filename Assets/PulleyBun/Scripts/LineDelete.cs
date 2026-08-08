using UnityEngine;
using UnityEngine.InputSystem;

namespace PulleyBun
{
    public class LineDelete : MonoBehaviour
    {
        [SerializeField] InputAction rightClick;
        [SerializeField] InputAction mousePosition;
        [SerializeField] Collider2D collider;

        void Awake()
        {
            rightClick.Enable();
            mousePosition.Enable();
        }

        void OnRightClick()
        {
            var position = Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>());
            bool hit = collider.OverlapPoint(position);
            if (hit)
            {
                Sfx.MirrorRemoved(transform.position);
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (UIBlocker.IsBlocking) return;

            if (rightClick.WasPressedThisFrame())
            {
                OnRightClick();
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
