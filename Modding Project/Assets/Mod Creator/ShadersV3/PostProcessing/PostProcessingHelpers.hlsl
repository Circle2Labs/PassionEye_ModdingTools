#ifndef POSTPROCESSINGHELPERS_HLSL
#define POSTPROCESSINGHELPERS_HLSL

float4 BoxBlur(Texture2D tex, float2 uv, float radiusFloat)
{
    float2 uvIncrement = 1.0 / _ScreenSize.xy;
    int radius = (int)radiusFloat;

    float4 colorSum = 0;
    float weightSum = 0;

    for(int i=-radius; i<=radius; i++)
    {
        for(int j=-radius; j<=radius; j++)
        {
            float2 uvs = uv + float2(i, j) * uvIncrement;
            float4 colorSample = SAMPLE_TEXTURE2D(tex, sampler_LinearClamp, uvs);
            float weight = 1;
            colorSum += colorSample * weight;
            weightSum += weight;
        }
    }
    return colorSum / weightSum;
}

float gaussian_matrix_3x3[9];
float gaussian_sum_3x3;

float4 GaussianBlur3x3(Texture2D tex, float2 uv)
{
    gaussian_sum_3x3 = 16;

    gaussian_matrix_3x3[0] = 1;
    gaussian_matrix_3x3[1] = 2;
    gaussian_matrix_3x3[2] = 1;
    gaussian_matrix_3x3[3] = 2;
    gaussian_matrix_3x3[4] = 4;
    gaussian_matrix_3x3[5] = 2;
    gaussian_matrix_3x3[6] = 1;
    gaussian_matrix_3x3[7] = 2;
    gaussian_matrix_3x3[8] = 1;

    float2 uvIncrement = 1.0 / _ScreenSize.xy;

    float4 colorSum = 0;

    for(int i=-1; i<=1; i++)
    {
        for(int j=-1; j<=1; j++)
        {
            float2 uvs = uv + float2(i, j) * uvIncrement;
            float4 colorSample = SAMPLE_TEXTURE2D(tex, sampler_LinearClamp, uvs);
            if(colorSample.r > 1 || colorSample.g > 1 || colorSample.b > 1)
            {
                return float4(1, 0, 0, 1);
            } else
            {
                colorSum += colorSample * gaussian_matrix_3x3[(i + 1) * 3 + j + 1];
            }
        }
    }
    
    return colorSum / gaussian_sum_3x3;
}

float gaussian_matrix_5x5[25];

float gaussian_sum_5x5;
float4 GaussianBlur5x5(Texture2D tex, float2 uv)
{
    gaussian_sum_5x5 = 273;

    gaussian_matrix_5x5[0] = 1;
    gaussian_matrix_5x5[1] = 4;
    gaussian_matrix_5x5[2] = 7;
    gaussian_matrix_5x5[3] = 4;
    gaussian_matrix_5x5[4] = 1;
    gaussian_matrix_5x5[5] = 4;
    gaussian_matrix_5x5[6] = 16;
    gaussian_matrix_5x5[7] = 26;
    gaussian_matrix_5x5[8] = 16;
    gaussian_matrix_5x5[9] = 4;
    gaussian_matrix_5x5[10] = 7;
    gaussian_matrix_5x5[11] = 26;
    gaussian_matrix_5x5[12] = 41;
    gaussian_matrix_5x5[13] = 26;
    gaussian_matrix_5x5[14] = 7;
    gaussian_matrix_5x5[15] = 4;
    gaussian_matrix_5x5[16] = 16;
    gaussian_matrix_5x5[17] = 26;
    gaussian_matrix_5x5[18] = 16;
    gaussian_matrix_5x5[19] = 4;
    gaussian_matrix_5x5[20] = 1;
    gaussian_matrix_5x5[21] = 4;
    gaussian_matrix_5x5[22] = 7;
    gaussian_matrix_5x5[23] = 4;
    gaussian_matrix_5x5[24] = 1;
    
    float2 uvIncrement = 1.0 / _ScreenSize.xy;

    float4 colorSum = 0;

    for(int i=-2; i<=2; i++)
    {
        for(int j=-2; j<=2; j++)
        {
            float2 uvs = uv + float2(i, j) * uvIncrement;
            float4 colorSample = SAMPLE_TEXTURE2D(tex, sampler_LinearClamp, uvs);
            if(colorSample.r > 1 || colorSample.g > 1 || colorSample.b > 1)
            {
                return float4(1, 0, 0, 1);
            } else
            {
                colorSum += colorSample * gaussian_matrix_5x5[(i + 2) * 5 + j + 2];
            }
        }
    }
    
    return colorSum / gaussian_sum_5x5;
}

#endif
