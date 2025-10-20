Shader "Toon/Skin"
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

        // Alpha map
        [HideInInspector]_AlphaMap("Alpha Map", 2D) = "white" {}

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
            #include_with_pragmas "SkinForward.hlsl"
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
            #include_with_pragmas "../ToonShaders/ToonShadowCaster.hlsl"
            ENDHLSL
        }
        // TODO: usepass for the shadowcaster creates a bunch of errors but the shader works fine. Copy the passes over manually
        //UsePass "Toon/Base/ShadowCaster"
        UsePass "Toon/Base/DepthOnly"
        UsePass "Toon/Base/DepthNormals"
        UsePass "Toon/Base/META"
    }
    //FallBack "Hidden/Core/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.ToonShaderGUI"
}