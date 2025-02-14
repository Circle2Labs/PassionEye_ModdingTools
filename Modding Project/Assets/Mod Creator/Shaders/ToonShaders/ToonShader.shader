Shader "Toon/ToonShader" {
    Properties {
        [Space]
        _ShadowSharpness ("Light Smooth", Range(0, 1)) = 0.1
        _SecondBandOffset ("Second Band Offset", Range(0, 1)) = 0.1
        _NdotLBias ("NdotL Bias", Range(-1, 1)) = 0
        _SSSPower ("SubSurface Scattering Power", Vector) = (1,1,1)
        _SSSOffset ("SubSurface Scattering Offset", Vector) = (0,0,0)
        [Space]
        _LightTint ("Light Tint", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        [Toggle] _Shadow2ColorAuto ("Automatic Penumbra Color", float) = 0
        _Shadow2Color ("Penumbra Color", Color) = (0.25, 0.25, 0.25, 1)
        [Space]
        [MainColor] _BaseColor ("Tint Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap ("Main Color (RGB)", 2D) = "white" {}
        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 0.5
        [Space]
        _MRFATex ("Metalness, Roughness, Fresnel", 2D) = "white" {}
        [Toggle] _Use_Metalness ("Use Metalness", float) = 0
        _Metalness ("Metalness", Range(0, 1)) = 0 //texture
        [Space]
        [Toggle] _Use_Roughness ("Use Roughness", float) = 0
        _Roughness ("Roughness", Range(0, 1)) = 0.5 //texture
        _Fresnel ("Fresnel Amount", Range(0, 1)) = 0
        [Space]
        _SpecularAmount ("Specular Amount", Range(0, 1)) = 0
        [PowerSlider(1.5)] _SpecularPower ("Specular Power", Range(0.01, 100)) = 0.1
        [Toggle] _AutoSpecularColor ("Automatic Specular Color", float) = 0
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        [Space]
        [Toggle] _Face_Rendering ("Face Rendering Fix", float) = 0
        [Toggle] _IsSdf ("Use SDF for Face Rendering", float) = 0
        _SDFTex ("SDF Texture", 2D) = "white" {}
        _FaceCenter ("Face Center", Vector) = (0, 0, 0, 0)
        _FaceFwdVec ("Face Forward Vector", Vector) = (0, 0, 0, 0)
        _FaceRightVec ("Face Right Vector", Vector) = (0, 0, 0, 0)
        [Space]
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        [Toggle] _UseAlphaForTransparency ("Use Alpha Channel for Transparency", float) = 0
        _Transparency ("Alpha Intensity", Range(0, 1)) = 0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Space]
        _CensorPartTex ("Censor Texture", 2D) = "black" {}
        [Space]
        [Toggle] _ExpandVertices ("Expand Vertices", float) = 0
        _ExpandAmount ("Expand Amount", Range(0, 0.001)) = 0
        _ClothingLayersSeparation ("Clothing Layers Separation", Range(0, 0.01)) = 0.003
        _ClothingLayer ("Clothing Layer", float) = 0
        [Space]
        [Toggle] _HairHighlightToggle ("Enable Hair Highlight", float) = 0
        _HairHighlightColor ("Hair Highlight Color", Color) = (1, 1, 1, 1)
        _HairHighlightStrength ("Hair Highlight Strength", Range(0, 1)) = 1.0
        _HairHighlightExponent ("Hair Highlight Exponent", Range(0, 20)) = 5.0
        _HairHighlightLength ("Hair Highlight Length", Range(0, 1.5)) = 1.0
        [Space]
        _HairHighlightNoiseStrength ("Hair Highlight Noise Strength", Range(0.001, 3)) = 0.15
        _HairHighlightNoiseStretch ("Hair Highlight Noise Stretch", Range(0.001, 50)) = 3.0
        _HairHighlightNoiseSeed ("Hair Highlight Noise Seed", Range(0, 9999999)) = 1234
        [Space]
        _HairHighlightFresnelPower ("Hair Highlight Fresnel Power", Range(0.001, 100)) = 1.0
        _HairHighlightFresnelBias ("Hair Highlight Fresnel Bias", Range(0.7, 1.0)) = 0.9
        [Space]
        _PerlinNoiseTex ("Perlin Noise Texture", 2D) = "black" {}
        
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
            Name "ToonForward"
            
            Tags { 
                "RenderPipeline"="UniversalPipeline"
            }
            
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]
                        
            HLSLPROGRAM
            #include "ToonForward.hlsl"
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
            #include_with_pragmas "ToonShadowCaster.hlsl"
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
            #include "ToonDepthOnly.hlsl"
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
            
            #include "ToonDepthNormals.hlsl"
            
            ENDHLSL
        }
        //UsePass "Universal Render Pipeline/Lit/META"
    }
    //FallBack "Hidden/Core/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.ToonShaderGUI"
}
