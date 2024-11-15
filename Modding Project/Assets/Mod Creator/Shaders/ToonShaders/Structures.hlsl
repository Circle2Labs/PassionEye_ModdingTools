#ifndef STRUCTURES_HLSL
#define STRUCTURES_HLSL

#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
#endif

/**
 * \brief Structure for face rendering data used in PE Shading.
 * \note Provided yoiu know what you're doing, you could also send null values for the face center or the SDF specific vectors.
 */
struct FaceData {
    bool    isFace;
    bool    isSdf;
    float2  sdfSample;
    float3  faceCenter;
    float3  faceFwdVec;
    float3  faceRgtVec;
};

/**
 * \brief Structure for generic geometry data used in PE Shading.
 * \note This structure is carrying also some non strictly geometrical data. Like shadowMask.
 */
struct GeometryData {
    float3  lgtDir;
    float3  posWs;
    float3  nrmWs;
    float3  viewDir;
    float3  mainLgtDir;
    float4  shadowCoord;
    half4   shadowMask;
    half4   lightmapUV;
};

/**
 * \brief Structure for diffuse shading data used in PE Shading.
 * \note The surface color is not here. See the MetallicData structure.
 */
struct DiffuseData {
    float   smooth;
    float   secBndOffset;
    float3  SSSPower;
    float3  SSSOffset;
    float   NdotL;
    float   NdotLBias;
    float3  lightTint;
    bool    auto2ndBndCol;
    float3  firstBndCol;
    float3  secBndCol;
    float   shadowAttn;
};

/**
 * \brief Structure for specular shading data used in PE Shading.
 * \note The smoothness is not here. See the RoughnessData structure.
 */
struct SpecularData {
    float   specAmt;
    float   specPow;
    bool    autoCol;
    float3  specCol;
};

/**
 * \brief Structure for metallic shading data used in PE Shading.
 * \note The surface color is here as "baseCol". The reason is memory data optimization.
 */
struct MetallicData {
    bool        useMetallic;
    float       metalness;
    float3      baseCol;
    Gradient    gradient;
};

/**
 * \brief Structure for roughness shading data used in PE Shading.
 * \note The smoothness is here as "roughness". (Smoothness = 1-Roughness)
 */
struct RoughnessData {
    bool    useRoughness;
    float   roughness;
    float   fresnelAmt;
};

#endif