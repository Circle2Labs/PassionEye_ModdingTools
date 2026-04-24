Shader "Toon/Plastic"
{
    Properties
    {
        // Surface options
        [KeywordEnum(Lambert,StylizedLambert,HalfLambert)]_ShadingModel("Shading Model", Float) = 0.0
        [KeywordEnum(Additive,Replace)]_AddLightMix("Additional Light Mix", Range(0, 1)) = 0.0
        
        // Main color
        _BaseColor("Main Color", Color) = (1,1,1,1)
        [MainTexture]_BaseMap("Base Color", 2D) = "white" {}
        _Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5
        
        // Normal map
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 1
        
        // Specularity
        _SpecularPower("Specular Power", Range(0, 1)) = 0.5
        _SpecularAmount("Specular Amount", Range(0, 1)) = 1
        [HDR]_SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularMap("Specular Map", 2D) = "white" {}
        
        // rim light
        _RimLightColor("Rim Light Color", Color) = (1,1,1,1)
        _RimLightAmount("Rim Light Amount", Range(0.0, 1.5)) = 1.0
        [PowerSlider(5)]_RimLightPower("Rim Light Power", Range(0, 30)) = 5
        
        // Clothing layers
        _ClothingLayersSeparation ("Clothing Layers Separation", Range(0, 0.01)) = 0.003
        _ClothingLayer ("Clothing Layer", float) = 0
        
        // Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _ZTest("__zt", Float) = 4.0
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0
        [HideInInspector] _AddPrecomputedVelocity("_AddPrecomputedVelocity", Float) = 0.0
        
        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0
    }
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit"
        }
        Pass {
            Name "PlasticForwardLit"
            Tags{ "LightMode" = "UniversalForward" }
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #define RG_SPECULAR
            #define RG_PLASTIC
            #include_with_pragmas "../CommonVariants.hlsl"
            #include_with_pragmas "PlasticInput.hlsl"
            #include_with_pragmas "../CommonSetup.hlsl"
            #include_with_pragmas "PlasticForward.hlsl"
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile _ LOD_FADE_CROSSFADE dont use it

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing dont use it
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl" dont use it

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "PlasticInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass {
            Name "DepthOnly"
            Tags {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile _ LOD_FADE_CROSSFADE dont use it

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing dont use it
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl" dont use it

            // -------------------------------------
            // Includes
            #include "PlasticInput.hlsl"
            #include "../DepthOnlyCloth.hlsl"
            ENDHLSL
        }
        Pass {
            Name "DepthNormals"
            Tags {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile _ LOD_FADE_CROSSFADE dont use it

            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing dont use it
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl" dont use it

            // -------------------------------------
            // Includes
            #include "PlasticInput.hlsl"
            #include "../DepthNormalsCloth.hlsl"
            ENDHLSL
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "PlasticInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }
        Pass {
            Name "MotionVectors"
            Tags { "LightMode" = "MotionVectors" }
            ColorMask RG

            HLSLPROGRAM
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma multi_compile _ LOD_FADE_CROSSFADE dont use it
            #pragma shader_feature_local_vertex _ADD_PRECOMPUTED_VELOCITY

            #include "PlasticInput.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ObjectMotionVectors.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "GameAssets.Shaders.ShaderGUIs.BaseToonShader"
    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitShader"
}