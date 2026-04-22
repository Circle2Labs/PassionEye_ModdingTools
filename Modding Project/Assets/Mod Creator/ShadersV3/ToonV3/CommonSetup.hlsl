#ifndef COMMON_SETUP_HLSL
#define COMMON_SETUP_HLSL

#include "CommonInclude.hlsl"

struct Attributes {
    float4 positionOS        : POSITION;
    float3 normalOS          : NORMAL;
    float4 tangentOS         : TANGENT;
    float2 texCoords         : TEXCOORD0;
    float2 staticLightmapUV  : TEXCOORD1;
    float2 dynamicLightmapUV : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GeomData
{
    float4 vertexOS             : POSITION;
    float3 normalOS             : NORMAL;
    float4 tangentOS            : TANGENT;
    float2 texCoords            : TEXCOORD0;
    float2 staticLightmapUV     : TEXCOORD1;
    float2 dynamicLightmapUV    : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float2 uv                      : TEXCOORD0;
    
    float3 positionWS              : TEXCOORD1;    // xyz: posWS

    #ifdef _NORMALMAP
    half4 normalWS                 : TEXCOORD2;    // xyz: normal, w: viewDir.x
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: viewDir.y
    half4 bitangentWS              : TEXCOORD4;    // xyz: bitangent, w: viewDir.z
    #else
    half3  normalWS                : TEXCOORD2;
    #endif
    
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
    #else
    half  fogFactor               : TEXCOORD5;
    #endif
    
    #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    float4 shadowCoord            : TEXCOORD6;
    #endif
    
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
    
    #ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV     : TEXCOORD8;
    #endif

    #ifdef USE_APV_PROBE_OCCLUSION
    float4 probeOcclusion         : TEXCOORD9;
    #endif
    
    // Clip space position
    float4 positionCS             : SV_POSITION;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    
    #ifdef RG_FUR
    float layerDepth              : ATTRIB0;
    #endif
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData) {
    inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    #if defined(DEBUG_DISPLAY)
    inputData.positionCS = input.positionCS;
    #endif

    #ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
    inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
    #else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
    inputData.normalWS = input.normalWS;
    #endif
    
    inputData.viewDirectionWS = viewDirWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    #else
    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
    inputData.vertexLighting = half3(0, 0, 0);
    #endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #if defined(USE_APV_PROBE_OCCLUSION)
    inputData.probeOcclusion = input.probeOcclusion;
    #endif
    #endif
}

void InitializeBakedGIData(Varyings input, inout InputData inputData)
{
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
    #elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(input.vertexSH,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        input.positionCS.xy,
        input.probeOcclusion,
        inputData.shadowMask);
    #else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
    #endif
}

StylizationData InitializeBaseStylizationData() {
    StylizationData stylizationData;
    stylizationData.lightMin        = _LightMin;
    stylizationData.midpoint        = _MidPoint;
    stylizationData.lightSmooth     = _LightSmooth;
    stylizationData.lightTint       = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.75));
    stylizationData.shadowTint      = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.25));
    stylizationData.tintStrength    = _DNTintStr;
    return stylizationData;
}

#endif