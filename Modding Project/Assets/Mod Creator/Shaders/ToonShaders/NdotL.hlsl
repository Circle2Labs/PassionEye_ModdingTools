#ifndef NDOTL_HLSL
#define NDOTL_HLSL

#include "Structures.hlsl"
#include "FaceSdfShadow.hlsl"
#include "SphereNormals.hlsl"

/**
 * \brief Calculates the NdotL for a given face if needed, otherwise returns a normal NdotL.
 * \param faceData face rendering data. Contains bools for choosing SDF/Sphere Remapping, and if the current pixel is a face.
 * \param geomData generic geometrical data.
 * \param diffData diffuse shading data.
 * \return Adjusted NdotL
 */
float NdotL(FaceData faceData, GeometryData geomData, DiffuseData diffData) {
    // we kinda know this is based on a bool that acts like an uniform. We can branch safely on it.
    // the day someone actually has problems with this... they can render 2 materials.
    // Way faster than calculating (2^2)-1 branches per pixel.
    UNITY_BRANCH if(faceData.isFace) {
        UNITY_BRANCH if(faceData.isSdf) {
            return FaceSdfShadow(
                diffData.smooth,
                faceData.faceFwdVec,
                faceData.faceRgtVec,
                geomData.lgtDir,
                faceData.sdfSample
            );
        } else {
            float3 normals = SphereNrm(geomData.posWs, faceData.faceCenter);
            return saturate(dot(normals, geomData.lgtDir) + diffData.NdotLBias);
        }
    } else {
        return saturate(dot(geomData.nrmWs, geomData.lgtDir) + diffData.NdotLBias);
    }
}

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

/**blend
 * \brief Applies the half lambert formula to a given value.
 * \param x value to apply the half lambert formula to.
 * \return half lambert value.
 */
float3 HalfLambertCurve(float3 x)
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
    return saturate(pow(dot(normalWS, lightDir) * 0.5 + 0.5, 2));
}

#endif