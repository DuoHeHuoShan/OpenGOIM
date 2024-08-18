Shader "Unlit/MotionBlurSprite" {
	Properties {
		color_map ("color_map", 2D) = "white" {}
		_Tint ("tint", Float) = 1
	}
	SubShader {
		Pass {
			Tags { "LIGHTMODE" = "MOTIONVECTORS" }
			Blend One One, One One
			ZTest Less
			ZWrite Off
			Cull Off
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
			float _Tint;
			sampler2D color_map;
			v2f vert (appdata v)
			{
			    v2f o;
				o.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, v.vertex));
			    o.uv = v.uv;
			    return o;
			}
					

					float4 frag(v2f i) : SV_Target 
					{
					    float u_xlat0 = _Tint * 0.100000001;
					    fixed4 u_xlat10_1 = tex2D(color_map, i.uv.xy);
					    return float4(u_xlat10_1.xyz * u_xlat0, u_xlat10_1.w);
					}
					
				
				
			
			ENDCG
		}
	}
}