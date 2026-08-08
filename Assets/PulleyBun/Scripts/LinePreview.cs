using UnityEngine;
using UnityEngine.InputSystem;

namespace PulleyBun
{
    public class LinePreview : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Color unavailableColor = new(0.6f, 0.2f, 0.2f, 0.5f);
        Color originalColor;

        void Start()
        {
            originalColor = spriteRenderer.color;
        }

        void Update()
        {
            if (!LineMaker.Instance.CanPlaceLine())
            {
                spriteRenderer.color = unavailableColor;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}
