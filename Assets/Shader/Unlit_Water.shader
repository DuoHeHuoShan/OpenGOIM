Shader "Unlit/Water" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_SkyColor ("Sky Color", Color) = (1,1,1,1)
		_WaterColor ("Water Color", Color) = (0.2,0.4,0.6,1)
	}
	SubShader {
		LOD 100
		Tags { "QUEUE" = "Overlay" "RenderType" = "Transparent" }
		GrabPass {
		}
		Pass {
			LOD 100
			Tags { "QUEUE" = "Overlay" "RenderType" = "Transparent" }
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
						float2 uv0 : TEXCOORD0;
						float4 uv1 : TEXCOORD1;
						float4 vertex : SV_POSITION;
					};
					float4 _SkyColor;
					float4 _WaterColor;
					sampler2D _MainTex;
					float4 _MainTex_ST;
					sampler2D _GrabTexture;
					v2f vert(appdata v)
					{
						v2f o;
					    o.uv0.xy = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
						float4 u_xlat0 = mul(unity_MatrixVP, mul(unity_ObjectToWorld, v.vertex));
						float4 u_xlat1;
						o.vertex = u_xlat0;
					    o.uv1 = ComputeGrabScreenPos(o.vertex);
					    return o;
					}
					
					
					float u_xlat0;
					float4 u_xlat1;
					float4 u_xlat2;
					half4 u_xlat16_2;
					fixed4 u_xlat10_2;
					float4 u_xlat3;
					half4 u_xlat16_3;
					fixed4 u_xlat10_3;
					fixed4 u_xlat10_4;
					float3 u_xlat5;
					fixed3 u_xlat10_5;
					float u_xlat10;
					half4 frag(v2f i) : SV_Target
					{
					    u_xlat0 = (-_Time.x) * 0.699999988 + i.uv0.x;
					    u_xlat0 = u_xlat0 * 37.0;
					    u_xlat0 = sin(u_xlat0);
					    u_xlat0 = u_xlat0 * 0.00150000001 + i.uv0.y;
					    u_xlat5.x = i.uv0.x + (-_Time.x);
					    u_xlat5.x = u_xlat5.x * 46.0;
					    u_xlat5.x = sin(u_xlat5.x);
					    u_xlat0 = u_xlat5.x * 0.00150000001 + u_xlat0;
					    u_xlat0 = (-u_xlat0) + 0.996999979;
					    u_xlat0 = u_xlat0 * 120.0;
					    u_xlat0 = clamp(u_xlat0, 0.0, 1.0);
					    u_xlat0 = (-u_xlat0) + 1.0;
					    u_xlat5.x = u_xlat0 + u_xlat0;
					    u_xlat0 = u_xlat0 * 10.0 + -9.0;
					    u_xlat0 = max(u_xlat0, 0.0);
					    u_xlat10 = sin(u_xlat5.x);
					    u_xlat5.x = min(u_xlat5.x, 1.0);
					    u_xlat1 = u_xlat5.xxxx * _SkyColor;
					    u_xlat1 = u_xlat1 * 0.400000006;
					    u_xlat2.x = _Time.x;
					    u_xlat2.y = 0.0;
					    u_xlat5.xz = u_xlat2.xy + i.uv0.xy;
					    u_xlat10_5.xz = tex2D(_MainTex, u_xlat5.xz).xy;
					    u_xlat2.xy = u_xlat10_5.xz * 0.00999999978;
					    u_xlat2.z = u_xlat10 * 0.100000001 + u_xlat2.y;
					    u_xlat2 = i.uv1.xyxy * 0.980000019 + (-u_xlat2.xzxz);
					    u_xlat10_3 = tex2D(_GrabTexture, u_xlat2.zw);
					    u_xlat2 = u_xlat2 + float4(0.00100000005, 0.00100000005, -0.00100000005, -0.00100000005);
					    u_xlat10_4 = tex2D(_GrabTexture, u_xlat2.xy);
					    u_xlat10_2 = tex2D(_GrabTexture, u_xlat2.zw);
					    u_xlat16_3 = u_xlat10_3 + u_xlat10_4;
					    u_xlat16_2 = u_xlat10_2 + u_xlat16_3;
					    u_xlat5.x = (-i.uv0.y) + 1.0;
					    u_xlat5.x = clamp(u_xlat5.x, 0.0, 1.0);
					    u_xlat5.x = u_xlat5.x * 10.0;
					    u_xlat5.x = min(u_xlat5.x, 1.0);
					    u_xlat3 = u_xlat5.xxxx * _WaterColor;
					    u_xlat5.x = (-u_xlat5.x) + 1.0;
					    u_xlat2 = u_xlat16_2 * 0.333333343 + (-u_xlat3);
					    u_xlat3 = (-_WaterColor) + float4(1.0, 1.0, 1.0, 1.0);
					    u_xlat3 = u_xlat5.xxxx * u_xlat3;
					    u_xlat1 = u_xlat2 * u_xlat3 + u_xlat1;
					    u_xlat10_2 = tex2D(_GrabTexture, i.uv1.xy);
					    u_xlat2 = (-u_xlat1) + u_xlat10_2;
					    half4 result = u_xlat0 * u_xlat2 + u_xlat1;
					    result = clamp(result, 0.0, 1.0);
					    return result;
					}
					
					ENDCG
		}
	}
}