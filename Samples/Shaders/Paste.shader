Shader "Hidden/AlephVault/TextureUtils/Paste"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Current Texture", 2D) = "white" {}
        [NoScaleOffset] _OverlayTex ("Overlay", 2D) = "white" {}
        _OverlayOffset ("Overlay Offset", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _OverlayTex;
            float4 _OverlayTex_TexelSize;
            float4 _OverlayOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 current = tex2D(_MainTex, i.uv);
                float4 overlay = tex2D(_OverlayTex, (_OverlayOffset.xy + i.uv * _MainTex_TexelSize.zw) * _OverlayTex_TexelSize.xy);
                float a = current.a * (1 - overlay.a) + overlay.a;
                float4 result;
                result.rgb = lerp(current.rgb, overlay.rgb, overlay.a);
                result.a = a;
                return result;
            }
            ENDCG
        }
    }
}