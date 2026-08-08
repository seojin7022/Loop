using UnityEngine;

namespace PulleyBun
{
    /// 별도 아트 에셋 없이 오라·하수인 표시를 만들기 위한 런타임 스프라이트/머티리얼 캐시.
    public static class RuntimeGfx
    {
        static Sprite square;
        static Sprite circle;
        static Material lineMaterial;

        public static Sprite Square
        {
            get
            {
                if (square == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "RuntimeSquare" };
                    var pixels = new Color32[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                    square.name = "RuntimeSquare";
                }
                return square;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    const int size = 32;
                    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "RuntimeCircle" };
                    float r = size * 0.5f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                            float a = Mathf.Clamp01((r - d) * 2f);
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                        }
                    }
                    tex.Apply();
                    circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                    circle.name = "RuntimeCircle";
                }
                return circle;
            }
        }

        static Texture2D dashTexture;
        static Material dashLineMaterial;

        /// 가로로 반복하면 점선이 되는 작은 텍스처 (칠한 구간 5 / 빈 구간 3)
        public static Texture2D DashTexture
        {
            get
            {
                if (dashTexture == null)
                {
                    const int width = 8;
                    const int filled = 5;

                    dashTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
                    {
                        name = "RuntimeDash",
                        wrapMode = TextureWrapMode.Repeat,
                        filterMode = FilterMode.Point,
                    };

                    for (int x = 0; x < width; x++)
                        dashTexture.SetPixel(x, 0, new Color(1f, 1f, 1f, x < filled ? 1f : 0f));

                    dashTexture.Apply();
                }
                return dashTexture;
            }
        }

        /// 점선용 머티리얼. 타일링 밀도는 LineRenderer.textureScale 로 각자 조절한다.
        public static Material DashLineMaterial
        {
            get
            {
                if (dashLineMaterial == null)
                {
                    Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
                    dashLineMaterial = new Material(shader)
                    {
                        name = "RuntimeDashLineMaterial",
                        mainTexture = DashTexture,
                    };
                }
                return dashLineMaterial;
            }
        }

        public static Material LineMaterial
        {
            get
            {
                if (lineMaterial == null)
                {
                    Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
                    lineMaterial = new Material(shader) { name = "RuntimeLineMaterial" };
                }
                return lineMaterial;
            }
        }

        /// 선분 주변 반경 radius의 캡슐(스타디움) 외곽선을 월드 좌표로 만든다.
        public static Vector3[] CapsuleOutline(Vector3 a, Vector3 b, float radius, int arcSegments = 16)
        {
            Vector3 axis = b - a;
            Vector3 dir = axis.sqrMagnitude > 1e-6f ? axis.normalized : Vector3.right;
            Vector3 normal = new(-dir.y, dir.x, 0f);

            var points = new Vector3[arcSegments * 2 + 3];
            int index = 0;

            for (int i = 0; i <= arcSegments; i++)
            {
                float t = Mathf.PI * i / arcSegments;
                points[index++] = b + (dir * Mathf.Cos(t - Mathf.PI * 0.5f) + normal * Mathf.Sin(t - Mathf.PI * 0.5f)) * radius;
            }
            for (int i = 0; i <= arcSegments; i++)
            {
                float t = Mathf.PI * i / arcSegments;
                points[index++] = a + (-dir * Mathf.Cos(t - Mathf.PI * 0.5f) - normal * Mathf.Sin(t - Mathf.PI * 0.5f)) * radius;
            }
            points[index] = points[0];

            return points;
        }

        /// 점 p 와 선분 ab 사이의 최단 거리.
        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 1e-6f) return Vector2.Distance(p, a);

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
