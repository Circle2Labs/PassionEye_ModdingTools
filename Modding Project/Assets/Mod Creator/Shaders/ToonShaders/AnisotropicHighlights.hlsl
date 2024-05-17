#ifndef ANISOTROPIC_HIGHLIGHTS_HLSL
#define ANISOTROPIC_HIGHLIGHTS_HLSL

#include "../Utils.hlsl"

/**
 * \brief Computes stylized Anisotropic highlights from a map.
 * \note this is intended for hair. It's not a correct model for brushed metals/anisotropic surfaces.
 * \param mask anisotropic highlight mask. Only R channel is used.
 * \param lightDot NdotL from the light.
 * \param normalWS World Space normals.
 * \param ViewDir View direction in world space.
 * \param fresnelPow fresnel power.
 * \param fresnelIntensity fresnel intensity.
 * \return additive anisotropic highlight.
 */
float3 AnisotropicHighlight(float3 mask, float lightDot, float3 normalWS, float3 ViewDir, float fresnelPow, float fresnelIntensity) {
    float fresnel = Fresnel(normalWS, ViewDir, fresnelPow, fresnelIntensity);
    float anisotropic = saturate (1-fresnel) * mask.r * lightDot;
    return anisotropic;
}

/**
 * \brief Computes stylized Anisotropic highlights from a map.
 * \note this is intended for hair. It's not a correct model for brushed metals/anisotropic surfaces.
 * \param mask anisotropic highlight mask. Only R channel is used.
 * \param lightDot NdotL from the light.
 * \param normalWS World Space normals.
 * \param ViewDir View direction in world space.
 * \param fresnelPow fresnel power.
 * \param fresnelIntensity fresnel intensity.
 * \return additive anisotropic highlight.
 */
half3 AnisotropicHighlight(half3 mask, half lightDot, half3 normalWS, half3 ViewDir, half fresnelPow, half fresnelIntensity) {
    half fresnel = Fresnel(normalWS, ViewDir, fresnelPow, fresnelIntensity);
    half anisotropic = saturate (1-fresnel) * mask.r * lightDot;
    return anisotropic;
}

#endif