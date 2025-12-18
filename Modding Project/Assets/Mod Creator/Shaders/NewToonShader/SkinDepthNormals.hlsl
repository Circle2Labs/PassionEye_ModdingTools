#ifndef SKIN_DEPTH_NORMALS
#define SKIN_DEPTH_NORMALS

#include_with_pragmas "./CommonInclude.hlsl"

CBUFFER_START(UnityPerMaterial)
CBufTexture(_AlphaMap);
float _AlphaClip;
float _Cutoff;
float _Surface;

CBUFFER_END

#pragma vertex vert
#pragma fragment frag

struct v {
    float3 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
};

struct v2f {
    float4 position : SV_POSITION;
    #ifdef _ALPHATEST_ON
    float2 uv       : TEXCOORD1;
    #endif
    float3 normalWS     : TEXCOORD2;
};

void vert(v IN, out v2f output) {
    output.position   = TransformObjectToHClip(IN.vertex);
    #ifdef _ALPHATEST_ON
    output.uv         = TRANSFORM_TEX(IN.uv, _AlphaMap);
    #endif
    VertexNormalInputs vni = GetVertexNormalInputs(IN.normal, IN.tangent);
    output.normalWS = NormalizeNormalPerVertex(vni.normalWS);
}

void frag(
    v2f IN,
    out half4 normalsWS : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{    
    #ifdef _ALPHATEST_ON
    float4 alphaTexSample = RG_TEX_SAMPLE(_AlphaMap, IN.uv);
    CalculateAlpha(alphaTexSample.r, _AlphaClip, _Cutoff, _Surface);
    #endif
    
    #ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(IN.position);
    #endif
        
    half3 normalWS = NormalizeNormalPerPixel(IN.normalWS);
    normalsWS = half4(normalWS, 0);
    
    #ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif
}

#endif
