Shader "Unlit/HammerShadow" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		LOD 100
		Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			LOD 100
			Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend Zero SrcColor, Zero SrcColor
			ZTest Greater
			ZWrite Off
			Stencil {
				Ref 1
				Comp NotEqual
				Pass Keep
				Fail Keep
				// ZFail DecrementWrap
			}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
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
					float4 _MainTex_ST;
					v2f vert (appdata v)
					{
						v2f o;
					    o.uv.xy = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
						o.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, v.vertex));
					    return o;
					}
					
			
					half4 frag(v2f i) : SV_Target
					{
						half3 u_xlat16_0;
						fixed4 u_xlat10_1;
					    u_xlat16_0.x = min(unity_AmbientGround.y, unity_AmbientGround.x);
					    u_xlat16_0.x = min(u_xlat16_0.x, unity_AmbientGround.z);
					    u_xlat16_0.xyz = (-u_xlat16_0.xxx) + unity_AmbientGround.xyz;
					    u_xlat10_1 = tex2D(_MainTex, i.uv.xy);
					    
					    return half4(u_xlat16_0.xyz + u_xlat10_1.xyz, u_xlat10_1.w);
					}
					
					ENDCG
		}
	}
}