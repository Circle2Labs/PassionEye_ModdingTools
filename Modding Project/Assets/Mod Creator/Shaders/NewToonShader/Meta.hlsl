#ifndef RailMETA
#define RailMETA

#pragma vertex UniversalVertexMeta
#pragma fragment RailMetaLit

#include_with_pragmas "./CommonInclude.hlsl"
#include "./CommonInclude.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#define MetaInput UnityMetaInput
#define MetaFragment UnityMetaFragment
float4 MetaVertexPosition(float4 positionOS, float2 uv1, float2 uv2, float4 uv1ST, float4 uv2ST)
{
    return UnityMetaVertexPosition(positionOS.xyz, uv1, uv2, uv1ST, uv2ST);
}

CBUFFER_START (UnityPerMaterial)
float4 _TintColor;
Texture2D _MainTex;
SamplerState sampler_MainTex;
float4 _MainTex_ST;
Texture2D _NormalMap;
SamplerState sampler_NormalMap;
float4 _NormalMap_ST;
float _NormalStrength;
CBUFFER_END

#pragma shader_feature_local_fragment _EMISSION
#pragma shader_feature EDITOR_VISUALIZATION

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv0          : TEXCOORD0;
    float2 uv1          : TEXCOORD1;
    float2 uv2          : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
    #ifdef EDITOR_VISUALIZATION
    float2 VizUV        : TEXCOORD1;
    float4 LightCoord   : TEXCOORD2;
    #endif
};

Varyings UniversalVertexMeta(Attributes input)
{
    Varyings output = (Varyings)0;
    output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
    output.uv = TRANSFORM_TEX(input.uv0, _MainTex);
    #ifdef EDITOR_VISUALIZATION
    UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
    #endif
    return output;
}

half4 UniversalFragmentMeta(Varyings fragIn, MetaInput metaInput)
{
    #ifdef EDITOR_VISUALIZATION
    metaInput.VizUV = fragIn.VizUV;
    metaInput.LightCoord = fragIn.LightCoord;
    #endif

    return UnityMetaFragment(metaInput);
}

half4 RailMetaLit(Varyings input) : SV_Target {
    
    MetaInput metaInput = (MetaInput)0;
    metaInput.Albedo = _TintColor.rgb * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
    metaInput.Emission = half3(0,0,0);

    return UniversalFragmentMeta(input, metaInput);
}

#endif
