Shader "Custom/Hammer" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", 2D) = "white" {}
		_Normal ("Normal", 2D) = "bump" {}
	}
	SubShader {
		LOD 200
		Tags { "RenderType" = "Opaque" }
		Stencil {
			Ref 1
			Comp Always
			Pass Replace
			Fail Keep
			ZFail Keep
		}
		CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _Metallic;
		sampler2D _Normal;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_BumpMap;
        };

        half _Glossiness;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
			o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_BumpMap));
            o.Metallic = tex2D(_Metallic, IN.uv_MainTex).w;
            o.Smoothness = _Glossiness * tex2D(_Metallic, IN.uv_MainTex).w;
            o.Alpha = c.a;
        }
        ENDCG
	}
}