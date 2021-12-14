Shader "Custom/MyShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        // Extending the Shader with a Normal Map property. (Added as part of "creating a surface shader" tutorial)
        _Normal ("Normal", 2D) = "bump" {}
        // Extending the Shader with a Cube/Environment Map. (Added as part of "creating a surface shader" tutorial)
        _EnvMap("Environment Map", CUBE) = "" {}
        _Opacity("Opacity", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
                Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        // The actual Normal Map. (Added as part of "creating a surface shader" tutorial)
        sampler2D _Normal;
        // The cube map. (Added as part of "creating a surface shader" tutorial)
        samplerCUBE _EnvMap;

        struct Input
        {
            float2 uv_MainTex;
            // Adding the normal map's UV. (Added as part of "creating a surface shader" tutorial)
            float2 uv_Normal;
            // Obtain the world reflection vector. INTERNAL DATA required. (Added as part of "creating a surface shader" tutorial)
            float3 worldRefl;
            INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;
        half _Opacity;
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
            // We dim the Albedo by half to make the reflection less intense. (Added as part of "creating a surface shader" tutorial)
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * 0.5 * _Color;
            float3 n = tex2D (_MainTex, IN.uv_Normal);

            o.Albedo = c.rgb;
            // Calculating the cubemap reflection by writing to Emission. (Added as part of "creating a surface shader" tutorial)
            // Distort reflection by the normal map. (Added as part of "creating a surface shader" tutorial)
            o.Emission = texCUBE(_EnvMap, IN.worldRefl * n).rgb;

            // Implementing the Normal Map calculation here . (Added as part of "creating a surface shader" tutorial)
            o.Normal = UnpackNormal (tex2D(_Normal, IN.uv_Normal));
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a * _Opacity;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
