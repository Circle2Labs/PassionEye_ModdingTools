#ifndef TOON_SHADOWCASTER
#define TOON_SHADOWCASTER

#pragma vertex vert
#pragma fragment frag

#pragma multi_compile _ _ALPHATEST_ON
#pragma multi_compile _ _SURFACE_TYPE_TRANSPARENT
#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#include "CustomLighting.hlsl"

//biases
float3 _LightDirection;
float3 _LightPosition;

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
    output.pos = GetShadowPositionHClip(input.positionOS, input.normalOS, _LightPosition, _LightDirection);
    output.uv = input.uv;
    
    return output;
}

float4 frag(v2f input) : SV_TARGET {
    // Sample the alpha mask texture
    float4 alphaMapSample = SAMPLE_TEXTURE2D(_AlphaMap, sampler_LinearClamp, input.uv);

    float alpha;
    
    if(_UseAlphaForTransparency)
        alpha = alphaMapSample.a * _Transparency;
    else
        alpha = alphaMapSample.r * _Transparency;

    if(_AlphaClip)
    {
        clip(alpha - _Cutoff);
        alpha = SharpenAlpha(alpha, _Cutoff);
    }
    
    #if defined(_ALPHATEST_ON) || defined(_SURFACE_TYPE_TRANSPARENT)
        //if we didn't already clip previously, we do the same checks
        if(_AlphaClip == 0){
            clip(alpha - _Cutoff);
            alpha = SharpenAlpha(alpha, _Cutoff);
        }
    #endif

    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(input.positionCS);
    #endif

    return 0;
}

#endif