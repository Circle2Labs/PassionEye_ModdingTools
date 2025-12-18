Shader "Toon/Face"
{
    Properties
    {
        [Toggle]_EnableCameraLight("Enable Camera Light", Float) = 1
        _CameraLightColor("Camera Light Color", Color) = (1,1,1,1)
        _CameraLightSmooth("Camera Light Smoothing", Range(0.0001,1)) = 0.3
        _CameraLightMidPoint("Camera Light Mid Point", Range(0,1)) = 0.25

        // SSS
        _SSSShadowColor("SSS Shadow Color", Color) = (.5, .04, 0, 1)
        _SSSTransitionColor("SSS Transition Color", Color) = (1, .08, 0, 1)

        // Specularity
        [PowerSlider(1000)]_SpecularPower("Specular Power", Range(1, 1000)) = 50
        _SpecularAmount("Specular Amount", Range(0, 2)) = 1
        _SpecularColor("Specular Color", Color) = (1,1,1,1)

        // Main color
        _TintColor("Main Color", Color) = (1,1,1,1)
        _MainTex("Base Color", 2D) = "white" {}

        // Normal map
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 1

        // SDF face map
        _FaceShadowTex("Face Shadow Texture", 2D) = "white" {}
        [PowerSlider(2)]_FaceShadowSmooth("Face Shadow Smoothing", Range(0, .25)) = 0.1

        // Face vectors
        _FaceForward("Face Forward", Vector) = (0, 0, 0, 0)
        _FaceRight("Face Right", Vector) = (0, 0, 0, 0)
        _FaceUp("Face Up", Vector) = (0, 0, 0, 0)

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
    SubShader
    {
        Pass
        {
            Name "BaseToonSkin"

            Tags
            {
                "RenderPipeline"="UniversalPipeline"
            }

            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]

            HLSLPROGRAM
            #include_with_pragmas "FaceForward.hlsl"
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
            #include_with_pragmas "BaseShadowCaster.hlsl"
            ENDHLSL
        }
        Pass {
            Name "SkinDepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On //Write depth
            ZTest [_ZTest]
            Cull [_Cull]
            ColorMask 0 //Don't output any color
            
            HLSLPROGRAM
            #include_with_pragmas "SkinDepthOnly.hlsl"
            ENDHLSL
        }
        Pass {
            Name "SkinDepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            ZTest [_ZTest]
            Cull [_Cull]
            
            HLSLPROGRAM
            #include_with_pragmas "SkinDepthNormals.hlsl"
            ENDHLSL
        }
        UsePass "Toon/Base/META"
    }
    //FallBack "Hidden/Core/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.ToonShaderGUI"
}