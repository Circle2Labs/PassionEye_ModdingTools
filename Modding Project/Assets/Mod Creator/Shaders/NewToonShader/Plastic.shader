Shader "Toon/Plastic"
{
    Properties
    {
        // Main color
        _TintColor("Main Color", Color) = (1,1,1,1)
        _MainTex("Base Color", 2D) = "white" {}

        // Normal map
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 1

        // Specularity
        [PowerSlider(1000)]_SpecularPower("Specular Power", Range(1, 1000)) = 50
        _SpecularAmount("Specular Amount", Range(0, 2)) = 1
        _SpecularColor("Specular Color", Color) = (1,1,1,1)

        // rim light
        _RimLightColor("Rim Light Color", Color) = (1,1,1,1)
        _RimLightAmount("Rim Light Amount", Range(0.0, 1)) = 1.0
        _RimLightPower("Rim Light Power", Range(0, 30)) = 5

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // Clothing layers
        _ClothingLayersSeparation ("Clothing Layers Separation", Range(0, 0.01)) = 0.003
        _ClothingLayer ("Clothing Layer", float) = 0

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
                "RenderPipeline"="UniversalPipeline"
            }

            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]

            HLSLPROGRAM
            #include_with_pragmas "PlasticForward.hlsl"
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