Shader "Adventure Forest/basic"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [Toggle] _Wind("Wind", Float) = 1
        _WindPower ("Wind Power", Range(0,1)) = 0.5
        _Transparency ("Transparency", Range(0,1)) = 1.0        
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Cull off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow vertex:vert
        

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float _Wind;
        float _WindPower;
        

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

       

        
        fixed4 _Color;

         void vert (inout appdata_full v) {
            if(_Wind > 0){
				float3 vertexWorld = mul (unity_ObjectToWorld, v.vertex);

                float height = v.vertex.z / 0.017;
                v.vertex.y += sin(_Time.g * 3 + vertexWorld.z) * height * 0.01 * _WindPower;
			}
                
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

         half _Transparency;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            //clip(c.a - 0.1);
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;

            // Screen-door transparency: Discard pixel if below threshold.
            float4x4 thresholdMatrix =
            {  1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
            13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
            4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
            16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            float2 pos = IN.screenPos.xy / IN.screenPos.w;
            pos *= _ScreenParams.xy; // pixel position
            clip(c.a - thresholdMatrix[pos.x % 4] [pos.y % 4]);
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
