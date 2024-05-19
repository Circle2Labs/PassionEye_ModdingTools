#ifndef FACE_SDF_SHADOW_HLSL
#define FACE_SDF_SHADOW_HLSL

#include "../Utils.hlsl"

/**
 * \brief Computes a shadow value for a given SDF shadow map and light direction.
 * \param smoothing Smoothness of the shadow. Influences the size of the SDF slice that gets sampled.
 * \param FwdVec Forward vector of the face.
 * \param RgtVec Right vector of the face.
 * \param LightDir Light direction.
 * \param shadowTex SDF sample at the current pixel.
 * \return Shadow intensity at current pixel.
 */
float FaceSdfShadow(float smoothing, float3 FwdVec, float3 RgtVec, float3 LightDir, float2 shadowTex) {
    float dotF = dot(normalize(FwdVec.xz), LightDir.xz);
    float dotR = dot(normalize(RgtVec.xz), LightDir.xz);
    //determines if light shines from front (1) or back (0)
    float _step = step(0, dotF);
    //maps all values on the left 0-1, and all values on right > 1
    float dotRAcos = acos(dotR)/PI*2; 
    //NO IDEA, seems to be the slice point for the SDF shadow sampling.
    float dotRAcosDir = (dotR < 0)? 1-dotRAcos : dotRAcos-1;
    //selects which channel will be used.
    float texShadowDir = (dotR < 0)? shadowTex.r : shadowTex.g;
    
    float shadowInt = remap(
        -dotRAcosDir-(smoothing/2),
        -dotRAcosDir+(smoothing/2),
        0, 1,
        clamp(texShadowDir, -dotRAcosDir-smoothing, -dotRAcosDir+smoothing)
    )*_step;
    
    return shadowInt*dotF*2;
}

/**
 * \brief Computes a shadow value for a given SDF shadow map and light direction.
 * \param smoothing Smoothness of the shadow. Influences the size of the SDF slice that gets sampled.
 * \param FwdVec Forward vector of the face.
 * \param RgtVec Right vector of the face.
 * \param LightDir Light direction.
 * \param shadowTex SDF sample at the current pixel.
 * \return Shadow intensity at current pixel.
 */
half FaceSdfShadow(half smoothing, half3 FwdVec, half3 RgtVec, half3 LightDir, half2 shadowTex) {
    half dotF = dot(normalize(FwdVec.xz), LightDir.xz);
    half dotR = dot(normalize(RgtVec.xz), LightDir.xz);
    //determines if light shines from front (1) or back (0)
    half _step = step(0, dotF);
    //maps all values on the left 0-1, and all values on right > 1
    half dotRAcos = acos(dotR)/PI*2; 
    //NO IDEA, seems to be the slice point for the SDF shadow sampling.
    half dotRAcosDir = (dotR < 0)? 1-dotRAcos : dotRAcos-1;
    //selects which channel will be used.
    half texShadowDir = (dotR < 0)? shadowTex.r : shadowTex.g;
    
    half shadowInt = remap(
        -dotRAcosDir-(smoothing/2),
        -dotRAcosDir+(smoothing/2),
        0, 1,
        clamp(texShadowDir, -dotRAcosDir-smoothing, -dotRAcosDir+smoothing)
    )*_step;
    
    return shadowInt*dotF*2;
}

#endif