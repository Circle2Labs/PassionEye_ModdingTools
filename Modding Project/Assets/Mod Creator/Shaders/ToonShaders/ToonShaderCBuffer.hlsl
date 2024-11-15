#ifndef TOON_SHADER_CBUFFER_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#define TOON_SHADER_CBUFFER_INCLUDED

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

#endif
