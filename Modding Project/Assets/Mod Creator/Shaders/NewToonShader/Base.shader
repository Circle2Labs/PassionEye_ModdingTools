Shader "Toon/Base"
{
    Properties
    {
        // Main color
        _TintColor("Main Color", Color) = (1,1,1,1)
        [MainTexture]_MainTex("Base Color", 2D) = "white" {}

        // Normal map
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 1

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
            Name "BaseToon"

            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
            }

            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]

            HLSLPROGRAM
            #include_with_pragmas "BaseForward.hlsl"
            ENDHLSL
        }
        UsePass "Toon/ToonShader/ShadowCaster"
        UsePass "Toon/ToonShader/DepthOnly"
        UsePass "Toon/ToonShader/DepthNormals"
        Pass
        {
            Name "META"
            Tags
            {
                "LightMode"="Meta"
            }
            Cull Off
            HLSLPROGRAM
            #include_with_pragmas "Meta.hlsl"
            ENDHLSL
        }
    }
    //FallBack "Hidden/Core/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.ToonShaderGUI"
}