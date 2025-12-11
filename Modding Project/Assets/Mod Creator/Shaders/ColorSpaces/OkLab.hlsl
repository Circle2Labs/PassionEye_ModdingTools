#ifndef OKLAB_HLSL
#define OKLAB_HLSL

/**
 * \brief Converts an RGBA color into OkLab
 * \param RGB RGBA color to convert
 * \return OkLab color
 */
float3 LinearSRGBtoOkLAB(float3 RGB) {
    float l = 0.4122214708f * RGB.r + 0.5363325363f * RGB.g + 0.0514459929f * RGB.b;
    float m = 0.2119034982f * RGB.r + 0.6806995451f * RGB.g + 0.1073969566f * RGB.b;
    float s = 0.0883024619f * RGB.r + 0.2817188376f * RGB.g + 0.6299787005f * RGB.b;

    float l_ = pow(l, 1.0f / 3.0f);
    float m_ = pow(m, 1.0f / 3.0f);
    float s_ = pow(s, 1.0f / 3.0f);
    
    return float3(
        0.2104542553f*l_ + 0.7936177850f*m_ - 0.0040720468f*s_,
        1.9779984951f*l_ - 2.4285922050f*m_ + 0.4505937099f*s_,
        0.0259040371f*l_ + 0.7827717662f*m_ - 0.8086757660f*s_
    );
}

/**
 * \brief Converts an RGBA color into OkLab
 * \param RGB RGBA color to convert
 * \return OkLab color
 */
half3 LinearSRGBtoOkLAB(half3 RGB) {
    half l = 0.4122214708f * RGB.r + 0.5363325363f * RGB.g + 0.0514459929f * RGB.b;
    half m = 0.2119034982f * RGB.r + 0.6806995451f * RGB.g + 0.1073969566f * RGB.b;
    half s = 0.0883024619f * RGB.r + 0.2817188376f * RGB.g + 0.6299787005f * RGB.b;

    half l_ = pow(l, 1.0f / 3.0f);
    half m_ = pow(m, 1.0f / 3.0f);
    half s_ = pow(s, 1.0f / 3.0f);
    
    return half3(
        0.2104542553f*l_ + 0.7936177850f*m_ - 0.0040720468f*s_,
        1.9779984951f*l_ - 2.4285922050f*m_ + 0.4505937099f*s_,
        0.0259040371f*l_ + 0.7827717662f*m_ - 0.8086757660f*s_
    );
}

/**
 * \brief Converts an OkLab color into RGBA
 * \param c OkLab color to convert
 * \return RGBA color
 */
float3 OkLABtoLinearSRGB(float3 c) {
    float l_ = c.x + 0.3963377774f * c.y + 0.2158037573f * c.z;
    float m_ = c.x - 0.1055613458f * c.y - 0.0638541728f * c.z;
    float s_ = c.x - 0.0894841775f * c.y - 1.2914855480f * c.z;

    float l = l_*l_*l_;
    float m = m_*m_*m_;
    float s = s_*s_*s_;

    return float3(
        +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
        -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
        -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s
    );
}

/**
 * \brief Converts an OkLab color into RGBA
 * \param c OkLab color to convert
 * \return RGBA color
 */
half3 OkLABtoLinearSRGB(half3 c) {
    half l_ = c.x + 0.3963377774f * c.y + 0.2158037573f * c.z;
    half m_ = c.x - 0.1055613458f * c.y - 0.0638541728f * c.z;
    half s_ = c.x - 0.0894841775f * c.y - 1.2914855480f * c.z;

    half l = l_*l_*l_;
    half m = m_*m_*m_;
    half s = s_*s_*s_;

    return half3(
        +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
        -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
        -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s
    );
}

//TODO: This requires some testing!!!
/**
 * \brief Blends two colors in OkLab space
 * \param from First color to blend
 * \param to Second color to blend
 * \param t Blend factor
 * \return Blended color
 */
float3 BlendOkLab(float3 from, float3 to, float t) {
    from = LinearSRGBtoOkLAB(from);
    to = LinearSRGBtoOkLAB(to);
    return saturate(OkLABtoLinearSRGB(lerp(from, to, t)));
}

/**
 * \brief Blends two colors in OkLab space
 * \param from First color to blend
 * \param to Second color to blend
 * \param t Blend factor
 * \return Blended color
 */
half3 BlendOkLab(half3 from, half3 to, half t) {
    from = LinearSRGBtoOkLAB(from);
    to = LinearSRGBtoOkLAB(to);
    return OkLABtoLinearSRGB(lerp(from, to, t));
}

#endif