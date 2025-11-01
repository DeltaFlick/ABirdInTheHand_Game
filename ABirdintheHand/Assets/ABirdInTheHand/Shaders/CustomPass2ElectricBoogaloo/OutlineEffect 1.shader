Shader "Renderers/OutlineEffect1"

{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0,1,1,1)
        _Thickness("Thickness", Float) = 1.5
        _ObjectMask("Object Mask", 2DArray) = "black" {}
        _ViewportParams("Viewport Params", Vector) = (1920, 1080, 0.0005208333, 0.0009259259)
        
        // Legacy properties for compatibility
        _Color("Color", Color) = (1,1,1,1)
        _ColorMap("ColorMap", 2D) = "white" {}
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [HideInInspector]_BlendMode("_BlendMode", Range(0.0, 1.0)) = 0.5
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    // #pragma enable_d3d11_debug_symbols

    //enable GPU instancing support
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "OutlinePass"
            Tags { "LightMode" = "CustomPass" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always

            Cull Off

            HLSLPROGRAM

            #define _ALPHATEST_ON

            #define _SURFACE_TYPE_TRANSPARENT

            #define _ENABLE_FOG_ON_TRANSPARENT
            
            #define ATTRIBUTES_NEED_TEXCOORD0

            #define VARYINGS_NEED_TEXCOORD0

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            
            TEXTURE2D_ARRAY(_ObjectMask);
            SAMPLER(sampler_ObjectMask);

            
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _Thickness;
                float4 _ViewportParams; 
                
                float4 _ColorMap_ST;
                float4 _Color;
                float _AlphaCutoff;
                float _BlendMode;
            CBUFFER_END

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

             float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                uint eyeIndex = 0; 
                
                float2 texelSize = _ViewportParams.zw; 
                texelSize *= _Thickness;
                
                float center = SAMPLE_TEXTURE2D_ARRAY(_ObjectMask, sampler_ObjectMask, uv, eyeIndex).r;
                
                if (center > 0.5)
                    return float4(0, 0, 0, 0);
                
                float2 leftUV = uv + float2(-texelSize.x, 0);
                float2 rightUV = uv + float2(texelSize.x, 0);
                float2 upUV = uv + float2(0, texelSize.y);
                float2 downUV = uv + float2(0, -texelSize.y);
                
                float left = 0.0;
                float right = 0.0;
                float up = 0.0;
                float down = 0.0;
                
                if (leftUV.x >= 0.0 && leftUV.x <= 1.0 && leftUV.y >= 0.0 && leftUV.y <= 1.0)
                    left = SAMPLE_TEXTURE2D_ARRAY(_ObjectMask, sampler_ObjectMask, leftUV, eyeIndex).r;
                    
                if (rightUV.x >= 0.0 && rightUV.x <= 1.0 && rightUV.y >= 0.0 && rightUV.y <= 1.0)
                    right = SAMPLE_TEXTURE2D_ARRAY(_ObjectMask, sampler_ObjectMask, rightUV, eyeIndex).r;
                    
                if (upUV.x >= 0.0 && upUV.x <= 1.0 && upUV.y >= 0.0 && upUV.y <= 1.0)
                    up = SAMPLE_TEXTURE2D_ARRAY(_ObjectMask, sampler_ObjectMask, upUV, eyeIndex).r;
                    
                if (downUV.x >= 0.0 && downUV.x <= 1.0 && downUV.y >= 0.0 && downUV.y <= 1.0)
                    down = SAMPLE_TEXTURE2D_ARRAY(_ObjectMask, sampler_ObjectMask, downUV, eyeIndex).r;
                
              
                float edge = max(max(left, right), max(up, down));
                
                
                float outline = edge * (1.0 - center);
                outline = saturate(outline);
                
                return float4(_OutlineColor.rgb, outline * _OutlineColor.a);
            }

            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }
    }
}
