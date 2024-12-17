#ifndef TOON_SHADER_CBUFFER_INCLUDED
#define TOON_SHADER_CBUFFER_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Structures.hlsl"
#include "CustomLighting.hlsl"

CBUFFER_START(UnityPerMaterial)
    float _ShadowSharpness;
    float _SecondBandOffset;
    float _NdotLBias;
    float4 _LightTint;
    float3 _SSSPower;
    float3 _SSSOffset;
    float4 _ShadowColor;
    float _AutoSpecularColor;
    float4 _Shadow2Color;
    float _Shadow2ColorAuto;
    float4 _BaseColor;
    Texture2D _BaseMap;
    SamplerState sampler_BaseMap;
    float4 _BaseMap_ST;
    Texture2D _NormalMap;
    SamplerState sampler_NormalMap;
    float4 _NormalMap_ST;
    float _NormalStrength;
    Texture2D _MRFATex;
    SamplerState sampler_MRFATex;
    float4 _MRFATex_ST;
    float _Use_Metalness;
    int _metalGradCount;
    float4 _metalGradColors[8];
    float _Metalness;
    float _Use_Roughness;
    float _Roughness;
    float _Fresnel;
    float _SpecularAmount;
    float _SpecularPower;
    float4 _SpecularColor;
    float _Face_Rendering;
    float _IsSdf;
    Texture2D _SDFTex;
    float3 _FaceCenter;
    float3 _FaceFwdVec;
    float3 _FaceRightVec;
    float _ExpandVertices;
    float _ExpandAmount;
    float _ClothingLayersSeparation;
    float _ClothingLayer;
    float _Transparency;
    Texture2D _AlphaMap;
    SamplerState sampler_AlphaMap;
    float _UseAlphaForTransparency;
    float _AlphaClip;
    float _Cutoff;
    float _Surface;
CBUFFER_END

float CalculateAlpha(float2 uv) {
    float4 alphaTexSample = SAMPLE_TEXTURE2D(_AlphaMap, sampler_LinearRepeat, uv);
    float alpha;
    
    if(_UseAlphaForTransparency)
        alpha = alphaTexSample.a * _Transparency;
    else
        alpha = alphaTexSample.r * _Transparency;

    if(_AlphaClip)
    {
        clip(alpha - _Cutoff);
        alpha = SharpenAlpha(alpha, _Cutoff);
    }
    
    return OutputAlpha(alpha, _Surface);
}

FaceData SetupFaceData(float2 uv) {
    FaceData fdata;
    fdata.isFace = _Face_Rendering != 0;
    fdata.isSdf = _IsSdf != 0;
    #ifdef _SDF_SHADING
        fdata.sdfSample = SAMPLE_TEXTURE2D(_SDFTex, sampler_LinearClamp, uv);
    #else
        fdata.sdfSample = 0;
    #endif
    fdata.faceCenter = _FaceCenter;
    fdata.faceFwdVec = _FaceFwdVec;
    fdata.faceRgtVec = _FaceRightVec;
    return fdata;
}

DiffuseData SetupDiffuseData() {
    DiffuseData ddata;
    ddata.smooth = _ShadowSharpness;
    ddata.secBndOffset = _SecondBandOffset;
    ddata.NdotL = 0;
    ddata.SSSPower = _SSSPower;
    ddata.SSSOffset = _SSSOffset;
    ddata.NdotLBias = _NdotLBias;
    ddata.lightTint = _LightTint.rgb;
    ddata.auto2ndBndCol = _Shadow2ColorAuto != 0;
    ddata.firstBndCol = _ShadowColor.rgb;
    ddata.secBndCol = _Shadow2Color.rgb;
    ddata.auto2ndBndCol = _Shadow2ColorAuto != 0;
    ddata.shadowAttn = 0;
    return ddata;
}

GeometryData SetupGeometryData(float2 nrmUV, float3 posWS, float3 nrm, float4 tng, float4 lmuv) {
    float2 normalMapUV = TRANSFORM_TEX(nrmUV, _NormalMap);
    float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalMapUV);
    float3 unpackedNormal = normalize(lerp(float3(0,0,1), UnpackNormal(normalSample), _NormalStrength));
    GeometryData gdata;
    gdata.lgtDir = 0;
    gdata.posWs = posWS;
    gdata.nrmWs = NormalMapToWorld(unpackedNormal, nrm, tng);
    gdata.viewDir = GetWorldSpaceViewDir(posWS);
    gdata.mainLgtDir = 0;
    gdata.shadowMask = 0;
    gdata.lightmapUV = 0;
    OUTPUT_LIGHTMAP_UV(lmuv, unity_LightmapST, gdata.lightmapUV);

    #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
    gdata.shadowCoord = ComputeScreenPos(TransformWorldToHClip(posWS));
    #else
    gdata.shadowCoord = TransformWorldToShadowCoord(posWS);
    #endif
    
    return gdata;
}

SpecularData SetupSpecularData() {
    SpecularData sdata;
    sdata.specAmt = _SpecularAmount;
    sdata.specPow = _SpecularPower;
    sdata.specCol = _SpecularColor.xyz;
    sdata.autoCol = _AutoSpecularColor != 0;
    return sdata;
}

MetallicData SetupMetallicData(float2 uv) {
    float2 baseColUV = TRANSFORM_TEX(uv, _BaseMap);
    float3 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseColUV).rgb * _BaseColor.rgb;
    float2 MRFAUV = TRANSFORM_TEX(uv, _MRFATex);
    MetallicData mdata;
    mdata.useMetallic = _Use_Metalness != 0;
    mdata.metalness = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).r * _Metalness;
    mdata.baseCol = baseCol;
    mdata.gradient = GetMetallicGradient();
    return mdata;
}

RoughnessData SetupRoughnessData(float2 uv) {
    float2 MRFAUV = TRANSFORM_TEX(uv, _MRFATex);
    RoughnessData rdata;
    rdata.useRoughness = _Use_Roughness != 0;
    rdata.roughness = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).g * _Roughness;
    rdata.fresnelAmt = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).b * _Fresnel;
    return rdata;
}

#endif
