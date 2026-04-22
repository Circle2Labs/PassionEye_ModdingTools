#ifndef CLOTHING_INPUT_HLSL
#define CLOTHING_INPUT_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
#include "../../Utils.hlsl"
#include "../../BlendModes.hlsl"

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
TEXTURE2D(_ClothFiberMap); SAMPLER(sampler_ClothFiberMap);
TEXTURE2D(_ClothFiberNormalMap); SAMPLER(sampler_ClothFiberNormalMap);
TEXTURE2D(_SpecularMap); SAMPLER(sampler_SpecularMap);

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
// Cloth Fiber Map
float _FiberStrenght;
float4 _ClothFiberMap_ST;
float4 _ClothFiberMap_TexelSize;
float4 _ClothFiberNormalMap_ST;
float4 _ClothFiberNormalMap_TexelSize;
float _Sheen;
float _SheenPower;
half4 _SheenColor;
// Specular Map
float _SpecularPower;
float _SpecularAmount;
float4 _SpecularColor;
float4 _SpecularMap_ST;
float4 _SpecularMap_TexelSize;
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
    UNITY_DOTS_INSTANCED_PROP(half4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _NormalStrength)
    UNITY_DOTS_INSTANCED_PROP(float4, _NormalMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _NormalMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _FiberStrenght)
    UNITY_DOTS_INSTANCED_PROP(float4, _ClothFiberMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _ClothFiberMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float4, _ClothFiberNormalMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _ClothFiberNormalMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _Sheen)
    UNITY_DOTS_INSTANCED_PROP(float, _SheenPower)
    UNITY_DOTS_INSTANCED_PROP(half4, _SheenColor)
    UNITY_DOTS_INSTANCED_PROP(float, _SpecularPower)
    UNITY_DOTS_INSTANCED_PROP(float, _SpecularAmount)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularMap_ST)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularMap_TexelSize)
    UNITY_DOTS_INSTANCED_PROP(float, _ClothingLayer)
    UNITY_DOTS_INSTANCED_PROP(float, _ClothingLayersSeparation)
    UNITY_DOTS_INSTANCED_PROP(float, _UseAlphaForTransparency)
    UNITY_DOTS_INSTANCED_PROP(float, _AlphaClip)
    UNITY_DOTS_INSTANCED_PROP(float, _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float, _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

static half4  unity_DOTS_Sampled_BaseColor;
static float4 unity_DOTS_Sampled_BaseMap_ST;
static float4 unity_DOTS_Sampled_BaseMap_TexelSize;
static float  unity_DOTS_Sampled_NormalStrength;
static float4 unity_DOTS_Sampled_NormalMap_ST;
static float4 unity_DOTS_Sampled_NormalMap_TexelSize;
static float  unity_DOTS_Sampled_FiberStrenght;
static float4 unity_DOTS_Sampled_ClothFiberMap_ST;
static float4 unity_DOTS_Sampled_ClothFiberMap_TexelSize;
static float4 unity_DOTS_Sampled_ClothFiberNormalMap_ST;
static float4 unity_DOTS_Sampled_ClothFiberNormalMap_TexelSize;
static float  unity_DOTS_Sampled_Sheen;
static float  unity_DOTS_Sampled_SheenPower;
static half4 unity_DOTS_Sampled_SheenColor;
static float  unity_DOTS_Sampled_SpecularPower;
static float  unity_DOTS_Sampled_SpecularAmount;
static float4 unity_DOTS_Sampled_SpecularColor;
static float4 unity_DOTS_Sampled_SpecularMap_ST;
static float4 unity_DOTS_Sampled_SpecularMap_TexelSize;
static float  unity_DOTS_Sampled_ClothingLayer;
static float  unity_DOTS_Sampled_ClothingLayersSeparation;
static float  unity_DOTS_Sampled_UseAlphaForTransparency;
static float  unity_DOTS_Sampled_AlphaClip;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Surface;

void SetupDOTSToonMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor =                  UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _BaseColor);
    unity_DOTS_Sampled_BaseMap_ST =                 UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _BaseMap_ST);
    unity_DOTS_Sampled_BaseMap_TexelSize =          UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _BaseMap_TexelSize);
    unity_DOTS_Sampled_NormalStrength =             UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _NormalStrength);
    unity_DOTS_Sampled_NormalMap_ST =               UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _NormalMap_ST);
    unity_DOTS_Sampled_NormalMap_TexelSize =        UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _NormalMap_TexelSize);
    unity_DOTS_Sampled_FiberStrenght =              UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _FiberStrenght);
    unity_DOTS_Sampled_ClothFiberMap_ST =           UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _ClothFiberMap_ST);
    unity_DOTS_Sampled_ClothFiberMap_TexelSize =    UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _ClothFiberMap_TexelSize);
    unity_DOTS_Sampled_ClothFiberNormalMap_ST =     UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _ClothFiberNormalMap_ST);
    unity_DOTS_Sampled_ClothFiberNormalMap_TexelSize = UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _ClothFiberNormalMap_TexelSize);
    unity_DOTS_Sampled_Sheen =                      UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _Sheen);
    unity_DOTS_Sampled_SheenPower =                 UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SheenPower);
    unity_DOTS_Sampled_SheenColor =                 UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SheenColor);
    unity_DOTS_Sampled_SpecularPower =              UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SpecularPower);
    unity_DOTS_Sampled_SpecularAmount =             UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SpecularAmount);
    unity_DOTS_Sampled_SpecularColor =              UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SpecularColor);
    unity_DOTS_Sampled_SpecularMap_ST =             UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SpecularMap_ST);
    unity_DOTS_Sampled_SpecularMap_TexelSize =      UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _SpecularMap_TexelSize);
    unity_DOTS_Sampled_ClothingLayer =              UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _ClothingLayer);
    unity_DOTS_Sampled_ClothingLayersSeparation =   UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _ClothingLayersSeparation);
    unity_DOTS_Sampled_UseAlphaForTransparency =    UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _UseAlphaForTransparency);
    unity_DOTS_Sampled_AlphaClip =                  UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _AlphaClip);
    unity_DOTS_Sampled_Cutoff =                     UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _Cutoff);
    unity_DOTS_Sampled_Surface =                    UNITY_ACCESS_DOTS_INSTANCED_PROP(MaterialPropertyMetadata, _Surface);
}

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSToonMaterialPropertyCaches()

#define _BaseColor               unity_DOTS_Sampled_BaseColor
#define _BaseMap_ST              unity_DOTS_Sampled_BaseMap_ST
#define _BaseMap_TexelSize       unity_DOTS_Sampled_BaseMap_TexelSize
#define _NormalStrength          unity_DOTS_Sampled_NormalStrength
#define _NormalMap_ST            unity_DOTS_Sampled_NormalMap_ST
#define _NormalMap_TexelSize     unity_DOTS_Sampled_NormalMap_TexelSize
#define _FiberStrenght           unity_DOTS_Sampled_FiberStrenght
#define _ClothFiberMap_ST        unity_DOTS_Sampled_ClothFiberMap_ST
#define _ClothFiberMap_TexelSize unity_DOTS_Sampled_ClothFiberMap_TexelSize
#define _ClothFiberNormalMap_ST  unity_DOTS_Sampled_ClothFiberNormalMap_ST
#define _ClothFiberNormalMap_TexelSize unity_DOTS_Sampled_ClothFiberNormalMap_TexelSize
#define _Sheen                   unity_DOTS_Sampled_Sheen
#define _SheenPower              unity_DOTS_Sampled_SheenPower
#define _SheenColor              unity_DOTS_Sampled_SheenColor
#define _SpecularPower           unity_DOTS_Sampled_SpecularPower
#define _SpecularAmount          unity_DOTS_Sampled_SpecularAmount
#define _SpecularColor           unity_DOTS_Sampled_SpecularColor
#define _SpecularMap_ST          unity_DOTS_Sampled_SpecularMap_ST
#define _SpecularMap_TexelSize   unity_DOTS_Sampled_SpecularMap_TexelSize
#define _ClothingLayer           unity_DOTS_Sampled_ClothingLayer
#define _ClothingLayersSeparation unity_DOTS_Sampled_ClothingLayersSeparation
#define _UseAlphaForTransparency unity_DOTS_Sampled_UseAlphaForTransparency
#define _AlphaClip               unity_DOTS_Sampled_AlphaClip
#define _Cutoff                  unity_DOTS_Sampled_Cutoff
#define _Surface                 unity_DOTS_Sampled_Surface

#endif

void InitializeBaseSurfaceData(float2 uv, out SurfaceData surfaceData)
{
    surfaceData = (SurfaceData)0;
    half4 mainTex = RG_TEX_SAMPLE(_BaseMap, uv);
    half4 fiberMap = RG_TEX_SAMPLE(_ClothFiberMap, uv);
    half4 mainAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    surfaceData.alpha = _BaseColor.a * mainAlpha.a;
    surfaceData.alpha = AlphaDiscard(surfaceData.alpha, _Cutoff);
    
    surfaceData.albedo = _BaseColor.rgb * mainTex.rgb;
    surfaceData.albedo = AlphaModulate(surfaceData.albedo, surfaceData.alpha);
    
    float3 normalTS = UnpackNormal(RG_TEX_SAMPLE(_NormalMap, uv));
    normalTS = normalize(lerp(float3(0, 0, 1), normalTS, _NormalStrength));
    
    float3 fiberNormal = UnpackNormal(RG_TEX_SAMPLE(_ClothFiberNormalMap, uv));
    fiberNormal = normalize(lerp(float3(0, 0, 1), fiberNormal, _FiberStrenght));
    surfaceData.normalTS = normalize(float3(normalTS.rg + fiberNormal.rg, normalTS.b * fiberNormal.b));
    
    surfaceData.occlusion = 1.0;
    
    surfaceData.emission = 0;
    
    half3 specularSample = RG_TEX_SAMPLE(_SpecularMap, uv).r;
    surfaceData.specular = _SpecularColor.rgb * specularSample * _SpecularAmount;
    surfaceData.smoothness = saturate(_SpecularPower);
}

#endif