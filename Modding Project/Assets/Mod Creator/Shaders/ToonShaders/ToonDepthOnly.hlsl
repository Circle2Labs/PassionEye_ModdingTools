#ifndef TOON_DEPTH_ONLY
#define TOON_DEPTH_ONLY

#pragma target 2.0

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#include "CustomLighting.hlsl"

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#pragma vertex vert
#pragma fragment frag

#pragma multi_compile _ _ALPHATEST_ON
#pragma multi_compile _ _SURFACE_TYPE_TRANSPARENT
#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
#pragma multi_compile_instancing

struct v {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct v2f {
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

#include "ToonShaderCBuffer.hlsl"

v2f vert(v input) {
    v2f output;
    output.pos = TransformObjectToHClip(input.positionOS);
    output.uv = input.uv;
    return output;
}

float frag(v2f input) : SV_Target {
    float2 MRFAUV = TRANSFORM_TEX(input.uv, _MRFATex);
    float alpha = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).a * _Transparency;
    AlphaDiscard(alpha, _Cutoff);
    return 1;
}

#endif