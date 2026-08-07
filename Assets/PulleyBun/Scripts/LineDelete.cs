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
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (rightClick.WasPressedThisFrame())
            {
                OnRightClick();
            }
        }

        void OnDestroy()
        {
            LineMaker.Instance.OnRemoveLine();
        }
    }
}
