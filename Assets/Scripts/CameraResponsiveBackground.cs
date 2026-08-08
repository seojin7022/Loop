using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class CameraResponsiveBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float sortingOrder = -1000f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (targetCamera == null)
            targetCamera = Camera.main;
        ResizeToCamera();
    }

    private void LateUpdate()
    {
        ResizeToCamera();
    }

    private void ResizeToCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        var spriteSize = spriteRenderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        float viewHeight;
        float viewWidth;
        if (targetCamera.orthographic)
        {
            viewHeight = targetCamera.orthographicSize * 2f;
            viewWidth = viewHeight * targetCamera.aspect;
        }
        else
        {
            var distance = Mathf.Clamp(targetCamera.farClipPlane - 1f, 1f, 1000f);
            viewHeight = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            viewWidth = viewHeight * targetCamera.aspect;
        }

        var depth = Mathf.Clamp(targetCamera.farClipPlane - 1f, 1f, 1000f);
        transform.position = targetCamera.transform.position + targetCamera.transform.forward * depth;
        transform.rotation = targetCamera.transform.rotation;
        transform.localScale = new Vector3(viewWidth / spriteSize.x, viewHeight / spriteSize.y, 1f);
        spriteRenderer.sortingOrder = Mathf.RoundToInt(sortingOrder);
    }
}