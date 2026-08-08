using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cysharp.Threading.Tasks;
using R3;

public class DamageScreenEffect : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private float lowHealthThreshold = 0.5f;
    [SerializeField] private float hitIntensity = 0.6f;
    [SerializeField] private float lowHealthMaxIntensity = 0.5f;
    [SerializeField] private float effectWidth = 0.15f;
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private AnimationCurve lowHealthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Image damageImage;
    private Coroutine effectCoroutine;

    void Awake()
    {
        CreateEffect();
    }

    void CreateEffect()
    {
        GameObject canvasObject = new GameObject("DamageEffectCanvas");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasObject.AddComponent<CanvasScaler>();

        GameObject imageObject = new GameObject("DamageEffect");
        imageObject.transform.SetParent(canvasObject.transform, false);

        damageImage = imageObject.AddComponent<Image>();
        RectTransform rect = damageImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Texture2D texture = CreateGradientTexture();
        damageImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        damageImage.color = new Color(1f, 0f, 0f, 0f);
    }

    Texture2D CreateGradientTexture()
    {
        int width = 512;
        Texture2D texture = new Texture2D(width, 1, TextureFormat.RGBA32, false);

        for (int x = 0; x < width; x++)
        {
            float normalized = (float)x / (width - 1);
            float distance = Mathf.Abs(normalized - 0.5f) * 2f;
            float edgeStart = 1f - effectWidth * 2f;
            float edge = Mathf.InverseLerp(edgeStart, 1f, distance);
            float alpha = Mathf.Pow(edge, 2.5f);
            texture.SetPixel(x, 0, new Color(1f, 0f, 0f, alpha));
        }

        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    public void TakeDamage(int currentHP, int maxHP)
    {
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(DamageEffect(currentHP, maxHP));
    }

    IEnumerator DamageEffect(int currentHP, int maxHP)
    {
        float healthRatio = (float)currentHP / maxHP;

        if (healthRatio < lowHealthThreshold)
        {
            float lowHealthRatio = Mathf.InverseLerp(lowHealthThreshold, 0f, healthRatio);
            float permanentAlpha = lowHealthCurve.Evaluate(lowHealthRatio) * lowHealthMaxIntensity;
            float startAlpha = Mathf.Clamp01(permanentAlpha + hitIntensity);

            float time = 0f;
            while (time < fadeOutTime)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, permanentAlpha, time / fadeOutTime);
                damageImage.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }

            damageImage.color = new Color(1f, 0f, 0f, permanentAlpha);
        }
        else
        {
            float time = 0f;
            while (time < fadeOutTime)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(hitIntensity, 0f, time / fadeOutTime);
                damageImage.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }

            damageImage.color = new Color(1f, 0f, 0f, 0f);
        }
    }
}