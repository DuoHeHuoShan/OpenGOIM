Shader "Unlit/BennettSkybox" {
	Properties {
		_TopColor ("Top Color", Color) = (1,1,1,1)
		_MidColor ("Middle Color", Color) = (0.5,0.5,0.5,1)
		_BottomColor ("Bottom Color", Color) = (0,0,0,1)
	}
	SubShader {
		LOD 100
		Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Skybox" "QUEUE" = "Background" "RenderType" = "Background" }
		Pass {
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Skybox" "QUEUE" = "Background" "RenderType" = "Background" }
			ZWrite Off
			Fog {
				Mode Off
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 _TopColor;
			float4 _MidColor;
			float4 _BottomColor;
			// vertex
			// in highp vec4 in_POSITION0;
			// in highp vec2 in_TEXCOORD0;
			// out highp vec2 vs_TEXCOORD0;
			// out highp vec4 vs_TEXCOORD1;
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
			v2f vert(appdata v)
			{
				v2f o;
			    o.uv0.xy = v.uv.xy;
			    // u_xlat0 = in_POSITION0.yyyy * hlslcc_mtx4x4unity_ObjectToWorld[1];
			    // u_xlat0 = hlslcc_mtx4x4unity_ObjectToWorld[0] * in_POSITION0.xxxx + u_xlat0;
			    // u_xlat0 = hlslcc_mtx4x4unity_ObjectToWorld[2] * in_POSITION0.zzzz + u_xlat0;
			    // u_xlat0 = u_xlat0 + hlslcc_mtx4x4unity_ObjectToWorld[3];
			    // u_xlat1 = u_xlat0.yyyy * hlslcc_mtx4x4unity_MatrixVP[1];
			    // u_xlat1 = hlslcc_mtx4x4unity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
			    // u_xlat1 = hlslcc_mtx4x4unity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
			    // u_xlat0 = hlslcc_mtx4x4unity_MatrixVP[3] * u_xlat0.wwww + u_xlat1;
			    // gl_Position = u_xlat0;
				float4 u_xlat0 = mul(unity_MatrixVP, mul(unity_ObjectToWorld, v.vertex));
				float4 u_xlat1;
				o.vertex = u_xlat0;
				u_xlat0.y *= _ProjectionParams.x;
				u_xlat1.xzw = u_xlat0.xwy * 0.5;
				o.uv1.zw = u_xlat0.zw;
				o.uv1.xy = u_xlat1.zz + u_xlat1.xw;
			    return o;
			}
			
			// fragment
			half4 frag(v2f i) : SV_Target
			{
				float3 u_xlat0;
				float3 u_xlat1;
				float3 u_xlat2;
				float2 u_xlat4;
			    u_xlat0.x = i.uv1.y / i.uv1.w;
				u_xlat0.y = (-u_xlat0.x) + 1.0;
				u_xlat4.x = u_xlat0.y * u_xlat0.y;
				u_xlat4.y = u_xlat0.x * 3.0;
				u_xlat1.x = u_xlat4.y * u_xlat4.x;
				u_xlat4.xy = u_xlat0.yx * u_xlat4.xy;
				u_xlat2.x = u_xlat0.y * u_xlat4.y;
				u_xlat1.xyz = u_xlat1.xxx * _MidColor.xyz;
				u_xlat1.xyz = u_xlat4.xxx * _BottomColor.xyz + u_xlat1.xyz;
				u_xlat2.xyz = u_xlat2.xxx * _MidColor.xyz + u_xlat1.xyz;
				u_xlat1.x = u_xlat0.x * u_xlat0.x;
				u_xlat0.x = u_xlat0.x * u_xlat1.x;
				u_xlat0.xyz = u_xlat0.xxx * _TopColor.xyz + u_xlat2.xyz;
				u_xlat0.xyz = clamp(u_xlat0.xyz, 0.0, 1.0);
			    return half4(u_xlat0, 1.0);
			}
			ENDCG
		}
	}
}