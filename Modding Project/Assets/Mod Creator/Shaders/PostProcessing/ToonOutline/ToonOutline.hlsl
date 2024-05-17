/*
case BufferType.NormalWorldSpace:
s.AppendLine("return SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv);");
break;
case BufferType.MotionVectors:
s.AppendLine("uint2 pixelCoords = uint2(uv * _ScreenSize.xy);");
s.AppendLine($"return LOAD_TEXTURE2D_X_LOD(_MotionVectorTexture, pixelCoords, 0).xy;");
break;
case BufferType.BlitSource:
s.AppendLine("uint2 pixelCoords = uint2(uv * _ScreenSize.xy);");
s.AppendLine($"return LOAD_TEXTURE2D_X_LOD(_BlitTexture, pixelCoords, 0);");
break;
*/

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//shadergraph includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

#include "Assets/GameAssets/Shaders/ColorSpaces.hlsl"

float4 pack(float depth)
{
    const float4 bitSh = float4(256.0 * 256.0 * 256.0, 256.0 * 256.0, 256.0, 1.0);
    const float4 bitMsk = float4(0, 1.0 / 256.0, 1.0 / 256.0, 1.0 / 256.0);
    float4 comp = frac(depth * bitSh);
    comp -= comp.xxyz * bitMsk;
    return comp;
}

float unpack(float depth)
{
    float4 packedZValue = depth;
    const float4 bitShifts = float4(1.0 / (256.0 * 256.0 * 256.0), 1.0 / (256.0 * 256.0), 1.0 / 256.0, 1);
    float unpackedDepth = dot(packedZValue , bitShifts); 
    return unpackedDepth;
}

//sobel matrix
float3x3 sobelX = float3x3(-1, 0, 1,
                           -2, 0, 2,
                           -1, 0, 1);

float3x3 sobelY = float3x3(-1, -2, -1,
                            0,  0,  0,
                            1,  2,  1);

TEXTURE2D (_BlitTexture);

float SampleDepth(float2 UV)
{
        return LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV), _ZBufferParams);
}

float SampleLinear01Depth(float2 uv)
{
    return Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv), _ZBufferParams);
}

float3 SampleNormal(float2 uv)
{
    return SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv);
}

float OutlineFadeDistance(float distStart, float distEnd, float2 uv)
{
    #ifdef UNITY_REVERSED_Z
    float farPlane = 1/_ZBufferParams.w;
    #else
    float farPlane = _ZBufferParams.y / _ZBufferParams.w;
    #endif

    float linearDepth = SampleLinear01Depth(uv);

    //distStart = 3, distEnd = 10
    float distanceMeters = ((linearDepth) * farPlane) + 0.5; //.5 * 10 = 5m
    float fadeStart = lerp(0, 1, distStart / farPlane); //0, 1, 3/10 = .3
    float fadeEnd = lerp(0, 1, distEnd / farPlane); //0, 1, 10/10 = 1

    //if distanceMeter is less than distStart, return >= 1
    //else if distanceMeter is greater than distEnd, return <= 0
    //else lerp between 1 and 0
    return lerp(1, 0, saturate((distanceMeters - distStart) / (distEnd - distStart)));

}

void ToonOutline_float(float2 uv, float depthThreshold, float normalsThreshold, float4 outlineColor, float radiusFloat, float3 viewSpaceDir,
    float angleFixScale, float angleFixPower,
    float fadeStart, float fadeEnd,
    bool debug, float debugTransparency, out float4 color)
{
    float2 uvIncrement = 1.0 / _ScreenSize.xy;
    int radius = (int)radiusFloat * length(_ScreenSize.xy / float2(1920, 1080));

    float3 normalPixel = SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv);
    float depthPixel = SampleDepth(uv);

    float edgeXDepth = 0;
    float edgeYDepth = 0;
    float edgeXNormal = 0;
    float edgeYNormal = 0;

    float minDepth = 1.0;
    float maxDepth = 0.0;

    /*
    float3 viewNormal = normalPixel * 2 - 1;
    float NdotV = (dot(viewNormal, -viewSpaceDir));
    //color = float4(viewSpaceDir, 1);
    color = NdotV;
    return;*/

    float NdotV = viewSpaceDir;
    
    float normalThreshold = normalsThreshold * angleFixScale;
    normalThreshold = pow(normalThreshold, angleFixPower);
        
    float rem = radiusFloat - radius;
    
    for(int i=-radius; i<=radius; i++)
    {
        for(int j=-radius; j<=radius; j++)
        {
            float2 uvs = uv + float2(i, j) * uvIncrement;
            float3 normalSample = SampleNormal(uvs);
            float depthSample = SampleDepth(uvs);

            if (depthSample < minDepth)
            {
                minDepth = depthSample;
            }

            if (depthSample > maxDepth)
            {
                maxDepth = depthSample;
            }

            float sobelSampleXDepth = (depthSample - depthPixel);
            float sobelSampleYDepth = (depthSample - depthPixel);
            
            float sobelSampleXNormal = length(normalSample - normalPixel);
            float sobelSampleYNormal = length(normalSample - normalPixel);

            float i_calc = i;
            float j_calc = j;
            if (i == abs(radius) && rem > 0)
            {
                i_calc = radius - (1 - rem);
            }

            if (j == abs(radius) && rem > 0)
            {
                j_calc = radius - (1 - rem);
            }
            
            edgeXDepth += sobelSampleXDepth * i_calc;
            edgeYDepth += sobelSampleYDepth * j_calc;

            edgeXNormal += sobelSampleXNormal * i_calc;
            edgeYNormal += sobelSampleYNormal * j_calc;
        }
    }

    //normalize based on radius
    edgeXDepth /= radius;
    edgeYDepth /= radius;
    edgeXNormal /= radius;
    edgeYNormal /= radius;
    
    float edgeDepth = sqrt(edgeXDepth * edgeXDepth + edgeYDepth * edgeYDepth);
    float edgeNormal = sqrt(edgeXNormal * edgeXNormal + edgeYNormal * edgeYNormal);

    float edge = max(edgeDepth, edgeNormal);
    
    float4 blitPixel = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);

    if (edgeDepth > depthThreshold * radius * maxDepth)
    {
        float linearFade = OutlineFadeDistance(fadeStart, fadeEnd, uv);
        color = linearFade;
        
        if(debug)
        {
            if(edgeDepth > depthThreshold * radius * maxDepth)
                color = float4(0,0,1,1); //depth
            else
                color = float4(0,1,0,1); //normal

            //color.b = lerp(blitPixel, outlineColor, saturate(1.60-minDepth));
        }
    }
    else if (edgeNormal > normalThreshold * radius)
    {
        float linearFade = OutlineFadeDistance(fadeStart, fadeEnd/6, uv);
        color = linearFade;
        
        if(debug)
        {
            if(edgeDepth > depthThreshold * radius * maxDepth)
                color = float4(0,0,1,1); //depth
            else
                color = float4(0,1,0,1); //normal

            //color.b = lerp(blitPixel, outlineColor, saturate(1.60-minDepth));
        }
    }
    else
    {
        color = 0;
        if(debug)
            color = float4(0,0,0,1);
    }
    //color = 1;

    if(debug)
    {
        float3 hsvPixel = RGBtoHSV(blitPixel);
        hsvPixel.z -= 0.3;
        float3 rgbPixel = HSVtoRGB(hsvPixel);
        //color = lerp(blitPixel, color, debugTransparency);
        color = lerp(color,float4(rgbPixel,1), debugTransparency);
    }
    
    //color = float4(normalPixel, 1.0);
    //color = float4(NdotV, NdotV, NdotV, 1);
    
    //color = float4(depthPixel, depthPixel, depthPixel, 1);
    //edge = saturate(edge);
    //color = float4(edge, edge, edge, 1);
    //color = float4(pack(depthPixel).xyz, 1);
}