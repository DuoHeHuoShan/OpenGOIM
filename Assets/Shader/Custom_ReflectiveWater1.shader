Shader "Custom/ReflectiveWater1" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_BumpMap ("Normalmap", 2D) = "bump" {}
	}
	SubShader {
		LOD 100
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "BASE"
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, One One
			ZWrite Off
			Fog {
				Mode Off
			}
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					#include "Lighting.cginc"
					struct appdata
					{
						float4 vertex : POSITION;
						float4 tangent : TANGENT0;
						float3 normal : NORMAL0;
						float2 uv : TEXCOORD0;
					};

					struct v2f
					{
						float2 uv0 : TEXCOORD0;
						float4 uv1 : TEXCOORD1;
						float3 uv2 : TEXCOORD2;
						float3 uv3 : TEXCOORD3;
						float3 uv4 : TEXCOORD4;
						float3 uv5 : TEXCOORD5;
						float4 vertex : SV_POSITION;
					};
					float4 _MainTex_ST;
					v2f vert(appdata v)
					{
						v2f o;
						float4 u_xlat0 = mul(unity_ObjectToWorld, v.vertex);
						o.uv2.xyz = _WorldSpaceCameraPos.xyz - u_xlat0;
						u_xlat0 = mul(unity_MatrixVP, u_xlat0);
						float4 u_xlat1;
						float3 u_xlat2;
						float u_xlat9;
					    o.vertex = u_xlat0;
					    o.uv0.xy = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					    u_xlat0.y = u_xlat0.y * _ProjectionParams.x;
					    u_xlat1.xzw = u_xlat0.xwy * 0.5;
					    o.uv1.zw = u_xlat0.zw;
					    o.uv1.xy = u_xlat1.zz + u_xlat1.xw;
					    // u_xlat0.y = dot(v.normal.xyz, unity_WorldToObject[0].xyz);
					    // u_xlat0.z = dot(v.normal.xyz, unity_WorldToObject[1].xyz);
					    // u_xlat0.x = dot(v.normal.xyz, unity_WorldToObject[2].xyz);
						u_xlat0.x = unity_WorldToObject[0].x;
						u_xlat0.y = unity_WorldToObject[1].x;
						u_xlat0.z = unity_WorldToObject[2].x;
						u_xlat0.z = dot(v.normal.xyz, u_xlat0.xyz);
						u_xlat1.x = unity_WorldToObject[0].y;
						u_xlat1.y = unity_WorldToObject[1].y;
						u_xlat1.z = unity_WorldToObject[2].y;
						u_xlat0.x = dot(v.normal.xyz, u_xlat1.xyz);
						u_xlat1.x = unity_WorldToObject[0].z;
						u_xlat1.y = unity_WorldToObject[1].z;
						u_xlat1.z = unity_WorldToObject[2].z;
						u_xlat0.y = dot(v.normal.xyz, u_xlat1.xyz);
					    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
					    u_xlat9 = rsqrt(u_xlat9);
					    u_xlat0.xyz = u_xlat9 * u_xlat0.xyz;
					    u_xlat1.xyz = v.tangent.yyy * unity_ObjectToWorld[1].yzx;
					    u_xlat1.xyz = unity_ObjectToWorld[0].yzx * v.tangent.xxx + u_xlat1.xyz;
					    u_xlat1.xyz = unity_ObjectToWorld[2].yzx * v.tangent.zzz + u_xlat1.xyz;
					    u_xlat9 = dot(u_xlat1.xyz, u_xlat1.xyz);
					    u_xlat9 = rsqrt(u_xlat9);
					    u_xlat1.xyz = u_xlat9 * u_xlat1.xyz;
					    u_xlat2.xyz = u_xlat0.xyz * u_xlat1.xyz;
					    u_xlat2.xyz = u_xlat0.zxy * u_xlat1.yzx + (-u_xlat2.xyz);
					    u_xlat2.xyz = u_xlat2.xyz * v.tangent.www;
					    o.uv3.y = u_xlat2.x;
					    o.uv3.x = u_xlat1.z;
					    o.uv3.z = u_xlat0.y;
					    o.uv4.x = u_xlat1.x;
					    o.uv5.x = u_xlat1.y;
					    o.uv4.z = u_xlat0.z;
					    o.uv5.z = u_xlat0.x;
					    o.uv4.y = u_xlat2.y;
					    o.uv5.y = u_xlat2.z;
					    return o;
					}
					
					

					half4 _ReflectColor;
					half4 _Color;
					sampler2D _BumpMap;
					half3 u_xlat16_0;
					float3 u_xlat1;
					fixed3 u_xlat10_1;
					half3 u_xlat16_2;
					float3 u_xlat3;
					half3 u_xlat16_3;
					fixed3 u_xlat10_3;
					float3 u_xlat4;
					half u_xlat16_15;
					float u_xlat16;
					half4 frag(v2f i) : SV_Target
					{
					    u_xlat16_0.xyz = glstate_lightmodel_ambient.xyz * _Color.xyz;
					    u_xlat16_0.xyz = u_xlat16_0.xyz + u_xlat16_0.xyz;
					    u_xlat1.xy = i.uv1.xy / i.uv1.ww;
					    u_xlat10_1.xyz = tex2D(_BumpMap, u_xlat1.xy).xyz;
					    u_xlat16_2.xyz = u_xlat10_1.xyz * 2.0 + half3(-1.0, -1.0, -1.0);
					    u_xlat1.x = dot(i.uv3.xyz, u_xlat16_2.xyz);
					    u_xlat1.y = dot(i.uv4.xyz, u_xlat16_2.xyz);
					    u_xlat1.z = dot(i.uv5.xyz, u_xlat16_2.xyz);
					    u_xlat16 = dot(i.uv2.xyz, u_xlat1.xyz);
					    u_xlat16 = u_xlat16 + u_xlat16;
					    u_xlat3.xyz = u_xlat1.xyz * -u_xlat16 + i.uv2.xyz;
					    u_xlat10_3.xyz = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, u_xlat3.xyz).xyz;
					    u_xlat16_3.xyz = u_xlat10_3.xyz * _ReflectColor.xyz;
					    u_xlat16_2.xyz = u_xlat16_3.xyz * _ReflectColor.www;
					    u_xlat16 = dot(i.uv2.xyz, i.uv2.xyz);
					    u_xlat16 = rsqrt(u_xlat16);
					    u_xlat4.xyz = u_xlat16 * i.uv2.xyz;
					    u_xlat16 = dot(u_xlat4.xyz, u_xlat1.xyz);
					    u_xlat16_15 = dot(u_xlat1.xyz, float3(-0.600000024, 0.600000024, -0.400000006));
					    u_xlat16_15 = max(u_xlat16_15, 0.0);
					    u_xlat16_15 = log2(u_xlat16_15);
					    u_xlat16_15 = u_xlat16_15 * 10.0;
					    u_xlat16_15 = exp2(u_xlat16_15);
					    u_xlat16_15 = u_xlat16_15 * 1.5;
					    u_xlat1.x = u_xlat16 + 1.0;
					    u_xlat16_0.xyz = u_xlat16_2.xyz * u_xlat1.xxx + u_xlat16_0.xyz;
					    u_xlat16_2.x = u_xlat1.x * u_xlat1.x;
					    u_xlat16_0.xyz = u_xlat16_2.xxx * u_xlat16_3.xyz + u_xlat16_0.xyz;
						return half4(u_xlat16_15 * _LightColor0.xyz + u_xlat16_0.xyz, _Color.w);
					    
					}
					ENDCG
		}
	}
	Fallback "Legacy Shaders/VertexLit"
}