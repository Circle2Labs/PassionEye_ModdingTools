#ifndef FUR_FORWARD
#define FUR_FORWARD

#include_with_pragmas "ToonVariants.hlsl"

#pragma vertex vert
#pragma geometry geom
#pragma fragment frag

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "CustomLighting.hlsl"

struct v {
    RG_VertIn;
};

struct v2g {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
    float4 lmuv : TEXCOORD1;
    float4 rtuv : TEXCOORD2;
};

struct g2f {
    RG_FragIn;
    float layerDepth : ATTRIB0;
    bool alpha : ATTRIB1;
};

#include "ToonShaderCBuffer.hlsl"

v2g vert(v i) {
    v2g o;
    o.vertex = float4(i.vertex, 0);
    o.normal = i.normal;
    o.tangent = i.tangent;
    o.uv = i.uv;
    o.lmuv = i.lmuv;
    o.rtuv = i.rtuv;
    return o;
}

const int MaxLayers = 5;

[maxvertexcount(30)]
void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream) {
    for (int layer = 0; layer <= 5; layer++) {
        for (int x = 0; x < 3; x++) {
            g2f o;
            float layerOffset = .0001;
            o.normal = TransformObjectToWorldNormal(i[x].normal);
            o.tangent = float4(TransformObjectToWorldDir(i[x].tangent.xyz), i[x].tangent.w * GetOddNegativeScale());
            float3 pos = TransformObjectToWorld(i[x].vertex);
            pos += o.normal * layer * layerOffset;
            o.position = TransformWorldToHClip(pos);
            o.positionWS = pos;

            o.uv = i[x].uv;
            o.lmuv = i[x].lmuv;
            o.rtuv = i[x].rtuv;
            o.layerDepth = clamp(float(layer)+1 / 5.0f, 0.5f, 1.0f);
            o.alpha = layer != 0;
            triStream.Append(o);
        }
    }
}

float4 frag(g2f IN): SV_Target {
    float alpha = 0;
    float layerAttenuation = 1;
    
    if (IN.alpha) {
        alpha = CalculateAlpha(IN.uv);
        layerAttenuation = IN.layerDepth;
    }
    
    return float4(PE_LIGHTING(IN.uv, IN.positionWS, IN.normal, IN.tangent, IN.lmuv) * layerAttenuation, alpha);
}
#endif