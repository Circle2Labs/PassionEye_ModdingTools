Shader "Toon/Metal"
{
    Properties {
        _LightSmooth("Light Smoothing", Range(0.0001,1)) = 0.3
        _LightMin("Light Min", Range(0,0.999)) = 0.25
        _MidPoint("Mid Point", Range(0,1)) = 0.25
        
        // Main color
        _TintColor("Main Color", Color) = (1,1,1,1)
        [MainTexture]_MainTex("Base Color", 2D) = "white" {}
        
        // Normal map
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 1
        
        //metalness
        _MetallicColor("Metallic Color", Color) = (1,1,1,1)
        [PowerSlider(3)]_MetallicStrength("Metallic Strength", Range(0, 1)) = 1
        _MetallicReflection("Metallic Reflection", Range(0, 1)) = 1
        _MetallicRoughness("Metallic Roughness", Range(0, 1)) = .2
        
        // hue shift
        _ShiftAmount("Shift Amount", Range(0,1)) = 0.1
        _kelvinTemp("Kelvin temperature", Range(0, 1)) = .5
        _DayNightRamp("Day Night Ramp", 2D) = "white" {}
        _DNTintStr("Day Night Tint Strength", Range(0, 1)) = 1
        
        // Blending state
        [HideInInspector]_Surface("__surface", Float) = 0.0
        [HideInInspector]_Blend("__blend", Float) = 0.0
        [HideInInspector]_Cull("__cull", Float) = 2.0
        [HideInInspector][Toggle(_ALPHATEST_ON)] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zwrite", Float) = 1.0
        [HideInInspector] _ZTest("__ztest", Float) = 4.0
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0
    }
 SubShader {
        Pass {
            Name "BaseToon"
            
            Tags { 
                "RenderPipeline"="UniversalPipeline"
            }
            
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]
                        
            HLSLPROGRAM
            #pragma 
            #include "MetalForward.hlsl"
            
            ENDHLSL
        }
        UsePass "Toon/Base/ShadowCaster"
        UsePass "Toon/Base/DepthOnly"
        UsePass "Toon/Base/DepthNormals"
        UsePass "Toon/Base/META"
    }
    //FallBack "Hidden/Core/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.ToonShaderGUI"
}