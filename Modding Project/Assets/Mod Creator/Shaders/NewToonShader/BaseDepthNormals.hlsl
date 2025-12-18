#ifndef BASE_DEPTH_NORMALS
#define BASE_DEPTH_NORMALS

#include_with_pragmas "./CommonInclude.hlsl"

CBUFFER_START(UnityPerMaterial)
CBufTexture(_MainTex);
float _AlphaClip;
float _Cutoff;
float _Surface;
float _ClothingLayersSeparation;
float _ClothingLayer;
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
    float2 uv : TEXCOORD1;
    #endif
    float3 normalWS : TEXCOORD2;
};

void vert(v IN, out v2f output) {
    output.position   = TransformObjectToHClip(IN.vertex);
    
    //push position of vertex towards camera normal plane based on clothing layer
    float linearZ  = Linear01Depth(output.position.z, _ZBufferParams);
    output.position.z += _ClothingLayersSeparation * _ClothingLayer * linearZ;
    
    #ifdef _ALPHATEST_ON
    output.uv         = TRANSFORM_TEX(IN.uv, _MainTex);
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
    float4 alphaTexSample = RG_TEX_SAMPLE(_MainTex, IN.uv);
    CalculateAlpha(alphaTexSample.a, _AlphaClip, _Cutoff, _Surface);
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
