#ifndef BASE_SHADOW_CASTER
#define BASE_SHADOW_CASTER

#include_with_pragmas "./CommonInclude.hlsl"
#pragma vertex vert
#pragma fragment frag

// Shadow Casting Light geometric parameters.
// These variables are used when applying the shadow Normal Bias and are set
// by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer
// in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different
// at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;
CBUFFER_START(UnityPerMaterial)
CBufTexture(_MainTex);
float _AlphaClip;
float _Cutoff;
float _Surface;
CBUFFER_END

struct v {
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
};

struct v2f {
    #if defined(_ALPHATEST_ON)
    float2 uv       : TEXCOORD0;
    #endif
    float4 positionCS   : SV_POSITION;
};

float4 GetShadowPositionHClip(v input){
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
    float3 lightDirectionWS = _LightDirection;
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    positionCS = ApplyShadowClamping(positionCS);
    return positionCS;
}

void vert(v input, out v2f output) {
    #ifdef _ALPHATEST_ON
    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    #endif
    output.positionCS = GetShadowPositionHClip(input);
}

void frag(v2f input, out half4 shadow : SV_TARGET) {

    #ifdef _ALPHATEST_ON
    float4 alphaTexSample = RG_TEX_SAMPLE(_MainTex, input.uv);
    CalculateAlpha(alphaTexSample.a, _AlphaClip, _Cutoff, _Surface);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
    LODFadeCrossFade(input.positionCS);
    #endif

    shadow = 0;
}
#endif