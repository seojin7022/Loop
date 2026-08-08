// 적 타격 시 흰색 실루엣 플래시를 그리기 위한 언릿 스프라이트 셰이더.
// 적 본체의 머티리얼은 건드리지 않고, 같은 스프라이트를 이 셰이더로 한 장 겹쳐 그린다.
// URP 2D Renderer(Renderer2D)에서 렌더되도록 LightMode 를 Universal2D 로 지정한다.
Shader "Loop/SpriteFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FlashColor;
                float  _FlashAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 base = texel * IN.color;

                // 알파는 스프라이트 모양을 그대로 쓰고, 색만 플래시 색으로 덮는다.
                half3 rgb = lerp(base.rgb, _FlashColor.rgb, _FlashAmount);
                return half4(rgb, base.a * _FlashColor.a);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
