#ifndef HAIR_HLSL
#define HAIR_HLSL

float4 HairHighlight(float3 positionWS, float3 ogNormal, float2 uv, HairData hairData)
{
    float floatSeed1 = GenerateHashedRandomFloat(hairData.noiseSeed);
    float floatSeed2 = GenerateHashedRandomFloat(hairData.noiseSeed + 1);
    float floatSeed3 = GenerateHashedRandomFloat(hairData.noiseSeed + 2);
    
    // edit sampling of perlin noise texture
    float perlin_noise = SAMPLE_TEXTURE2D(hairData.perlinNoiseTex, sampler_LinearRepeat,
                            uv * float2(hairData.noiseStretch, .15) + float2(floatSeed1, floatSeed2)).r;

    perlin_noise = pow(perlin_noise, 2);

    perlin_noise *= hairData.noiseStrength;
    
    //center noise
    perlin_noise = perlin_noise * 2 - 1;
    perlin_noise -= hairData.noiseStrength * 0.5;
    perlin_noise /= 2;

    float3 viewForward = normalize(GetViewForwardDir());

    // IF the rotation of the normal of the hair is almost the same as the view forward, show the highlight
    // get angle between up and view forward
    float3 upVec = SafeNormalize(cross(hairData.forwardWS, hairData.rightWS));
    float viewAngle = acos(-dot(upVec, viewForward));

    float3 center = TransformWorldToObject(hairData.centerWS) - float3(0, -.1 ,0);
    
    float3 normal = SafeNormalize(TransformWorldToObject(positionWS) - center);
    float3 stretchedNormal = normal * float3(1, 3, 1);
    
    float cosine = clamp(dot(stretchedNormal, upVec) - 0.3, -1.0, 1.0);
    float normalAngle = acos(cosine);

    // Layer some noise
    float highFreqNoise = (SAMPLE_TEXTURE2D(
        hairData.perlinNoiseTex, 
        sampler_LinearRepeat, 
        uv * float2(50, 0.0) + float2(0, floatSeed1)
    ).r ) * .4;

    float midFreqNoise = (SAMPLE_TEXTURE2D(
        hairData.perlinNoiseTex, 
        sampler_LinearRepeat, 
        uv * float2(0.7, 0.0) + float2(0, floatSeed2)
    ).r ) * .2;

    float lowFreqNoise = (SAMPLE_TEXTURE2D(
        hairData.perlinNoiseTex, 
        sampler_LinearRepeat, 
        uv * float2(0.1, 0.0) + float2(0, floatSeed3)
    ).r ) * .9;

    float baseDiff = (viewAngle - normalAngle) + highFreqNoise + midFreqNoise + lowFreqNoise;
    float mirroredNoise = baseDiff < 0.0 ? -abs(perlin_noise) : abs(perlin_noise);
    float angleDiff = baseDiff + mirroredNoise;
    
    float angleFade = .2;
    
    float highlightPercentage = angleDiff < 0 ? 
        saturate(hairData.highlightLength + angleDiff) : 
        smoothstep(angleFade, 0, angleDiff);  // Reversed parameters for fade-out
    
    //but then we also some more noise on top for variety
    float noise = SAMPLE_TEXTURE2D(hairData.perlinNoiseTex, sampler_LinearRepeat, uv * float2(1,1) + float2(floatSeed1, floatSeed2)).r * 0.2;
    angleDiff += noise;

    float fresnel = saturate(pow(saturate(dot(normalize(GetWorldSpaceViewDir(positionWS)), ogNormal)), hairData.fresnelPower));
    float fakeFresnel = saturate(1.0 - pow(saturate(dot(normalize(GetWorldSpaceViewDir(positionWS)), normal) + hairData.fresnelBias), hairData.fresnelPower));
    
    float4 hairHighlight = float4(0, 0, 0, 0);
    float alpha;
    if (angleDiff < angleFade)
    {
        alpha = angleDiff < 0 ?
            pow(highlightPercentage, hairData.highlightExponent) * hairData.highlightStrength * fresnel * fakeFresnel :
            hairData.highlightStrength * fresnel * fakeFresnel;
        
        hairHighlight = hairData.highlightColor;
        hairHighlight.a *= alpha;
        hairHighlight = saturate(hairHighlight);
    }

    return hairHighlight;
}

#endif