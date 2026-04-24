Shader "Toon/Skin"
{
    Properties
    {
        // Surface options
        //[KeywordEnum(Lambert,StylizedLambert,HalfLambert)]_ShadingModel("Shading Model", Float) = 0.0
        [KeywordEnum(Additive,Replace)]_AddLightMix("Additional Light Mix", Range(0, 1)) = 0.0
        // Main color
        [MainColor]_BaseColor("Main Color", Color) = (1,1,1,1)
        [MainTexture]_BaseMap("Base Color", 2D) = "white" {}
        _Cutoff("Alpha Clipping", Range(0.0, 1.0)) = 0.5
        
        // Normal map
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 1
        
        // FaceRelated
        [ToggleUI] _IsFace("Is Face", Float) = 0.0
        [ToggleUI] _FaceDbg("Face Debug", Float) = 0.0
        _SphereCenter("Sphere Center", Vector) = (0,0,0,0)
        _SphereRadius("Sphere Radius", Float) = 0.5
        
        // RGB Offset
        _RChSmooth("Red Channel Smoothness", Range(0, 1)) = 1
        _GChSmooth("Green Channel Smoothness", Range(0, 1)) = 1
        _BChSmooth("Blue Channel Smoothness", Range(0, 1)) = 1
        
        // Specularity
        _SpecularPower("Specular Power", Range(0, 1)) = 0.5
        _SpecularAmount("Specular Amount", Range(0, 1)) = 1
        [HDR]_SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularMap("Specular Map", 2D) = "white" {}
        
        // OverlayMap
        _OverlayMap("Overlay Map", 2D) = "black" {}
        
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
            Name "SkinForwardLit"
            Tags{
                 "LightMode" = "UniversalForward"
            }
            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]
            
            HLSLPROGRAM
            #pragma target 2.0
            #define RG_SPECULAR
            #define RG_SKINDEF
            #include_with_pragmas "../CommonVariants.hlsl"
            #include_with_pragmas "SkinInput.hlsl"
            #include_with_pragmas "../CommonSetup.hlsl"
            #include_with_pragmas "SkinForward.hlsl"
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
            #include "SkinInput.hlsl"
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
            #include "SkinInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
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
            Cull [_Cull]

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
            #include "SkinInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitDepthNormalsPass.hlsl"
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
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "SkinInput.hlsl"
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

            #include "SkinInput.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ObjectMotionVectors.hlsl"
            ENDHLSL
        }
    }
    Fallback  "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.BaseToonShader"
}