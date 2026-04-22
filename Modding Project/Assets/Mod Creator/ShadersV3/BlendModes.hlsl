#ifndef BLENDMODES_HLSL
#define BLENDMODES_HLSL

//--------------------------------------------------------------------------------
// Normal Group
//--------------------------------------------------------------------------------

/**
 * \brief Blends two colors using the Normal blend mode. DOES NOT USE ALPHA.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 NormalColBlend(float3 base, float3 blend) {
    return blend > 0 ? blend : base;
}

/**
 * \brief Blends two colors using the Normal blend mode. USES ALPHA.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float4 NormalColBlend(float4 base, float4 blend) {
    return blend.a > 0 ? blend : base;
}

//--------------------------------------------------------------------------------
// Darken Group
//--------------------------------------------------------------------------------

/**
 * \brief Blends two colors using the Darken blend mode. Works on a per channel basis.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 DarkenBlend(float3 base, float3 blend) {
    return float3(min(base.r, blend.r), min(base.g, blend.g), min(base.b, blend.b));
}

/**
 * \brief Blends two colors using the Multiply blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 MultiplyBlend(float3 base, float3 blend) {
    return base * blend;
}

/**
 * \brief Blends two colors using the Color Burn blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 ColorBurnBlend(float3 base, float3 blend) {
    return 1-(1-base)/blend;
}

/**
 * \brief Blends two colors using the Linear Burn blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 LinearBurnBlend(float3 base, float3 blend) {
    return base + blend - 1;
}

/**
 * \brief Blends two colors using the Darker Color blend mode. Works on the overall color.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 DarkerColorBlend(float3 base, float3 blend) {
    return blend < base ? blend : base;
}

//--------------------------------------------------------------------------------
// Lighten Group
//--------------------------------------------------------------------------------

/**
 * \brief Blends two colors using the Lighten blend mode. Works on a per channel basis.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 LightenBlend(float3 base, float3 blend){
    return float3(max(base.r, blend.r), max(base.g, blend.g), max(base.b, blend.b));
}

/**
 * \brief Blends two colors using the Screen blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 ScreenBlend(float3 base, float3 blend) {
    return 1-(1-base)*(1-blend);
}

/**
 * \brief Blends two colors using the Color Dodge blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 ColorDodgeBlend(float3 base, float3 blend) {
    return base / (1-blend);
}

/**
 * \brief Blends two colors using the Linear Dodge blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 LinearDodgeBlend(float3 base, float3 blend) {
    return base + blend;
}

/**
 * \brief Blends two colors using the Lighter Color blend mode. Works on the overall color.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 LighterColorBlend(float3 base, float3 blend) {
    return blend > base ? blend : base;
}

//--------------------------------------------------------------------------------
// Contrast Group
//--------------------------------------------------------------------------------

/**
 * \brief Blends two colors using the Overlay blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 OverlayBlend(float3 base, float3 blend) {
    return (blend < 0.5) ? ScreenBlend(base, blend) : MultiplyBlend(base, blend);
}

/**
 * \brief Blends two colors using the Soft Light blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 SoftLightBlend(float3 base, float3 blend) {
    return blend > 0.5 ? ScreenBlend(base, blend*0.5) : MultiplyBlend(base, blend*2);
}

//TODO: this lacks some checking for proper blending. Cit:
// It uses a half-strength application of these modes,
// and logic similar to the Overlay blend mode,
// but favors the active layer, as opposed to the underlying layers.

/**
 * \brief Blends two colors using the Hard Light blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 HardLightBlend(float3 base, float3 blend) {
    return base > 0.5 ? LinearDodgeBlend(base, blend*0.5) : LinearBurnBlend(base, blend*2);
}

/**
 * \brief Blends two colors using the Vivid Light blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 VividLightBlend(float3 base, float3 blend) {
    return blend > 0.5 ? ColorDodgeBlend(base, blend*0.5) : ColorBurnBlend(base, blend*2);
}

/**
 * \brief Blends two colors using the Linear Light blend mode.
 * \param base Base color
 * \param blend Blend color
 * \return Blended color
 */
inline float3 LinearLightBlend(float3 base, float3 blend) {
    return blend > 0.5 ? LinearDodgeBlend(base, blend*0.5) : LinearBurnBlend(base, blend*2);
}

#endif