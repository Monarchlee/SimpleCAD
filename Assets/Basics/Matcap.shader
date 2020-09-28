// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Matcap" {
	Properties {
		_MatCapDiffuse ("Capture Map", 2D) = "white" {}
		_CurveScale ("Curve Scale", Float) = 1.0
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpValue ("Normal Value", Range(0,10)) = 1
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex("Albedo Map", 2D) = "white" {}
        _Diffuse ("Diffuse", Float) = 1.0
	}
	
	Subshader {
		Tags { "RenderType"="Opaque" }
		
		Pass {
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				
				struct v2f { 
					float4 pos : SV_POSITION;
					float3 normal : NORMAL;
					float4 uv : TEXCOORD0;
					float3 TtoV0 : TEXCOORD1;
					float3 TtoV1 : TEXCOORD2;
					float3 viewDir : TEXCOORD3;
				};

				uniform float _CurveScale;
				uniform fixed4 _Color;
				uniform sampler2D _BumpMap;          
				uniform float4 _BumpMap_ST;
				uniform sampler2D _MatCapDiffuse;
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform fixed _BumpValue;
            	uniform float _Diffuse;
            
				v2f vert (appdata_tan v)
				{
					v2f o;
					o.normal = v.normal;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
					o.uv.zw = TRANSFORM_TEX(v.texcoord,_BumpMap);
					
					float4 viewPos = mul(unity_WorldToObject, _WorldSpaceCameraPos);
					o.viewDir = normalize(UnityObjectToClipPos(viewPos) - o.pos);
					
					TANGENT_SPACE_ROTATION;
					o.TtoV0 = normalize(mul(rotation, UNITY_MATRIX_IT_MV[0].xyz));
					o.TtoV1 = normalize(mul(rotation, UNITY_MATRIX_IT_MV[1].xyz));
					return o;
				}			
				
				float4 frag (v2f i) : COLOR
				{
					fixed4 c = tex2D(_MainTex, i.uv.xy);
					float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
					normal.xy *= _BumpValue;
					normal.z = sqrt(1.0- saturate(dot(normal.xy ,normal.xy)));
					normal = normalize(normal);
					
					half2 vn;
					vn.x = dot(i.TtoV0, normal);
					vn.y = dot(i.TtoV1, normal);

					half2 vm;
					float3 adjust =  i.viewDir - 2 * dot(normal, i.viewDir) * normal;
					vm.x = dot(i.TtoV0, adjust);
					vm.y = dot(i.TtoV1, adjust);

					float cur = saturate(length(fwidth(i.normal)) / length(fwidth(i.pos)) * _CurveScale);

					float2 samp = lerp(vm, vn, cur);

					fixed4 matcapLookup = tex2D(_MatCapDiffuse, samp * 0.5 + 0.5);					
					fixed4 finalColor = matcapLookup * c * _Diffuse;
					return finalColor;
				}

			ENDCG
		}
	}
}