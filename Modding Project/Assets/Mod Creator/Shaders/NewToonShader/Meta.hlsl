#ifndef RailMETA
#define RailMETA

#pragma vertex UniversalVertexMeta
#pragma fragment RailMetaLit

#include_with_pragmas "./CommonInclude.hlsl"
#include "./CommonInclude.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

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
//#pragma shader_feature EDITOR_VISUALIZATION

half4 RailMetaLit(Varyings input) : SV_Target {
    
    MetaInput metaInput = (MetaInput)0;
    metaInput.Albedo = _TintColor.rgb * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb;
    metaInput.Emission = half3(10,10,1);

    return UniversalFragmentMeta(input, metaInput);
}

#endif
