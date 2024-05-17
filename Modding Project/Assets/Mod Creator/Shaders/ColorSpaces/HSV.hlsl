#ifndef HSV_HLSL
#define HSV_HLSL

/**
 * \brief Converts an RGBA color into HSVA
 * \param rgba RGBA color to convert
 * \return HSVA color
 */
float4 RGBAtoHSVA(float4 rgba) {
    float r = rgba.r;
    float g = rgba.g;
    float b = rgba.b;
    float max_value = max(max(r, g), b);
    float min_value = min(min(r, g), b);
    float delta = max_value - min_value;
    float4 hsva;
    
    // Hue calculation
    if (delta == 0.0f) {
        hsva.x = 0.0f;
    } else if (max_value == r) {
        hsva.x = 60.0f * fmod((g - b) / delta, 6.0f);
    } else if (max_value == g) {
        hsva.x = 60.0f * ((b - r) / delta + 2.0f);
    } else {
        hsva.x = 60.0f * ((r - g) / delta + 4.0f);
    }  if (hsva.x < 0.0f) {
        hsva.x += 360.0f;
    }  // Saturation calculation
    if (max_value == 0.0f) {
        hsva.y = 0.0f;
    } else {
        hsva.y = delta / max_value;
    }

    // Value calculation
    hsva.z = max_value;

    // Alpha channel
    hsva.w = rgba.w;
    
    return hsva;
}

/**
 * \brief Converts an RGBA color into HSVA
 * \param rgba RGBA color to convert 
 * \return  HSVA color
 */
half4 RGBAtoHSVA(half4 rgba) {
    half r = rgba.r;
    half g = rgba.g;
    half b = rgba.b;
    half max_value = max(max(r, g), b);
    half min_value = min(min(r, g), b);
    half delta = max_value - min_value;
    half4 hsva;
    // Hue calculation
    if (delta == 0.0f) {
        hsva.x = 0.0f;
    } else if (max_value == r) {
        hsva.x = 60.0f * fmod((g - b) / delta, 6.0f);
    } else if (max_value == g) {
        hsva.x = 60.0f * ((b - r) / delta + 2.0f);
    } else {
        hsva.x = 60.0f * ((r - g) / delta + 4.0f);
    }

    if (hsva.x < 0.0f) {
        hsva.x += 360.0f;
    }

    // Saturation calculation
    if (max_value == 0.0f) {
        hsva.y = 0.0f;
    } else {
        hsva.y = delta / max_value;
    }

    // Value calculation
    hsva.z = max_value;

    // Alpha channel
    hsva.w = rgba.w;
    
    return hsva;
}

/**
 * \brief Converts an RGBA color into HSVA
 * \param rgb RGBA color to convert
 * \return HSVA color
 */
float3 RGBtoHSV(float3 rgb) {
    float r = rgb.r;
    float g = rgb.g;
    float b = rgb.b;
    float max_value = max(max(r, g), b);
    float min_value = min(min(r, g), b);
    float delta = max_value - min_value;
    float3 hsv;
    
    // Hue calculation
    if (delta == 0.0f) {
        hsv.x = 0.0f;
    } else if (max_value == r) {
        hsv.x = 60.0f * fmod((g - b) / delta, 6.0f);
    } else if (max_value == g) {
        hsv.x = 60.0f * ((b - r) / delta + 2.0f);
    } else {
        hsv.x = 60.0f * ((r - g) / delta + 4.0f);
    }  if (hsv.x < 0.0f) {
        hsv.x += 360.0f;
    }  // Saturation calculation
    if (max_value == 0.0f) {
        hsv.y = 0.0f;
    } else {
        hsv.y = delta / max_value;
    }

    // Value calculation
    hsv.z = max_value;
    
    return hsv;
}

/**
 * \brief Converts an RGBA color into HSVA
 * \param rgb RGBA color to convert 
 * \return  HSVA color
 */
half3 RGBtoHSV(half3 rgb) {
    half r = rgb.r;
    half g = rgb.g;
    half b = rgb.b;
    half max_value = max(max(r, g), b);
    half min_value = min(min(r, g), b);
    half delta = max_value - min_value;
    half3 hsv;
    // Hue calculation
    if (delta == 0.0f) {
        hsv.x = 0.0f;
    } else if (max_value == r) {
        hsv.x = 60.0f * fmod((g - b) / delta, 6.0f);
    } else if (max_value == g) {
        hsv.x = 60.0f * ((b - r) / delta + 2.0f);
    } else {
        hsv.x = 60.0f * ((r - g) / delta + 4.0f);
    }

    if (hsv.x < 0.0f) {
        hsv.x += 360.0f;
    }

    // Saturation calculation
    if (max_value == 0.0f) {
        hsv.y = 0.0f;
    } else {
        hsv.y = delta / max_value;
    }

    // Value calculation
    hsv.z = max_value;
    
    return hsv;
}

/**
 * \brief Converts an HSVA color into RGBA
 * \param hsva HSVA color to convert
 * \return RGBA color
 */
float4 HSVAtoRGBA(float4 hsva) {
    float h = hsva.x;
    float s = hsva.y;
    float v = hsva.z;
    float a = hsva.w;
    
    float4 rgba;
    
    float c = v * s;
    float x = c * (1.0f - abs(fmod(h / 60.0f, 2.0f) - 1.0f));
    float m = v - c;

    if (h >= 0.0f && h < 60.0f) {
        rgba = float4(c, x, 0.0f, 1.0f);
    } else if (h >= 60.0f && h < 120.0f) {
        rgba = float4(x, c, 0.0f, 1.0f);
    } else if (h >= 120.0f && h < 180.0f) {
        rgba = float4(0.0f, c, x, 1.0f);
    } else if (h >= 180.0f && h < 240.0f) {
        rgba = float4(0.0f, x, c, 1.0f);
    } else if (h >= 240.0f && h < 300.0f) {
        rgba = float4(x, 0.0f, c, 1.0f);
    } else {
        rgba = float4(c, 0.0f, x, 1.0f);
    }

    rgba.xyz += m;
    rgba.w = a;
    return rgba;
}

/**
 * \brief Converts an HSVA color into RGBA
 * \param hsva HSVA color to convert
 * \return RGBA color
 */
half4 HSVAtoRGBA(half4 hsva) {
    half h = hsva.x;
    half s = hsva.y;
    half v = hsva.z;

    half c = v * s;
    half x = c * (1.0f - abs(fmod(h / 60.0f, 2.0f) - 1.0f));
    half m = v - c;

    half4 rgba;
    
    if (h >= 0.0f && h < 60.0f)
    {
        rgba = half4(c, x, 0.0f, 1.0f);
    }
    else if (h >= 60.0f && h < 120.0f)
    {
        rgba = half4(x, c, 0.0f, 1.0f);
    }
    else if (h >= 120.0f && h < 180.0f)
    {
        rgba = half4(0.0f, c, x, 1.0f);
    }
    else if (h >= 180.0f && h < 240.0f)
    {
        rgba = half4(0.0f, x, c, 1.0f);
    }
    else if (h >= 240.0f && h < 300.0f)
    {
        rgba = half4(x, 0.0f, c, 1.0f);
    }
    else
    {
        rgba = half4(c, 0.0f, x, 1.0f);
    }

    rgba.xyz += m;
    rgba.w = hsva.w;

    return rgba;
}

/**
 * \brief Converts an HSV color into RGB
 * \param hsv HSV color to convert
 * \return RGB color
 */
float3 HSVtoRGB(float3 hsv) {
    float h = hsv.x;
    float s = hsv.y;
    float v = hsv.z;
    
    float3 rgb;
    
    float c = v * s;
    float x = c * (1.0f - abs(fmod(h / 60.0f, 2.0f) - 1.0f));
    float m = v - c;

    if (h >= 0.0f && h < 60.0f) {
        rgb = float3(c, x, 0.0f);
    } else if (h >= 60.0f && h < 120.0f) {
        rgb = float3(x, c, 0.0f);
    } else if (h >= 120.0f && h < 180.0f) {
        rgb = float3(0.0f, c, x);
    } else if (h >= 180.0f && h < 240.0f) {
        rgb = float3(0.0f, x, c);
    } else if (h >= 240.0f && h < 300.0f) {
        rgb = float3(x, 0.0f, c);
    } else {
        rgb = float3(c, 0.0f, x);
    }

    rgb.xyz += m;
    return rgb;
}

/**
 * \brief Converts an HSV color into RGB
 * \param hsv HSV color to convert
 * \return RGB color
 */
half3 HSVtoRGB(half3 hsv) {
    half h = hsv.x;
    half s = hsv.y;
    half v = hsv.z;
    
    half3 rgb;

    half c = v * s;
    half x = c * (1.0f - abs(fmod(h / 60.0f, 2.0f) - 1.0f));
    half m = v - c;
    
    if (h >= 0.0f && h < 60.0f) {
        rgb = half3(c, x, 0.0f);
    } else if (h >= 60.0f && h < 120.0f) {
        rgb = half3(x, c, 0.0f);
    } else if (h >= 120.0f && h < 180.0f) {
        rgb = half3(0.0f, c, x);
    } else if (h >= 180.0f && h < 240.0f) {
        rgb = half3(0.0f, x, c);
    } else if (h >= 240.0f && h < 300.0f) {
        rgb = half3(x, 0.0f, c);
    } else {
        rgb = half3(c, 0.0f, x);
    }

    rgb.xyz += m;

    return rgb;
}

/**
 * \brief Blends two colors in the HSV color space
 * \param from First color to blend
 * \param to Second color to blend
 * \param t Blend factor
 * \return Blended color
 */
float4 BlendHSV(float4 from, float4 to, float t){
    //RGB to HSV conversion
    float4 fromHSV = RGBAtoHSVA(from);
    float4 toHSV = RGBAtoHSVA(to);

    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if((fromHSV.x == 0 || toHSV.x == 0) && (fromHSV.y == 0 || toHSV.y == 0)) {
        x = max(fromHSV.x, toHSV.x);
    } else {
        x = lerp(fromHSV.x, toHSV.x, t);
    }
    
    //HSV blending
    float4 result;
    result.x = x;
    result.yz = lerp(fromHSV.yz, toHSV.yz, t);
    
    //conversion back to RGB
    result = HSVAtoRGBA(result);
    //separate alpha channel blending
    result.w = lerp(from.w, to.w, t);
    //all done!
    return result;
}

/**
 * \brief Blends two colors in the HSV color space
 * \param from First color to blend
 * \param to Second color to blend
 * \param t Blend factor
 * \return Blended color
 */
half4 BlendHSV(half4 from, half4 to, half t){
    //RGB to HSV conversion
    half4 fromHSV = RGBAtoHSVA(from);
    half4 toHSV = RGBAtoHSVA(to);

    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if((fromHSV.x == 0 || toHSV.x == 0) && (fromHSV.y == 0 || toHSV.y == 0)) {
        x = max(fromHSV.x, toHSV.x);
    } else {
        x = lerp(fromHSV.x, toHSV.x, t);
    }
    
    //HSV blending
    half4 result;
    result.x = x;
    result.yz = lerp(fromHSV.yz, toHSV.yz, t);
    
    //conversion back to RGB
    result = HSVAtoRGBA(result);
    //separate alpha channel blending
    result.w = lerp(from.w, to.w, t);
    //all done!
    return result;
}

/**
 * \brief Blends two colors in the HSV color space
 * \param from First color to blend
 * \param to Second color to blend
 * \param t Blend factor
 * \return Blended color
 */
float3 BlendHSV(float3 from, float3 to, float t){
    //RGB to HSV conversion
    float3 fromHSV = RGBtoHSV(from);
    float3 toHSV = RGBtoHSV(to);

    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if((fromHSV.x == 0 || toHSV.x == 0) && (fromHSV.y == 0 || toHSV.y == 0)) {
        x = max(fromHSV.x, toHSV.x);
    } else {
        x = lerp(fromHSV.x, toHSV.x, t);
    }
    
    //HSV blending
    float3 result;
    result.x = x;
    result.yz = lerp(fromHSV.yz, toHSV.yz, t);
    
    //conversion back to RGB
    result = HSVtoRGB(result);
    //all done!
    return result;
}

/**
 * \brief Blends two colors in the HSV color space
 * \param from First color to blend
 * \param to Second color to blend
 * \param t Blend factor
 * \return Blended color
 */
half3 BlendHSV(half3 from, half3 to, half t){
    //RGB to HSV conversion
    half3 fromHSV = RGBtoHSV(from);
    half3 toHSV = RGBtoHSV(to);

    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if((fromHSV.x == 0 || toHSV.x == 0) && (fromHSV.y == 0 || toHSV.y == 0)) {
        x = max(fromHSV.x, toHSV.x);
    } else {
        x = lerp(fromHSV.x, toHSV.x, t);
    }
    
    //HSV blending
    half3 result;
    result.x = x;
    result.yz = lerp(fromHSV.yz, toHSV.yz, t);
    
    //conversion back to RGB
    result = HSVtoRGB(result);
    //all done!
    return result;
}

#endif