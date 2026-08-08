using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 위치를 빛의 각도로 삼아 반사상을 기울인다.
/// 거울선(원점을 지나는 수평선)은 움직이지 않는다. 반사상은 거울선에 붙은 채
/// 깊이에 비례해 옆으로 밀리기만 한다. x' = x + k·y, y' = -y (k = tan 빛각도)
///
/// RectTransform 은 전단(shear)을 직접 못 하므로 이 행렬을 회전·비균등스케일·회전
/// 세 단계로 나눠서 부모-자식으로 얹는다. (2x2 SVD: M = R(phi)·diag(sx,sy)·R(theta))
/// </summary>
public class TitleMirror : MonoBehaviour
{
    /// 가운데 단계: 비균등 스케일. 상하 반전도 여기서 나온다.
    public RectTransform scale;

    /// 안쪽 단계: 스케일 전에 걸리는 회전.
    public RectTransform spin;

    /// 화면 좌우 끝에서의 빛 기울기(도).
    public float maxLean = 28f;

    /// 클수록 마우스를 빠르게 따라온다.
    public float follow = 8f;

    float lean;

    void Update()
    {
        float target = maxLean * NormalizedMouseX();
        lean = Mathf.Lerp(lean, target, 1f - Mathf.Exp(-follow * Time.unscaledDeltaTime));

        Apply(Mathf.Tan(lean * Mathf.Deg2Rad));
    }

    void Apply(float k)
    {
        // M = [[1, k], [0, -1]]
        float e = 0f;               // (a + d) / 2
        float f = 1f;               // (a - d) / 2
        float g = k * 0.5f;         // (c + b) / 2
        float h = -k * 0.5f;        // (c - b) / 2

        float q = Mathf.Sqrt(e * e + h * h);
        float r = Mathf.Sqrt(f * f + g * g);
        float a1 = Mathf.Atan2(g, f);
        float a2 = Mathf.Atan2(h, e);

        // k=0 근처에서는 특이값이 같아져 세 단계가 각각 튀지만, 합쳐진 변환은 이어진다.
        transform.localRotation = Quaternion.Euler(0f, 0f, (a2 + a1) * 0.5f * Mathf.Rad2Deg);
        scale.localScale = new Vector3(q + r, q - r, 1f);
        spin.localRotation = Quaternion.Euler(0f, 0f, (a2 - a1) * 0.5f * Mathf.Rad2Deg);
    }

    /// 화면 왼쪽 끝 -1, 오른쪽 끝 +1.
    static float NormalizedMouseX()
    {
        if (Mouse.current == null) return 0f;

        float x = Mouse.current.position.ReadValue().x;
        return Mathf.Clamp(x / Mathf.Max(1f, Screen.width) * 2f - 1f, -1f, 1f);
    }
}
