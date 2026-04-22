Shader "Toon/EyeShader"
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

        // FaceRelated
        [ToggleUI] _IsFace("Is Face", Float) = 0.0
        [ToggleUI] _FaceDbg("Face Debug", Float) = 0.0
        _SphereCenter("Sphere Center", Vector) = (0,0,0,0)
        _SphereRadius("Sphere Radius", Float) = 0.5

        // RGB Offset
        _RChSmooth("Red Channel Smoothness", Range(0, 1)) = 1
        _GChSmooth("Green Channel Smoothness", Range(0, 1)) = 1
        _BChSmooth("Blue Channel Smoothness", Range(0, 1)) = 1
        
        // OverlayMap
        _OverlayMap("Overlay Map", 2D) = "black" {}
        
        [hideInInspector]_MRFATex ("MRFA", 2D) = "white" {}
        [Toggle]_IsLeftEye("Is Left Eye", Float) = 1.0
        
        [Space(20)]
        [Header(Outer Iris)]
        [Toggle]_EnableOuterIris("Enable Outer Iris", Float) = 1.0
        [Toggle]_UseOuterIrisTexture("Use Outer Iris Texture", Float) = 0.0
        [KeywordEnum(None, Multiply, Colorize)]_OuterIrisColorMode("Outer Iris Color Mode", Float) = 0
        _OuterIrisTexture("Outer Iris Texture", 2D) = "white" {}
        [Space(10)]
        _OuterIrisColor1("Outer Iris Color 1", Color) = (0, 0, 0, 1)
        _OuterIrisColor2("Outer Iris Color 2", Color) = (0, 0, 0, 1)
        [KeywordEnum(Gradient X, Gradient Y, Gradient Circle)]_OuterIrisGradientType("Outer Iris Gradient Type", Float) = 2
        _OuterIrisGradientMidpoint("Outer Iris Gradient Midpoint", Range(0, 1)) = 0.5
        _OuterIrisGradientSharpness("Outer Iris Gradient Sharpness", Range(0, 1)) = 0.5
        _OuterIrisXPos("Outer Iris X Position", Range(0, 1)) = 0.5
        _OuterIrisYPos("Outer Iris Y Position", Range(0, 1)) = 0.5
        [Space(10)]
        _OuterIrisXScale("Outer Iris X Scale", Range(0, 1)) = 0.33
        _OuterIrisYScale("Outer Iris Y Scale", Range(0, 1)) = 0.5
        
        [Space(20)]
        [Header(Inner Iris)]
        [Toggle]_EnableInnerIris("Enable Inner Iris", Float) = 1.0
        [Toggle]_UseInnerIrisTexture("Use Inner Iris Texture", Float) = 0.0
        [KeywordEnum(None, Multiply, Colorize)]_InnerIrisColorMode("Inner Iris Color Mode", Float) = 0
        _InnerIrisTexture("Inner Iris Texture", 2D) = "white" {}
        [Space(10)]
        _InnerIrisColor1("Inner Iris Color 1", Color) = (0, 0, 0, 1)
        _InnerIrisColor2("Inner Iris Color 2", Color) = (0, 0, 0, 1)
        [KeywordEnum(Gradient X, Gradient Y, Gradient Circle)]_InnerIrisGradientType("Inner Iris Gradient Type", Float) = 2
        _InnerIrisGradientMidpoint("Inner Iris Gradient Midpoint", Range(0, 1)) = 0.5
        _InnerIrisGradientSharpness("Inner Iris Gradient Sharpness", Range(0, 1)) = 0.5
        _InnerIrisXPos("Inner Iris X Position", Range(0, 1)) = 0.5
        _InnerIrisYPos("Inner Iris Y Position", Range(0, 1)) = 0.5
        [Space(10)]
        _InnerIrisXScale("Inner Iris X Scale", Range(0, 2)) = 0.25
        _InnerIrisYScale("Inner Iris Y Scale", Range(0, 2)) = 0.4
        [Space(10)]
        [Toggle]_EnableIrisLines("Enable Iris Lines", Float) = 0.0
        _IrisLinesColor("Iris Lines Color", Color) = (0, 0, 0, 1)
        _IrisLinesCount("Iris Lines Count", Range(0, 1000)) = 300
        _IrisLinesWidth("Iris Lines Width", Range(0, 1)) = 0.3
        _IrisLinesAngleStart("Iris Lines Angle Start", Range(0, 1)) = 0.0
        _IrisLinesAngleEnd("Iris Lines Angle End", Range(0, 1)) = 1.0
        [Toggle]_IrisLinesInvertAngle("Iris Lines Invert Angle", Float) = 0.0
        _IrisLinesSeed("Iris Lines Seed", Integer) = 0

        [Space(20)]
        [Header(Pupil)]
        [Toggle]_EnablePupil("Enable Pupil", Float) = 1.0
        [Toggle]_UsePupilTexture("Use Pupil Texture", Float) = 0.0
        [KeywordEnum(None, Multiply, Colorize)]_PupilColorMode("Pupil Color Mode", Float) = 0
        _PupilTexture("Pupil Texture", 2D) = "white" {}
        [Space(10)]
        _PupilColor1("Pupil Color 1", Color) = (1, 0, 0, 1)
        _PupilColor2("Pupil Color 2", Color) = (0, 0, 0, 1)
        [KeywordEnum(Gradient X, Gradient Y, Gradient Circle)]_PupilGradientType("Pupil Gradient Type", Float) = 2
        _PupilGradientMidpoint("Pupil Gradient Midpoint", Range(0, 1)) = 0.5
        _PupilGradientSharpness("Pupil Gradient Sharpness", Range(0, 1)) = 0.5
        _PupilXPos("Pupil X Position", Range(0, 1)) = 0.5
        _PupilYPos("Pupil Y Position", Range(0, 1)) = 0.6
        [Space(10)]
        _PupilXScale("Pupil X Scale", Range(0, 1)) = 0.3
        _PupilYScale("Pupil Y Scale", Range(0, 1)) = 0.4
        
        [Space(20)]
        [Header(Lighting)]
        [Toggle]_EnableLighting("Enable Lighting", Float) = 1.0
        
        [Space(20)]
        [Header(Shadow)]
        [Toggle]_EnableShadow("Enable Shadow", Float) = 1.0
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
        _ShadowHeight("Shadow Height", Range(0, 1)) = 0.8
        _ShadowHorizontalOffset("Shadow Horizontal Offset", Range(0, 1)) = 0.4
        _ShadowCurve("Shadow Curve", Range(-1, 1)) = -0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 1)) = 0.0
        
        [Space(20)]
        [Header(Highlight)]
        [Space(10)]
        [Toggle]_EnableHighlight("Enable Highlight", Float) = 1.0
        [Toggle]_UseHighlightTexture("Use Highlight Texture", Float) = 0.0
        _HighlightTexture("Highlight Texture", 2D) = "white" {}
        [KeywordEnum(None, Multiply, Colorize)]_HighlightColorMode("Highlight Color Mode", Float) = 0
        _HighlightScale("Highlight Scale", Range(0.01, 1)) = 1.0
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
        [Toggle]_AutoHighlightColor("Auto Highlight Color", float) = 0.0
        [Space(10)]
        _HighlightDeadzone("Highlight Deadzone", Range(0, 1)) = 0.1
        _HighlightDeadzoneAttenuation("Highlight Deadzone Attenuation", Range(0, 1)) = 0.8
        [Space(10)]
        [Toggle]_EnableSecondaryHighlight("Enable Secondary Highlight", Float) = 1.0
        [Toggle]_UseSecondaryHighlightTexture("Use Secondary Highlight Texture", Float) = 0.0
        _SecondaryHighlightTexture("Secondary Highlight Texture", 2D) = "white" {}
        [KeywordEnum(None, Multiply, Colorize)]_SecondaryHighlightColorMode("Secondary Highlight Color Mode", Float) = 0
        _SecondaryHighlightScale("Secondary Highlight Scale", Range(0.01, 1)) = 1.0
        [Toggle]_SecondaryHighlightMirrorX("Secondary Highlight Mirror X", Float) = 1.0
        [Toggle]_SecondaryHighlightMirrorY("Secondary Highlight Mirror Y", Float) = 1.0
        
        [Space(20)]
        [Header(Teary)]
        [Space(10)]
        [Toggle]_IsTeary("Is Teary", Float) = 0.0
        _TearySpeed("Teary Speed", Range(0,1)) = .4
        _TearyAmount("Teary Amount", Range(0.001, 0.1)) = 0.05
        
        _FaceCenter("Face Center", Vector) = (0, 0, 0, 1)
        _FaceFwdVec("Face Forward Vector", Vector) = (0, 0, 0, 1)
        _PerlinTex("Perlin Texture", 2D) = "white" {}
        
        _MovementX("Horizontal Movement", Range(-0.7, 0.55)) = 0.0
        _MovementY("Vertical Movement", Range(-0.4, 0.57)) = 0.0
        
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
            Name "EyeForwardLit"
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
            #define RG_EYE
            #include_with_pragmas "../ToonV3/CommonVariants.hlsl"
            #include_with_pragmas "EyeInput.hlsl"
            #include_with_pragmas "../ToonV3/CommonSetup.hlsl"
            #include_with_pragmas "EyeForward.hlsl"
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
            #include "EyeInput.hlsl"
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
            #include "EyeInput.hlsl"
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
            //#define _NORMALMAP 1
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
            #include "EyeInput.hlsl"
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
            //#define _EMISSION 1
            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "EyeInput.hlsl"
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

            #include "EyeInput.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ObjectMotionVectors.hlsl"
            ENDHLSL
        }
    }
    Fallback  "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.BaseToonShader"
}