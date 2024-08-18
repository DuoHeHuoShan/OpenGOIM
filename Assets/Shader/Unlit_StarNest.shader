Shader "Unlit/StarNest" {
	Properties {
		_FormuParam ("Formula Param", Float) = 0.53
		_Iterations ("Iterations", Float) = 15
		_VolSteps ("Volume Steps", Float) = 10
		_Speed ("Speed", Float) = 0.01
		_Zoom ("Zoom", Float) = 1.8
		_Brightness ("Brightness", Float) = 0.0025
		_Darkmatter ("Darkmatter", Float) = 0.3
		_Distfading ("DistFading", Float) = 0.43
		_Saturation ("Saturation", Float) = 0.85
		_StepSize ("StepSize", Float) = 0.3
		_Tile ("Tile", Float) = 0.85
	}
	SubShader {
		LOD 100
		Tags { "QUEUE" = "Geometry-1" "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "QUEUE" = "Geometry-1" "RenderType" = "Opaque" }
			Blend One One, One One
			Cull Off
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
						float4 vertex : SV_POSITION;
					};
					v2f vert(appdata v)
					{
						v2f o;
					    o.uv0.xy = v.uv.xy;
					    o.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, v.vertex));
					    return o;
					}
					
		
	
					float _FormuParam;
					int _Iterations;
					int _VolSteps;
					float _Speed;
					float _Zoom;
					float _Brightness;
					float _Darkmatter;
					float _Distfading;
					float _Saturation;
					float _StepSize;
					float _Tile;
					float3 u_xlat0;
					float2 u_xlat1;
					float3 u_xlat2;
					float3 u_xlat3;
					bool u_xlatb3;
					float3 u_xlat4;
					float3 u_xlat5;
					float3 u_xlat6;
					bool u_xlatb6;
					float3 u_xlat7;
					bool u_xlatb11;
					float u_xlat13;
					float u_xlat14;
					float u_xlat15;
					float u_xlat21;
					float u_xlat22;
					int u_xlati23;
					float u_xlat24;
					float u_xlat25;
					int u_xlati26;
					half4 frag(v2f i) : SV_Target
					{
					    u_xlat0.xy = float2(i.uv0.x / _ScreenParams.z, i.uv0.y / _ScreenParams.w);
					    u_xlat0.xy = u_xlat0.xy + float2(-0.5, -0.5);
					    u_xlat14 = _ScreenParams.w / _ScreenParams.z;
					    u_xlat0.y = u_xlat14 * u_xlat0.y;
					    u_xlat1.xy = u_xlat0.xy * _Zoom;
					    u_xlat0.y = _Time.x * _Speed;
					    u_xlat0.x = 24.0;
					    u_xlat0.z = 0.0;
					    u_xlat0.xyz = u_xlat0.xyz + float3(0.0, 35.25, -2.0);
					    u_xlat21 = _Tile + _Tile;
					    u_xlat15 = 0.100000001;
					    u_xlat22 = 1.0;
					    u_xlat2.x = 0.0;
					    u_xlat2.y = 0.0;
					    u_xlat2.z = 0.0;
					    for(int u_xlati_loop_1 = 0 ; u_xlati_loop_1<_VolSteps ; u_xlati_loop_1++)
					    {
					        u_xlat3.yz = u_xlat1.xy * u_xlat15;
					        u_xlat3.x = u_xlat15;
					        u_xlat4.xyz = u_xlat3.yzx * 0.5 + u_xlat0.xyz;
					        u_xlat4.xyz = u_xlat4.xyz / u_xlat21;
							bool tmpvar1 = u_xlat4.x >= -u_xlat4.x;
							bool tmpvar2 = u_xlat4.y >= -u_xlat4.y;
							bool tmpvar3 = u_xlat4.z >= -u_xlat4.z;
					        u_xlat4.xyz = frac(abs(u_xlat4.xyz));
							
					        u_xlat4.x = tmpvar1 ? u_xlat4.x : (-u_xlat4.x);
					        u_xlat4.y = tmpvar2 ? u_xlat4.y : (-u_xlat4.y);
					        u_xlat4.z = tmpvar3 ? u_xlat4.z : (-u_xlat4.z);
					        u_xlat4.xyz = (-u_xlat4.xyz) * u_xlat21 + float3(_Tile, _Tile, _Tile);
					        u_xlat5.xyz = abs(u_xlat4.xyz);
					        u_xlat24 = 0.0;
					        u_xlat25 = 0.0;
					        u_xlati26 = 0;
					        while(true){
					            
					            if(u_xlati26>=_Iterations){break;}
					            u_xlat6.x = dot(u_xlat5.xyz, u_xlat5.xyz);
					            u_xlat6.xyz = abs(u_xlat5.xyz) / u_xlat6.xxx;
					            u_xlat5.xyz = u_xlat6.xyz - float3(_FormuParam, _FormuParam, _FormuParam);
					            u_xlat6.x = dot(u_xlat5.xyz, u_xlat5.xyz);
					            u_xlat6.x = sqrt(u_xlat6.x);
					            u_xlat13 = (-u_xlat24) + u_xlat6.x;
					            u_xlat25 = u_xlat25 + abs(u_xlat13);
					            u_xlati26 = u_xlati26 + 1;
					            u_xlat24 = u_xlat6.x;
					        }
					        u_xlat24 = u_xlat25 * u_xlat25;
					        u_xlat4.x = (-u_xlat24) * 0.00100000005 + _Darkmatter;
					        u_xlat4.x = max(u_xlat4.x, 0.0);
					        u_xlat24 = u_xlat24 * u_xlat25;
					        u_xlat4.x = (-u_xlat4.x) + 1.0;
					        u_xlat4.x = u_xlat22 * u_xlat4.x;
					        u_xlat4.x = (6<u_xlati_loop_1) ? u_xlat4.x : u_xlat22;
					        u_xlat5.xyz = u_xlat2.xyz + u_xlat4.xxx;
					        u_xlat3.y = u_xlat15 * u_xlat15;
					        u_xlat3.z = u_xlat3.y * u_xlat3.y;
					        u_xlat3.xyz = u_xlat24 * u_xlat3.xyz;
					        u_xlat3.xyz = u_xlat3.xyz * _Brightness;
					        u_xlat2.xyz = u_xlat3.xyz * u_xlat4.xxx + u_xlat5.xyz;
					        u_xlat22 = u_xlat4.x * _Distfading;
					        u_xlat15 = u_xlat15 + _StepSize;
					    }
					    u_xlat0.x = dot(u_xlat2.xyz, u_xlat2.xyz);
					    u_xlat0.x = sqrt(u_xlat0.x);
					    u_xlat7.xyz = (-u_xlat0.xxx) + u_xlat2.xyz;
					    u_xlat0.xyz = _Saturation * u_xlat7.xyz + u_xlat0.xxx;
						half4 result;
					    result.xyz = u_xlat0.xyz * 0.00999999978 + float3(-0.100000001, -0.100000001, -0.100000001);
					    result.xyz = clamp(result.xyz, 0.0, 1.0);
					    result.w = 1.0;
					    return result;
					}
					
					ENDCG
		}
	}
}