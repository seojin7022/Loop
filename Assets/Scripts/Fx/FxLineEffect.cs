using PulleyBun;
using UnityEngine;

/// LineRenderer 하나로 두 가지 이펙트를 그린다.
/// - Ring:    한 점에서 퍼져 나가는 충격파 링
/// - Segment: 선분을 따라 번쩍였다 사라지는 펄스
public class FxLineEffect : MonoBehaviour
{
    public enum Mode { Ring, Segment }

    const int RingSegments = 48;

    LineRenderer line;
    Mode mode;
    Vector3 center, pointA, pointB;
    float startRadius, endRadius;
    float startWidth;
    Color startColor;
    float duration;
    float age;

    static readonly Vector3[] ringBuffer = new Vector3[RingSegments + 1];

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void EnsureLine()
    {
        if (line == null) line = GetComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.material = RuntimeGfx.LineMaterial;
        line.textureMode = LineTextureMode.Stretch;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
    }

    public void PlayRing(
        Vector3 center, float startRadius, float endRadius,
        Color color, float width, float duration, int sortingOrder)
    {
        EnsureLine();

        mode = Mode.Ring;
        this.center = center;
        this.startRadius = startRadius;
        this.endRadius = endRadius;

        Setup(color, width, duration, sortingOrder);
    }

    public void PlaySegment(
        Vector3 a, Vector3 b, Color color, float width, float duration, int sortingOrder)
    {
        EnsureLine();

        mode = Mode.Segment;
        pointA = a;
        pointB = b;

        line.positionCount = 2;
        line.SetPosition(0, a);
        line.SetPosition(1, b);

        Setup(color, width, duration, sortingOrder);
    }

    void Setup(Color color, float width, float duration, int sortingOrder)
    {
        startColor = color;
        startWidth = width;
        this.duration = Mathf.Max(0.01f, duration);
        age = 0f;

        line.sortingOrder = sortingOrder;
        line.startColor = line.endColor = color;
        line.widthMultiplier = width;

        gameObject.SetActive(true);
        Apply(0f);
    }

    void Update()
    {
        age += Time.deltaTime;

        float t = age / duration;
        if (t >= 1f)
        {
            Fx.Release(this);
            return;
        }

        Apply(t);
    }

    void Apply(float t)
    {
        // 빠르게 퍼졌다가 천천히 잦아드는 느낌
        float eased = 1f - Mathf.Pow(1f - t, 3f);
        float fade = 1f - t;

        if (mode == Mode.Ring)
        {
            float radius = Mathf.Lerp(startRadius, endRadius, eased);
            BuildRing(center, radius);

            line.positionCount = ringBuffer.Length;
            line.SetPositions(ringBuffer);
        }
        else
        {
            line.positionCount = 2;
            line.SetPosition(0, pointA);
            line.SetPosition(1, pointB);
        }

        line.widthMultiplier = startWidth * Mathf.Lerp(0.25f, 1f, fade);

        Color color = startColor;
        color.a = startColor.a * fade;
        line.startColor = line.endColor = color;
    }

    static void BuildRing(Vector3 center, float radius)
    {
        for (int i = 0; i <= RingSegments; i++)
        {
            float angle = Mathf.PI * 2f * i / RingSegments;
            ringBuffer[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }
    }
}
