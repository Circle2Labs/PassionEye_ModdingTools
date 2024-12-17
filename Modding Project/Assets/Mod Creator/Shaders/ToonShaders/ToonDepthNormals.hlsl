#ifndef TOON_DEPTH_NORMALS
#define TOON_DEPTH_NORMALS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "../Utils.hlsl"

#pragma dynamic_branch _ _ALPHATEST_ON
#pragma dynamic_branch _ LOD_FADE_CROSSFADE

#pragma vertex vert
#pragma fragment frag

struct v {
    float3 positionOS : POSITION;
    float4 tangentOS : TANGENT;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct v2f {
    float4 pos : SV_POSITION;
    float3 normal : TEXCOORD0;
    float4 tangentWS : TEXCOORD1;
    float2 uv : TEXCOORD2;
    float3 viewDirWS : TEXCOORD3;
};


#include "ToonShaderCBuffer.hlsl"

v2f vert(v input) {
    v2f output;
    output.pos = TransformObjectToHClip(input.positionOS);
    
    //push position of vertex towards camera normal plane based on clothing layer
    float linearZ  = Linear01Depth(output.pos.z, _ZBufferParams);
    output.pos.z += _ClothingLayersSeparation * _ClothingLayer * linearZ;

    output.normal = input.normalOS;
    output.uv = input.uv;
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w * GetOddNegativeScale());
    output.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(input.positionOS.xyz));
    return output;
}

void frag(v2f input, out half4 outNormalWS : SV_Target0
    #ifdef _WRITE_RENDERING_LAYERS
        , out float4 outRenderingLayers : SV_Target1
    #endif
    ){
    
    float2 uv = TRANSFORM_TEX(input.uv, _MRFATex);
    float alpha = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, uv).a * _Transparency;
    AlphaDiscard(alpha, _Cutoff);

    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(input.pos);
    #endif

    float2 normalMapUV = TRANSFORM_TEX(input.uv, _NormalMap);
    float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalMapUV);
    float3 unpackedNormal = normalize(lerp(float3(0,0,1), UnpackNormal(normalSample), _NormalStrength));

    float3 normalWS = NormalMapToWorld(unpackedNormal, input.normal, input.tangentWS);

    outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);

    #ifdef _WRITE_RENDERING_LAYERS
        uint renderingLayers = GetMeshRenderingLayer();
        outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif
}

#endif