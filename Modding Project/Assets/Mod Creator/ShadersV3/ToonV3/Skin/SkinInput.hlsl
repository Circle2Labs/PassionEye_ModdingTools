#ifndef SKIN_INPUT_HLSL
#define SKIN_INPUT_HLSL

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

TEXTURE2D(_NormalMap);   SAMPLER(sampler_NormalMap);
TEXTURE2D(_SpecularMap); SAMPLER(sampler_SpecularMap);
TEXTURE2D(_OverlayMap);  SAMPLER(sampler_OverlayMap);

CBUFFER_START (UnityPerMaterial)
float4 _BaseColor;
// Main Texture
float4 _BaseMap_ST;
float4 _BaseMap_TexelSize;
// Normal Map
float _NormalStrength;
float4 _NormalMap_ST;
float4 _NormalMap_TexelSize;
// Face Values
bool _IsFace;
float3 _SphereCenter;
float _SphereRadius;
float _FaceDbg;
// RGB channel smooth
float _RChSmooth;
float _GChSmooth;
float _BChSmooth;
// Specular Map
float4 _SpecularColor;
float4 _SpecularMap_ST;
float4 _SpecularMap_TexelSize;
float _SpecularPower;
float _SpecularAmount;
// OverlayMap
float4 _OverlayMap_ST;
float4 _OverlayMap_TexelSize;
//Emission
float4 _EmissionColor;
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
    UNITY_DOTS_INSTANCED_PROP(bool, _IsFace)
    UNITY_DOTS_INSTANCED_PROP(float3, _SphereCenter)
    UNITY_DOTS_INSTANCED_PROP(float, _SphereRadius)
    UNITY_DOTS_INSTANCED_PROP(float, _RChSmooth)
    UNITY_DOTS_INSTANCED_PROP(float, _GChSmooth)
    UNITY_DOTS_INSTANCED_PROP(float, _BChSmooth)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _SpecularPower)
    UNITY_DOTS_INSTANCED_PROP(float, _SpecularAmount)
    UNITY_DOTS_INSTANCED_PROP(float4, _OverlayMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _OverlayMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
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
static bool   unity_DOTS_Sampled_IsFace;
static float3 unity_DOTS_Sampled_SphereCenter;
static float  unity_DOTS_Sampled_SphereRadius;
static float  unity_DOTS_Sampled_RChSmooth;
static float  unity_DOTS_Sampled_GChSmooth;
static float  unity_DOTS_Sampled_BChSmooth;
static float4 unity_DOTS_Sampled_SpecularColor;
static float4 unity_DOTS_Sampled_SpecularMap_ST;
static float4 unity_DOTS_Sampled_SpecularMap_TexelSize;
static float unity_DOTS_Sampled_SpecularPower;
static float unity_DOTS_Sampled_SpecularAmount;
static float4 unity_DOTS_Sampled_OverlayMap_ST;
static float4 unity_DOTS_Sampled_OverlayMap_TexelSize;
static float4  unity_DOTS_Sampled_EmissionColor;
static float  unity_DOTS_Sampled_UseAlphaForTransparency;
static float  unity_DOTS_Sampled_AlphaClip;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Surface;

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSToonMaterialPropertyCaches()

void SetupDOTSToonMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor               = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
    unity_DOTS_Sampled_BaseMap_ST              = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseMap_ST);
    unity_DOTS_Sampled_BaseMap_TexelSize       = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseMap_TexelSize);
    unity_DOTS_Sampled_NormalStrength          = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _NormalStrength);
    unity_DOTS_Sampled_NormalMap_ST            = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _NormalMap_ST);
    unity_DOTS_Sampled_NormalMap_TexelSize     = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _NormalMap_TexelSize);
    unity_DOTS_Sampled_IsFace                 = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(bool, _IsFace);
    unity_DOTS_Sampled_SphereCenter           = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float3, _SphereCenter);
    unity_DOTS_Sampled_SphereRadius           = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SphereRadius);
    unity_DOTS_Sampled_RChSmooth              = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RChSmooth);
    unity_DOTS_Sampled_GChSmooth              = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _GChSmooth);
    unity_DOTS_Sampled_BChSmooth              = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _BChSmooth);
    unity_DOTS_Sampled_SpecularColor              = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpecularColor);
    unity_DOTS_Sampled_SpecularMap_ST             = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpecularMap_ST);
    unity_DOTS_Sampled_SpecularMap_TexelSize      = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpecularMap_TexelSize);
    unity_DOTS_Sampled_SpecularPower             = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SpecularPower);
    unity_DOTS_Sampled_SpecularAmount      = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SpecularAmount);
    unity_DOTS_Sampled_OverlayMap_ST             = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _OverlayMap_ST);
    unity_DOTS_Sampled_OverlayMap_TexelSize      = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _OverlayMap_TexelSize);
    unity_DOTS_Sampled_EmissionColor           = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _EmissionColor);
    unity_DOTS_Sampled_UseAlphaForTransparency = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _UseAlphaForTransparency);
    unity_DOTS_Sampled_AlphaClip               = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _AlphaClip);
    unity_DOTS_Sampled_Cutoff                  = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Cutoff);
    unity_DOTS_Sampled_Surface                 = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Surface);
}


#define _BaseColor               unity_DOTS_Sampled_BaseColor
#define _BaseMap_ST              unity_DOTS_Sampled_BaseMap_ST
#define _BaseMap_TexelSize       unity_DOTS_Sampled_BaseMap_TexelSize
#define _NormalStrength          unity_DOTS_Sampled_NormalStrength
#define _NormalMap_ST            unity_DOTS_Sampled_NormalMap_ST
#define _NormalMap_TexelSize     unity_DOTS_Sampled_NormalMap_TexelSize
#define _IsFace                 unity_DOTS_Sampled_IsFace
#define _SphereCenter           unity_DOTS_Sampled_SphereCenter
#define _SphereRadius           unity_DOTS_Sampled_SphereRadius
#define _RChSmooth              unity_DOTS_Sampled_RChSmooth
#define _GChSmooth              unity_DOTS_Sampled_GChSmooth
#define _BChSmooth              unity_DOTS_Sampled_BChSmooth
#define _SpecularColor              unity_DOTS_Sampled_SpecularColor
#define _SpecularMap_ST             unity_DOTS_Sampled_SpecularMap_ST
#define _SpecularMap_TexelSize      unity_DOTS_Sampled_SpecularMap_TexelSize
#define _SpecularPower             unity_DOTS_Sampled_SpecularPower
#define _SpecularAmount      unity_DOTS_Sampled_SpecularAmount
#define _OverlayMap_ST             unity_DOTS_Sampled_OverlayMap_ST
#define _OverlayMap_TexelSize      unity_DOTS_Sampled_OverlayMap_TexelSize
#define _EmissionColor           unity_DOTS_Sampled_EmissionColor
#define _UseAlphaForTransparency unity_DOTS_Sampled_UseAlphaForTransparency
#define _AlphaClip               unity_DOTS_Sampled_AlphaClip
#define _Cutoff                  unity_DOTS_Sampled_Cutoff
#define _Surface                 unity_DOTS_Sampled_Surface

#endif

void InitializeBaseSurfaceData(float2 uv, out SurfaceData surfaceData) {
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
    surfaceData.specular = RG_TEX_SAMPLE(_SpecularMap, uv).r * _SpecularColor * _SpecularAmount;
    surfaceData.smoothness = saturate(_SpecularPower);
    
    surfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

#endif