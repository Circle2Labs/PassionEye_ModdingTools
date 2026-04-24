#ifndef METAL_INPUT_HLSL
#define METAL_INPUT_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
#include "../../Utils.hlsl"

GLOBAL_CBUFFER_START(ToonGlobalBuffer, b0)
// light smoothing
float _LightSmooth;
//Magic number for the minimum value of the light
float _LightMin;
float _MidPoint;

// hue shifting
float _ShiftAmount;
float _kelvinTemp; //0-20000
CBufTexture(_DayNightRamp);
float _DNTintStr;
CBUFFER_END

TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

CBUFFER_START (UnityPerMaterial)
float4 _BaseColor;
// Main Texture
float4 _BaseMap_ST;
float4 _BaseMap_TexelSize;
// Normal Map
float _NormalStrength;
float4 _NormalMap_ST;
float4 _NormalMap_TexelSize;
// Needed for META pass
float4 _EmissionColor;
// Metalness
float4 _MetallicColor;
float _MetallicStrength;
float _MetallicRoughness;
float _MetallicReflection;
// Clothing separation
float _ClothingLayer;
float _ClothingLayersSeparation;
// ALPHA Values
float _UseAlphaForTransparency;
float _AlphaClip;
float _Cutoff;
float _Surface;
// Needed to make the debug macros work
UNITY_TEXTURE_STREAMING_DEBUG_VARS;
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _NormalStrength)
    UNITY_DOTS_INSTANCED_PROP(float4, _NormalMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _NormalMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _MetallicColor)
    UNITY_DOTS_INSTANCED_PROP(float, _MetallicStrength)
    UNITY_DOTS_INSTANCED_PROP(float, _MetallicRoughness)
    UNITY_DOTS_INSTANCED_PROP(float, _MetallicReflection)
    UNITY_DOTS_INSTANCED_PROP(float, _ClothingLayer)
    UNITY_DOTS_INSTANCED_PROP(float, _ClothingLayersSeparation)
    UNITY_DOTS_INSTANCED_PROP(float, _UseAlphaForTransparency)
    UNITY_DOTS_INSTANCED_PROP(float, _AlphaClip)
    UNITY_DOTS_INSTANCED_PROP(float, _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float, _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

static float4  unity_DOTS_Sampled_BaseColor;
static float4 unity_DOTS_Sampled_BaseMap_ST;
static float4 unity_DOTS_Sampled_BaseMap_TexelSize;
static float  unity_DOTS_Sampled_NormalStrength;
static float4 unity_DOTS_Sampled_NormalMap_ST;
static float4 unity_DOTS_Sampled_NormalMap_TexelSize;
static float4 unity_DOTS_Sampled_EmissionColor;
static float4 unity_DOTS_Sampled_MetallicColor;
static float  unity_DOTS_Sampled_MetallicStrength;
static float  unity_DOTS_Sampled_MetallicRoughness;
static float  unity_DOTS_Sampled_MetallicReflection;
static float  unity_DOTS_Sampled_ClothingLayer;
static float  unity_DOTS_Sampled_ClothingLayersSeparation;
static float  unity_DOTS_Sampled_UseAlphaForTransparency;
static float  unity_DOTS_Sampled_AlphaClip;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Surface;

void SetupDOTSToonMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseColor);
    unity_DOTS_Sampled_BaseMap_ST = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseMap_ST);
    unity_DOTS_Sampled_BaseMap_TexelSize = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseMap_TexelSize);
    unity_DOTS_Sampled_NormalStrength = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _NormalStrength);
    unity_DOTS_Sampled_NormalMap_ST = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _NormalMap_ST);
    unity_DOTS_Sampled_NormalMap_TexelSize = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _NormalMap_TexelSize);
    unity_DOTS_Sampled_EmissionColor = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _EmissionColor);
    unity_DOTS_Sampled_MetallicColor = UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _MetallicColor);
    unity_DOTS_Sampled_MetallicStrength = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _MetallicStrength);
    unity_DOTS_Sampled_MetallicRoughness = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _MetallicRoughness);
    unity_DOTS_Sampled_MetallicReflection = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _MetallicReflection);
    unity_DOTS_Sampled_ClothingLayer = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _ClothingLayer);
    unity_DOTS_Sampled_ClothingLayersSeparation = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _ClothingLayersSeparation);
    unity_DOTS_Sampled_UseAlphaForTransparency = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _UseAlphaForTransparency);
    unity_DOTS_Sampled_AlphaClip = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _AlphaClip);
    unity_DOTS_Sampled_Cutoff = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _Cutoff);
    unity_DOTS_Sampled_Surface = UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _Surface);
}

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSToonMaterialPropertyCaches()

#define _BaseColor               unity_DOTS_Sampled_BaseColor
#define _BaseMap_ST              unity_DOTS_Sampled_BaseMap_ST
#define _BaseMap_TexelSize       unity_DOTS_Sampled_BaseMap_TexelSize
#define _NormalStrength          unity_DOTS_Sampled_NormalStrength
#define _NormalMap_ST            unity_DOTS_Sampled_NormalMap_ST
#define _NormalMap_TexelSize     unity_DOTS_Sampled_NormalMap_TexelSize
#define _EmissionColor           unity_DOTS_Sampled_EmissionColor
#define _MetallicColor           unity_DOTS_Sampled_MetallicColor
#define _MetallicStrength        unity_DOTS_Sampled_MetallicStrength
#define _MetallicRoughness       unity_DOTS_Sampled_MetallicRoughness
#define _MetallicReflection      unity_DOTS_Sampled_MetallicReflection
#define _UseAlphaForTransparency unity_DOTS_Sampled_UseAlphaForTransparency
#define _AlphaClip              unity_DOTS_Sampled_AlphaClip
#define _Cutoff                 unity_DOTS_Sampled_Cutoff
#define _Surface                unity_DOTS_Sampled_Surface

#endif

void InitializeBaseSurfaceData(float2 uv, out SurfaceData surfaceData)
{
    surfaceData = (SurfaceData)0;
    float4 mainTex = RG_TEX_SAMPLE(_BaseMap, uv);
    float4 mainAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    surfaceData.alpha = _BaseColor.a * mainAlpha.a;
    surfaceData.alpha = AlphaDiscard(surfaceData.alpha, _Cutoff);
    
    surfaceData.albedo = _BaseColor.rgb * mainTex.rgb;
    surfaceData.albedo = AlphaModulate(surfaceData.albedo, surfaceData.alpha);
    
    float3 normalTS = UnpackNormal(RG_TEX_SAMPLE(_NormalMap, uv));
    normalTS = normalize(lerp(float3(0, 0, 1), normalTS, _NormalStrength));
    surfaceData.normalTS = normalTS;
    
    surfaceData.occlusion = 1.0;
    
    surfaceData.emission = 0;
    
    surfaceData.specular = _MetallicColor.rgb;
    surfaceData.smoothness = _MetallicRoughness;
    surfaceData.metallic = _MetallicStrength;
}

#endif