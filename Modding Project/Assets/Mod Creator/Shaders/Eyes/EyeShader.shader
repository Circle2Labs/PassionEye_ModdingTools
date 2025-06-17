Shader "Toon/EyeShader"
{
    Properties
    {
        [hideInInspector]_MRFATex ("MRFA", 2D) = "white" {}
        [Toggle]_IsLeftEye("Is Left Eye", Float) = 1.0
        [MainTexture]_MainTexture("Main Texture", 2D) = "white" {}
        _BackgroundColor("Background Color", Color) = (1, 1, 1, 1)
        
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
        _MinimumLightLevel("Minimum Light Level", Range(0, 1)) = 0.5
        
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
        
        _LightSmooth("Light Smoothing", Range(0.0001,1)) = 0.1
        _LightMin("Light Min", Range(0,0.999)) = 0.25
        _MidPoint("Mid Point", Range(0,1)) = 0.25
        
        [HideInInspector] _ZWrite("__zwrite", Float) = 1.0
        [HideInInspector] _Cull("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        ZTest [_ZTest]
        
        Pass
        {
            HLSLPROGRAM
            #include "EyeForward.hlsl"
            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            
            ZWrite [_ZWrite]
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "../ToonShaders/ToonShadowCaster.hlsl"
            ENDHLSL
        }

        Pass {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite [_ZWrite]
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma fragment frag
            #pragma vertex vert
            #include "../ToonShaders/ToonDepthOnly.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
            
            ZWrite [_ZWrite]
            Cull[_Cull]
            
            HLSLPROGRAM
            #pragma fragment frag
            #pragma vertex vert
            #include "../ToonShaders/ToonDepthNormals.hlsl"
            ENDHLSL
        }
    }
}
