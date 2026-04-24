#ifndef BEZIERS_HLSL
#define BEZIERS_HLSL

//==============================================================================
// Quadratic Bezier
//==============================================================================

/**
 * \brief Quadratic Bezier
 * \note this runs in polynomial mode, which is slower than matrix mode. Use Matrix versions for better performance
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float QuadraticBezier(float a, float b, float c, float t) {
    return (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
}

/**
 * \brief Quadratic Bezier
 * \note this runs in polynomial mode, which is slower than matrix mode. Use Matrix versions for better performance
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float2 QuadraticBezier(float2 a, float2 b, float2 c, float t) {
    return (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
}

/**
 * \brief Quadratic Bezier
 * \note this runs in polynomial mode, which is slower than matrix mode. Use Matrix versions for better performance
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float3 QuadraticBezier(float3 a, float3 b, float3 c, float t) {
    return (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
}

/**
 * \brief Quadratic Bezier
 * \note this runs in polynomial mode, which is slower than matrix mode. Use Matrix versions for better performance
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float4 QuadraticBezier(float4 a, float4 b, float4 c, float t) {
    return (1 - t) * ((1 - t) * a + (t * b)) + t * ((1 - t) * b + (t * c));
}

//==============================================================================
// Bezier Matrix Forms
//==============================================================================

#define QUADRATIC_B_MAT float3x3( 1, -2, 1, 0, 2, -2, 0, 0, 1)
#define CUBIC_B_MAT float4x4(1, 0, 0, 0, -3, 3, 0, 0, 3, -6, 3, 0, -1, 3, -3, 1)

/**
 * \brief Quadratic Bezier
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float QuadraticBezierMatrix(float a, float b, float c, float t) {
    float3x3 mat = transpose(QUADRATIC_B_MAT);
    mat[0] *= a;
    mat[1] *= b;
    mat[2] *= c;
    float3 result = mul(float1x3(1, t, t * t), mat);
    return result.x + result.y + result.z;
}

/**
 * \brief Quadratic Bezier
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float2 QuadraticBezierMatrix(float2 a, float2 b, float2 c, float t) {
    float3x3 matX = transpose(QUADRATIC_B_MAT);
    matX[0] *= a.x;
    matX[1] *= b.x;
    matX[2] *= c.x;
    
    float3x3 matY = transpose(QUADRATIC_B_MAT);
    matY[0] *= a.y;
    matY[1] *= b.y;
    matY[2] *= c.y;
    
    float2 result;
    float3 resultX = mul(float1x3(1, t, t * t), matX); 
    float3 resultY = mul(float1x3(1, t, t * t), matY);
    
    result.x = resultX.x + resultX.y + resultX.z;
    result.y = resultY.x + resultY.y + resultY.z;
    return result;
}

/**
 * \brief Quadratic Bezier
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float3 QuadraticBezierMatrix(float3 a, float3 b, float3 c, float t) {
    float3x3 matX = transpose(QUADRATIC_B_MAT);
    matX[0] *= a.x;
    matX[1] *= b.x;
    matX[2] *= c.x;
    
    float3x3 matY = transpose(QUADRATIC_B_MAT);
    matY[0] *= a.y;
    matY[1] *= b.y;
    matY[2] *= c.y;

    float3x3 matZ = transpose(QUADRATIC_B_MAT);
    matZ[0] *= a.z;
    matZ[1] *= b.z;
    matZ[2] *= c.z;
    
    float3 result;
    matX = transpose(matX);
    matY = transpose(matY);
    matZ = transpose(matZ);
    float3 resultX = mul(float1x3(1, t, t * t), matX); 
    float3 resultY = mul(float1x3(1, t, t * t), matY);
    float3 resultZ = mul(float1x3(1, t, t * t), matZ);
    
    result.x = resultX.x + resultX.y + resultX.z;
    result.y = resultY.x + resultY.y + resultY.z;
    result.z = resultZ.x + resultZ.y + resultZ.z;
    return result;
}

/**
 * \brief Quadratic Bezier
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param t The time value
 * \return The point on the curve at time t
 */
float4 QuadraticBezierMatrix(float4 a, float4 b, float4 c, float t) {
    t = clamp(t, 0, 1);
    float3x3 matX = transpose(QUADRATIC_B_MAT);
    matX[0] *= a.x;
    matX[1] *= b.x;
    matX[2] *= c.x;
    
    float3x3 matY = transpose(QUADRATIC_B_MAT);
    matY[0] *= a.y;
    matY[1] *= b.y;
    matY[2] *= c.y;

    float3x3 matZ = transpose(QUADRATIC_B_MAT);
    matZ[0] *= a.z;
    matZ[1] *= b.z;
    matZ[2] *= c.z;

    float3x3 matW = transpose(QUADRATIC_B_MAT);
    matW[0] *= a.w;
    matW[1] *= b.w;
    matW[2] *= c.w;
    
    float4 result;
    matX = transpose(matX);
    matY = transpose(matY);
    matZ = transpose(matZ);
    matW = transpose(matW);
    float3 resultX = mul(float1x3(1, t, t * t), matX); 
    float3 resultY = mul(float1x3(1, t, t * t), matY);
    float3 resultZ = mul(float1x3(1, t, t * t), matZ);
    float3 resultW = mul(float1x3(1, t, t * t), matW);
    
    result.x = resultX.x + resultX.y + resultX.z;
    result.y = resultY.x + resultY.y + resultY.z;
    result.z = resultZ.x + resultZ.y + resultZ.z;
    result.w = resultW.x + resultW.y + resultW.z;
    return result;
}

//==============================================================================
// Cubic Bezier
//==============================================================================

/**
 * \brief Cubic Bezier computation
 * \note This implementation uses Polynomial form for computation. Use the Matrix Version of this if possible.
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param d The fourth point
 * \param t The time value
 * \return The point on the curve at time t
 */
float CubicBezier(float a, float b, float c, float d, float t){ 
    return (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
}

/**
 * \brief Cubic Bezier computation
 * \note This implementation uses Polynomial form for computation. Use the Matrix Version of this if possible.
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param d The fourth point
 * \param t The time value
 * \return The point on the curve at time t
 */
float2 CubicBezier(float2 a, float2 b, float2 c, float2 d, float t){ 
    return (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
}

/**
 * \brief Cubic Bezier computation
 * \note This implementation uses Polynomial form for computation. Use the Matrix Version of this if possible.
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param d The fourth point
 * \param t The time value
 * \return The point on the curve at time t
 */
float3 CubicBezier(float3 a, float3 b, float3 c, float3 d, float t){ 
    return (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
}

/**
 * \brief Cubic Bezier computation
 * \note This implementation uses Polynomial form for computation. Use the Matrix Version of this if possible.
 * \param a The first point
 * \param b The second point
 * \param c The third point
 * \param d The fourth point
 * \param t The time value
 * \return The point on the curve at time t
 */
float4 CubicBezier(float4 a, float4 b, float4 c, float4 d, float t){ 
    return (1-t)*QuadraticBezier(a, b, c, t) + t*QuadraticBezier(b, c, d, t);
}

// TODO: Cubic Bezier Matrix Form!!!

#endif