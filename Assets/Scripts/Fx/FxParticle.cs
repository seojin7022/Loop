using UnityEngine;

/// 짧게 튀어나갔다 사라지는 스프라이트 파티클 한 알. Fx 가 풀링해서 재사용한다.
public class FxParticle : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Vector3 velocity;
    float drag;
    float lifetime;
    float age;
    float startSize;
    Color startColor;
    float spin;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(
        Sprite sprite, Vector3 position, Vector3 velocity, Color color,
        float size, float lifetime, float drag, float spin, int sortingOrder)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        transform.localScale = Vector3.one * size;

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;

        this.velocity = velocity;
        this.drag = drag;
        this.lifetime = Mathf.Max(0.01f, lifetime);
        this.spin = spin;

        startSize = size;
        startColor = color;
        age = 0f;

        gameObject.SetActive(true);
    }

    void Update()
    {
        age += Time.deltaTime;

        float t = age / lifetime;
        if (t >= 1f)
        {
            Fx.Release(this);
            return;
        }

        velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(0f, 0f, spin * Time.deltaTime);

        // 뒤로 갈수록 빠르게 작아지고 흐려진다.
        float fade = 1f - t * t;
        transform.localScale = Vector3.one * (startSize * Mathf.Lerp(0.35f, 1f, fade));

        Color color = startColor;
        color.a = startColor.a * fade;
        spriteRenderer.color = color;
    }
}
