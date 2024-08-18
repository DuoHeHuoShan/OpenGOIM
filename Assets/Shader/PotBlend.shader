Shader "Custom/PotBlend"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" { }
        _MainTexGold ("Gold Albedo", 2D) = "white" { }
        _MetalSmoothness ("MetallicSmoothness", 2D) = "white" { }
        _Normal ("Normal", 2D) = "bump" { }
        _NormalGold ("NormalGold", 2D) = "bump" { }
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Glossiness ("Glossiness", Range(0, 1)) = 0
        _Goldness ("Goldness", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MainTexGold;
        sampler2D _Normal;
        sampler2D _NormalGold;
        sampler2D _MetalSmoothness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MainTexGold;
            float2 uv_Normal;
            float2 uv_NormalGold;
            float2 uv_MetalSmoothness;
        };

        half _Glossiness;
        half _Metallic;
        float _Goldness;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 cgold = tex2D (_MainTexGold, IN.uv_MainTexGold);
            fixed3 normal = UnpackNormal (tex2D (_Normal, IN.uv_Normal));
            fixed3 normalGold = UnpackNormal (tex2D (_NormalGold, IN.uv_NormalGold));
            o.Albedo = lerp (c.rgb, cgold.rgb, _Goldness);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness * tex2D (_MetalSmoothness, IN.uv_MetalSmoothness);
            o.Alpha = lerp (c.a, cgold.a, _Goldness);
            o.Normal = lerp (normal, normalGold, _Goldness);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
