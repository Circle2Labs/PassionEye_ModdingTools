#ifndef NDOTL_HLSL
#define NDOTL_HLSL

#include "FaceSdfShadow.hlsl"
#include "SphereNormals.hlsl"

/**
 * \brief Calculates the NdotL the classic way.
 * \param normalWS world space normals
 * \param lightDir world space light direction
 * \return NdotL
 */
float NdotL(float3 normalWS, float3 lightDir) {
    #ifdef SHADERGRAPH_PREVIEW //bogus lighting for preview
    return saturate(dot(normalWS, float3(0.5, 0.5, 0)));
    #endif
    return saturate(dot(normalWS, lightDir));
}

/**
 * \brief Calculates the NdotL the classic way.
 * \param normalWS world space normals
 * \param lightDir world space light direction
 * \return NdotL
 */
half NdotL(half3 normalWS, half3 lightDir) {
    #ifdef SHADERGRAPH_PREVIEW //bogus lighting for preview
    return saturate(dot(normalWS, float3(0.5, 0.5, 0)));
    #endif
    return saturate(dot(normalWS, lightDir));
}

/**blend
* \brief Applies the half lambert formula to a given value.
* \param x value to apply the half lambert formula to.
* \return half lambert value.
*/
float HalfLambertCurve(float x)
{
    return saturate(pow(x * 0.5 + 0.5, 2));
}

/**
 * \brief Applies the half lambert formula to a given value.
 * \param x value to apply the half lambert formula to.
 * \return half lambert value.
 */
half HalfLambertCurve(half x)
{
    return saturate(pow(x * 0.5 + 0.5, 2));
}

/**
 * \brief Applies the half lambert formula to a given value.
 * \param x value to apply the half lambert formula to.
 * \return half lambert value.
 */
float3 HalfLambertCurve(float3 x)
{
    return saturate(pow(x * 0.5 + 0.5, 2));
}

/**
 * \brief Applies the half lambert formula to a given value.
 * \param x value to apply the half lambert formula to.
 * \return half lambert value.
 */
half3 HalfLambertCurve(half3 x)
{
    return saturate(pow(x * 0.5 + 0.5, 2));
}

/**
 * \brief Calculates the NdotL  using the half lambert formula (0.5 + 1/2(NdotL)).
 * \param normalWS world space normals
 * \param lightDir world space light direction
 * \return NdotL
 */
float HalfLambert(float3 normalWS, float3 lightDir)
{
    return HalfLambertCurve(dot(normalWS, lightDir));
}

/**
 * \brief Calculates the NdotL  using the half lambert formula (0.5 + 1/2(NdotL)).
 * \param normalWS world space normals
 * \param lightDir world space light direction
 * \return NdotL
 */
half HalfLambert(half3 normalWS, half3 lightDir)
{
    return HalfLambertCurve(dot(normalWS, lightDir));
}

#endif