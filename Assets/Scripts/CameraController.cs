using UnityEngine;
using DG.Tweening;
using R3;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] RoomManager roomManager;

    [Tooltip("방 가장자리와 화면 사이 여백 (월드 단위)")]
    [SerializeField] float padding = 1f;
    [SerializeField] float duration = 0.7f;
    [SerializeField] Ease ease = Ease.InOutCubic;

    Camera cam;
    bool fitted;

    void Awake()
    {
        cam = GetComponent<Camera>();

        EventBus.OnEvent("RoomsChanged")
                .Subscribe(_ => Fit())
                .AddTo(this);
    }

    void Fit()
    {
        if (roomManager.Rooms.Count == 0) return;

        Bounds bounds = roomManager.RoomsBounds();

        float size = Mathf.Max(
            bounds.extents.y + padding,
            (bounds.extents.x + padding) / cam.aspect);

        Vector3 position = new(bounds.center.x, bounds.center.y, transform.position.z);

        // 첫 프레임은 연출 없이 바로 맞춤
        float time = fitted ? duration : 0f;
        fitted = true;

        transform.DOKill();
        cam.DOKill();
        transform.DOMove(position, time).SetEase(ease);
        cam.DOOrthoSize(size, time).SetEase(ease);
    }
}
