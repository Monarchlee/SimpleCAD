// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/SphereSDFTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center("Center", Vector) = (1,1,1,1)
        _Scale ("Scale", Vector) = (1,1,1,1)
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
                float4 wsPos : TEXCOORD1;
                float3 wsView : TEXCOORD2;
            };

            struct p2r
            {
                float4 color : COLOR0;
                float4 depth : COLOR1;
            };

            float sdSphere(float3 pos, float3 center, float3 scale)
            {
                float3 stp = (pos / scale - center);
                float stpl = length(stp);
                float3 stpn = normalize(stp);
                if (stpl < 1) return -length((stpn - stp) * scale);
                else return length((stp - stpn) * scale);
            }

            float3 tmSphere(float3 ori, float3 dir, float3 center, float3 scale)
            {
                return float3(0, 0, 0);
            }

            float3 _Center, _Scale;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.wsPos = UnityObjectToClipPos(v.vertex);
                o.wsView = WorldSpaceViewDir(o.wsPos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 rayOrigin = i.wsPos;
                float3 rayDir = normalize(-i.wsView);
                float rayDist = 0;

                for (int i = 0; i < 100; i++)
                {
                    float3 sample = rayOrigin + rayDir * rayDist;
                    float sdf = sdSphere(sample, _Center, _Scale);

                    if (sdf < 0.01) return (1, 1, 1, 1);
                    else rayDist += sdf;
                }


                return (0,0,0,0);
            }
            ENDCG
        }
    }
}
