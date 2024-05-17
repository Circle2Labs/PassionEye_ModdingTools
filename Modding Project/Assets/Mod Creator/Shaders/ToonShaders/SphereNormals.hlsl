#ifndef SPHERE_NORMALS_HLSL
#define SPHERE_NORMALS_HLSL

/**
 * \brief Computes FAKE spherical normals for a given position and center
 * \param positionWS Position in world space
 * \param center Center of the sphere in world space
 * \return Spherical normal vector
 */
float3 SphereNrm(float3 positionWS, float3 center) {
    positionWS.y = center.y;
    //calculate normal of sphere in object space (using position)
    float3 normal = normalize(positionWS - center);
    normal.y = .2;
    normal.z *= 5;
    normal.x *= 1;
    return normal;
}

float3 SphereNrm(float3 positionWS, float3 center, float radius) {
    positionWS.y = center.y;
    //calculate normal of sphere in object space (using position)
    float3 normal = normalize(positionWS - center);
    normal.y = .2f * radius;
    normal.z *= 5.f * radius;
    normal.x *= 1.f * radius;
    return normal;
}

/**
 * \brief Computes FAKE spherical normals for a given position and center
 * \param positionWS Position in world space
 * \param center Center of the sphere in world space
 * \return Spherical normal vector
 */
half3 SphereNrm_half(half3 positionWS, half3 center) {
    positionWS.y = center.y;
    //calculate normal of sphere in object space (using position)
    half3 normal = normalize(positionWS - center);
    normal.y = .2;
    normal.z *= 5;
    normal.x *= 1;
    return normal;
}

#endif