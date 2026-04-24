#ifndef SDF_UTILS
#define SDF_UTILS

#include "../Utils.hlsl"
#include "../ColorSpaces.hlsl"
#include "../Beziers.hlsl"

#define SDF_GRADIENT_X 0
#define SDF_GRADIENT_Y 1
#define SDF_GRADIENT_CIRCLE 2

#define SDF_COLORBLEND_NONE 0
#define SDF_COLORBLEND_MULTIPLY 1
#define SDF_COLORBLEND_COLORIZE 2

float circleSDF(float2 uv, float2 center, float radius)
{
    return length(uv - center) - radius;
}

float4 multiplyColorSDF(float4 tex_color, float4 color1, float4 color2, float t)
{
    return tex_color * float4(BlendOkLab(color1, color2, t), 1.0);
}

float4 colorizeColorSDF(float4 tex_color, float4 color1, float4 color2, float t) //yeah i know
{
    return float4(BlendOkLab(color1, color2, t), tex_color.a);
}

float4 colorSDF(float sdf, float4 color1, float4 color2=0.0, float t=0.0)
{
    //if color2 is set, we blend the colors
    if(color2.a > 0.0)
    {
        return float4(BlendOkLab(color1, color2, t), color1.a);
    } else
    {
        return color1 * step(sdf, 0.0);
    }
}

float ellipseSDF(float2 uv, float2 axis, float2 center)
{
    //we deform the uv and center by the axis
    uv = deformScaleUV(uv, axis);
    center = deformScaleUV(center, axis);
    //we calculate the distance from the center
    return length(uv - center) - 1.0;
}

float quadraticSDF(float2 uv, float a, float b, float c)
{
    float y = a * uv.x * uv.x + b * uv.x + c;
    return y - uv.y;
}

//gradient functions

float calcSDFGradientEllipse(float2 uv, float2 radius, float2 center, float gradient_type,
                            float midpoint = 0.5, float sharpness = 0.5)
{
    float2 uv_deformed = deformScaleUV(uv, radius);
    float2 center_deformed = deformScaleUV(center, radius);
    float2 midpoint_deformed = lerp(center_deformed, uv_deformed, midpoint);

    float t;
    
    if(gradient_type == SDF_GRADIENT_X)
    {
        //left to right
        float left_border = center.x - radius.x;
        float right_border = center.x + radius.x;

        float2 remapped_uv = remap(left_border, right_border, 0.0, 1.0, uv.x);
        remapped_uv = saturate(remapped_uv);

        float distance_from_midpoint = (1.0 - sharpness);

        return saturate(smoothstep(midpoint - distance_from_midpoint,
                                        midpoint + distance_from_midpoint,
                                        remapped_uv.x));
        
    }
    else if(gradient_type == SDF_GRADIENT_Y)
    {
        //top to bottom
        float top_border = center.y - radius.y;
        float bottom_border = center.y + radius.y;

        float2 remapped_uv = remap(top_border, bottom_border, 0.0, 1.0, uv.y);
        remapped_uv = saturate(remapped_uv);

        float distance_from_midpoint = (1.0 - sharpness);

        return saturate(smoothstep(midpoint - distance_from_midpoint,
                                        midpoint + distance_from_midpoint,
                                        remapped_uv.y));
    }
    else if(gradient_type == SDF_GRADIENT_CIRCLE)
    {
        // Find distance to center
        float dist = length(uv_deformed - midpoint_deformed);
        
        t = dist / max(radius.x, radius.y);
        return saturate(smoothstep(sharpness, 1.0, t));
    }

    return 0.0;
}

float4 calcTexSDF(float2 uv, float2 scale, float2 center, int gradientType, float gradientMidpoint,
                  float gradientSharpness, Texture2D tex)
{
    float4 output;
    
    float2 calc_uv = deformScaleUV(uv, scale * 2);
    float2 calc_center = deformScaleUV(center, scale * 2);
    calc_uv = calc_uv + (float2(.5, .5) - calc_center);

    if(calc_uv.x < 0.0 || calc_uv.x > 1.0 || calc_uv.y < 0.0 || calc_uv.y > 1.0)
    {
        output = float4(0.0, 0.0, 0.0, 0.0);
    } else
    {
        output = SAMPLE_TEXTURE2D(tex, sampler_LinearClamp, calc_uv);
    }

    return output;
}

#endif
