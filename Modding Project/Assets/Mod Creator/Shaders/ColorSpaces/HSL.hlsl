#ifndef HSL_HLSL
#define HSL_HLSL

#include "../Constants.hlsl"

/**
 * \brief Converts an RGBA color into HSLA
 * \param color RGBA color to convert
 * \return HSLA color
 */
float4 RGBAtoHSLA(float4 color) {
    //Temp Variables for easier readability
    float R = color.r;
    float G = color.g;
    float B = color.b;
    float A = color.a;

    float H, S, L;

    //Find max between R G B
    float M = max(max(R, G), B);
    //Find min between R G B
    float m = min(min(R, G), B);
    //Find delta between max and min
    float d = M - m;

    //Calculate Luminance
    L = 0.5f * (M + m);

    //Calculate Saturation
    if (L > 0 && L < 1) {
        S = d / (1 - abs(2 * L - 1));
    } else {
        S = 0;
    }

    //Calculate HUE
    if (R == G && G == B)
        H = 0;
    else if (G >= B)
        H = acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    else
        H = 360 - acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    return float4(H, S, L, A);
}

/**
 * \brief Converts an RGBA color into HSLA
 * \param color RGBA color to convert
 * \return HSLA color
 */
half4 RGBAtoHSLA(half4 color) {
    //Temp Variables for easier readability
    half R, G, B, A;
    R = color.r;
    G = color.g;
    B = color.b;
    A = color.a;

    half H, S, L;

    //Find max between R G B
    half M = max(max(R, G), B);
    //Find min between R G B
    half m = min(min(R, G), B);
    //Find delta between max and min
    half d = M - m;

    //Calculate Luminance
    L = 0.5f * (M + m);

    //Calculate Saturation
    if (L > 0 && L < 1) {
        S = d / (1 - abs(2 * L - 1));
    } else {
        S = 0;
    }

    //Calculate HUE
    if (R == G && G == B)
        H = 0;
    else if (G >= B)
        H = acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    else
        H = 360 - acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    return half4(H, S, L, A);
}

/**
 * \brief Converts an RGB color into HSL
 * \param color RGB color to convert
 * \return HSL color
 */
float3 RGBtoHSL(float3 color) {
    //Temp Variables for easier readability
    float R = color.r;
    float G = color.g;
    float B = color.b;

    float H, S, L;

    //Find max between R G B
    float M = max(max(R, G), B);
    //Find min between R G B
    float m = min(min(R, G), B);
    //Find delta between max and min
    float d = M - m;

    //Calculate Luminance
    L = 0.5f * (M + m);

    //Calculate Saturation
    if (L > 0 && L < 1) {
        S = d / (1 - abs(2 * L - 1));
    } else {
        S = 0;
    }

    //Calculate HUE
    if (R == G && G == B)
        H = 0;
    else if (G >= B)
        H = acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    else
        H = 360 - acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    return float3(H, S, L);
}

/**
 * \brief Converts an RGB color into HSL
 * \param color RGB color to convert
 * \return HSL color
 */
half3 RGBtoHSL(half3 color) {
    //Temp Variables for easier readability
    float R = color.r;
    float G = color.g;
    float B = color.b;

    float H, S, L;

    //Find max between R G B
    float M = max(max(R, G), B);
    //Find min between R G B
    float m = min(min(R, G), B);
    //Find delta between max and min
    float d = M - m;

    //Calculate Luminance
    L = 0.5f * (M + m);

    //Calculate Saturation
    if (L > 0 && L < 1) {
        S = d / (1 - abs(2 * L - 1));
    } else {
        S = 0;
    }

    //Calculate HUE
    if (R == G && G == B)
        H = 0;
    else if (G >= B)
        H = acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    else
        H = 360 - acos(clamp((R - 0.5f * G - 0.5f * B) / sqrt(pow(R, 2) + pow(G, 2) + pow(B, 2) - R * G - R * B - G * B), -1, 1)) * RAD_2DEG;
    return half3(H, S, L);
}

/**
 * \brief Converts an HSLA color into RGBA
 * \param hlsa HSLA color to convert
 * \return RGBA color
 */
float4 HSLAtoRGBA(float4 hlsa) {
    //Temp variables
    float H, S, L, A;
    H = fmod(hlsa.x, 360);
    S = hlsa.y;
    L = hlsa.z;
    A = hlsa.w;
            
    float R, G, B;

    float d, m, x;
            
    d = S*(1- abs(2*L - 1));
    m = L - 0.5f * d;
    x = d * (1 - abs(((H / 60) % 2) - 1));

    if (H >= 0 && H < 60) {
        R = d + m;
        G = x + m;
        B = m;
    } else if (H >= 60 && H < 120) {
        R = x + m;
        G = d + m;
        B = m;
    } else if (H >= 120 && H < 180) {
        R = m;
        G = d + m;
        B = x + m;
    } else if (H >= 180 && H < 240) {
        R = m;
        G = x + m;
        B = d + m;
    } else if (H >= 240 && H < 300) {
        R = x + m;
        G = m;
        B = d + m;
    } else if (H >= 300 && H < 360) {
        R = d + m;
        G = m;
        B = x + m;
    }
    
    return float4(R, G, B, A);
}

/**
 * \brief Converts an HSLA color into RGBA
 * \param hlsa HSLA color to convert
 * \return RGBA color
 */
half4 HSLAtoRGBA(half4 hlsa) {
    //Temp variables
    half H, S, L, A;
    H = fmod(hlsa.x, 360);
    S = hlsa.y;
    L = hlsa.z;
    A = hlsa.w;
            
    half R, G, B;

    half d, m, x;
            
    d = S*(1- abs(2*L - 1));
    m = L - 0.5f * d;
    x = d * (1 - abs(((H / 60) % 2) - 1));

    if (H >= 0 && H < 60) {
        R = d + m;
        G = x + m;
        B = m;
    } else if (H >= 60 && H < 120) {
        R = x + m;
        G = d + m;
        B = m;
    } else if (H >= 120 && H < 180) {
        R = m;
        G = d + m;
        B = x + m;
    } else if (H >= 180 && H < 240) {
        R = m;
        G = x + m;
        B = d + m;
    } else if (H >= 240 && H < 300) {
        R = x + m;
        G = m;
        B = d + m;
    } else if (H >= 300 && H < 360) {
        R = d + m;
        G = m;
        B = x + m;
    }
    
    return half4(R, G, B, A);
}

/**
 * \brief Converts an HSL color into RGB
 * \param hls HSL color to convert
 * \return RGB color
 */
float3 HSLtoRGB(float3 hls) {
    //Temp variables
    float H = fmod(hls.x, 360);
    float S = hls.y;
    float L = hls.z;
            
    float R, G, B;

    float d, m, x;
            
    d = S*(1- abs(2*L - 1));
    m = L - 0.5f * d;
    x = d * (1 - abs(((H / 60) % 2) - 1));

    if (H >= 0 && H < 60) {
        R = d + m;
        G = x + m;
        B = m;
    } else if (H >= 60 && H < 120) {
        R = x + m;
        G = d + m;
        B = m;
    } else if (H >= 120 && H < 180) {
        R = m;
        G = d + m;
        B = x + m;
    } else if (H >= 180 && H < 240) {
        R = m;
        G = x + m;
        B = d + m;
    } else if (H >= 240 && H < 300) {
        R = x + m;
        G = m;
        B = d + m;
    } else if (H >= 300 && H < 360) {
        R = d + m;
        G = m;
        B = x + m;
    }
    
    return float3(R, G, B);
}

/**
 * \brief Converts an HSL color into RGB
 * \param hls HSL color to convert
 * \return RGB color
 */
half3 HSLtoRGB(half3 hls) {
    //Temp variables
    half H = fmod(hls.x, 360);
    half S = hls.y;
    half L = hls.z;
            
    half R, G, B;

    half d, m, x;
            
    d = S*(1- abs(2*L - 1));
    m = L - 0.5f * d;
    x = d * (1 - abs(((H / 60) % 2) - 1));

    if (H >= 0 && H < 60) {
        R = d + m;
        G = x + m;
        B = m;
    } else if (H >= 60 && H < 120) {
        R = x + m;
        G = d + m;
        B = m;
    } else if (H >= 120 && H < 180) {
        R = m;
        G = d + m;
        B = x + m;
    } else if (H >= 180 && H < 240) {
        R = m;
        G = x + m;
        B = d + m;
    } else if (H >= 240 && H < 300) {
        R = x + m;
        G = m;
        B = d + m;
    } else if (H >= 300 && H < 360) {
        R = d + m;
        G = m;
        B = x + m;
    }
    
    return half3(R, G, B);
}

/**
 * \brief Blends 2 colors together using HSLA
 * \param from first color
 * \param to second color
 * \param t blend amount
 * \return blended color
 */
float4 BlendHSL(float4 from, float4 to, float t) {
    //convert RGB colors into HSL
    float4 fromHSL = RGBAtoHSLA(from);
    float4 toHSL = RGBAtoHSLA(to);
    
    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if ((fromHSL.x == 0 || toHSL.x == 0) && (fromHSL.y == 0 || toHSL.y == 0)) {
        //The solution to the problem is to set the HUE to whichever color has one
        //so we just set HUE to the highest value of either colors.
        x = max(fromHSL.x, toHSL.x);
    } else {
        //In any other case we just use a linear interpolation.
        x = lerp(fromHSL.x, toHSL.x, t);
    }

    return HSLAtoRGBA(float4( x, lerp(fromHSL.yzw, toHSL.yzw, t)));
}

/**
 * \brief Blends 2 colors together using HSLA
 * \param from first color
 * \param to second color
 * \param t blend amount
 * \return blended color
 */
half4 BlendHSL(half4 from, half4 to, half t) {
    //convert RGB colors into HSL
    half4 fromHSL = RGBAtoHSLA(from);
    half4 toHSL = RGBAtoHSLA(to);
    
    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if ((fromHSL.x == 0 || toHSL.x == 0) && (fromHSL.y == 0 || toHSL.y == 0)) {
        //The solution to the problem is to set the HUE to whichever color has one
        //so we just set HUE to the highest value of either colors.
        x = max(fromHSL.x, toHSL.x);
    } else {
        //In any other case we just use a linear interpolation.
        x = lerp(fromHSL.x, toHSL.x, t);
    }

    return HSLAtoRGBA(half4( x, lerp(fromHSL.yzw, toHSL.yzw, t)));
}

/**
 * \brief Blends 2 colors together using HSL
 * \param from first color
 * \param to second color
 * \param t blend amount
 * \return blended color
 */
float3 BlendHSL(float3 from, float3 to, float t) {
    //convert RGB colors into HSL
    float3 fromHSL = RGBtoHSL(from);
    float3 toHSL = RGBtoHSL(to);
    
    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if ((fromHSL.x == 0 || toHSL.x == 0) && (fromHSL.y == 0 || toHSL.y == 0)) {
        //The solution to the problem is to set the HUE to whichever color has one
        //so we just set HUE to the highest value of either colors.
        x = max(fromHSL.x, toHSL.x);
    } else {
        //In any other case we just use a linear interpolation.
        x = lerp(fromHSL.x, toHSL.x, t);
    }

    return HSLtoRGB(float3( x, lerp(fromHSL.yz, toHSL.yz, t)));
}

/**
 * \brief Blends 2 colors together using HSLA
 * \param from first color
 * \param to second color
 * \param t blend amount
 * \return blended color
 */
half3 BlendHSL(half3 from, half3 to, half t) {
    //convert RGB colors into HSL
    half3 fromHSL = RGBtoHSL(from);
    half3 toHSL = RGBtoHSL(to);
    
    //check for HUE being 0 and Saturation being 0 in either colors
    //this would mean we are blending between a color to a grey and
    //because greys have no HUE their default value is 0.
    //In this case a lerp is not the correct way to blend the color as
    //it would result in a color shifting gradient.
    float x;
    if ((fromHSL.x == 0 || toHSL.x == 0) && (fromHSL.y == 0 || toHSL.y == 0)) {
        //The solution to the problem is to set the HUE to whichever color has one
        //so we just set HUE to the highest value of either colors.
        x = max(fromHSL.x, toHSL.x);
    } else {
        //In any other case we just use a linear interpolation.
        x = lerp(fromHSL.x, toHSL.x, t);
    }

    return HSLtoRGB(half3( x, lerp(fromHSL.yz, toHSL.yz, t)));
}

#endif