#ifndef HAIR_INPUT_HLSL
#define HAIR_INPUT_HLSL

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
TEXTURE2D(_PerlinNoise); SAMPLER(sampler_PerlinNoise);

CBUFFER_START (UnityPerMaterial)
half4 _BaseColor;
// Main Texture
float4 _BaseMap_ST;
float4 _BaseMap_TexelSize;
// Normal Map
float _NormalStrength;
float4 _NormalMap_ST;
float4 _NormalMap_TexelSize;
// Needed for META pass
float4 _EmissionColor;
// Anisotropic data
float2 _AnisoUV;
float _AnisoPower;
float4 _PerlinNoise_ST;
float4 _PerlinNoise_TexelSize;
float _LowFreqScale;
float _LowFreqStrenght;
float _MidFreqScale;
float _MidFreqStrenght;
float _HighFreqScale;
float _HighFreqStrenght;
float _HighlightContrast;
float4 _HighlightTint;
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
    UNITY_DOTS_INSTANCED_PROP(half4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _UseAlphaForTransparency)
    UNITY_DOTS_INSTANCED_PROP(float, _AlphaClip)
    UNITY_DOTS_INSTANCED_PROP(float, _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float, _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

static half4 unity_DOTS_Sampled_BaseColor;
static float4 unity_DOTS_Sampled_BaseMap_ST;
static float4 unity_DOTS_Sampled_BaseMap_TexelSize;
static float  unity_DOTS_Sampled_UseAlphaForTransparency;
static float  unity_DOTS_Sampled_AlphaClip;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Surface;

void SetupDOTSToonMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _BaseColor);
    unity_DOTS_Sampled_BaseMap_ST = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _BaseMap_ST);
    unity_DOTS_Sampled_BaseMap_TexelSize = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _BaseMap_TexelSize);
    unity_DOTS_Sampled_UseAlphaForTransparency = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _UseAlphaForTransparency);
    unity_DOTS_Sampled_AlphaClip = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _AlphaClip);
    unity_DOTS_Sampled_Cutoff = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _Cutoff);
    unity_DOTS_Sampled_Surface = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _Surface);
}

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSToonMaterialPropertyCaches()

#define _BaseColor               unity_DOTS_Sampled_BaseColor
#define _BaseMap_ST              unity_DOTS_Sampled_BaseMap_ST
#define _BaseMap_TexelSize       unity_DOTS_Sampled_BaseMap_TexelSize
#define _UseAlphaForTransparency unity_DOTS_Sampled_UseAlphaForTransparency
#define _AlphaClip               unity_DOTS_Sampled_AlphaClip
#define _Cutoff                  unity_DOTS_Sampled_Cutoff
#define _Surface                 unity_DOTS_Sampled_Surface

#endif

void InitializeBaseSurfaceData(float2 uv, out SurfaceData surfaceData) {
    surfaceData = (SurfaceData)0;
    half4 mainTex = RG_TEX_SAMPLE(_BaseMap, uv);
    half4 mainAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    surfaceData.alpha = _BaseColor.a * mainAlpha.a;
    surfaceData.alpha = AlphaDiscard(surfaceData.alpha, _Cutoff);
    
    surfaceData.albedo = _BaseColor.rgb * mainTex.rgb;
    surfaceData.albedo = AlphaModulate(surfaceData.albedo, surfaceData.alpha);
    
    surfaceData.normalTS = normalize(half3(0, 0, 1));
    
    surfaceData.occlusion = 1.0;
    
    surfaceData.emission = 0;
}

#endif