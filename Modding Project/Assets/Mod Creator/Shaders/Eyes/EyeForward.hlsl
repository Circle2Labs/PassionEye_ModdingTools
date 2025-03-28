#pragma vertex vert
#pragma fragment frag

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../SDF/Utils.hlsl"
#include "../ToonShaders/CustomLighting.hlsl"

#include_with_pragmas "../ToonShaders/ToonVariants.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv           : TEXCOORD0;
    float4 lmuv         : TEXCOORD1;
};

struct Varyings
{
    float4 positionHCS  : SV_POSITION;
    float3 normalOS     : NORMAL;
    float2 uv           : TEXCOORD0;
    float4 lmuv         : TEXCOORD1;
    float3 positionWS   : TEXCOORD7;
};            

Varyings vert(Attributes IN)
{
    Varyings OUT;
    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
    OUT.normalOS = IN.normalOS;
    OUT.uv = IN.uv;
    OUT.lmuv = IN.lmuv;

    return OUT;
}

float _IsLeftEye;

#define EYE_PART_VARS(PARTNAME) \
    float _Enable##PARTNAME; \
    float _Use##PARTNAME##Texture; \
    float _##PARTNAME##ColorMode; \
    Texture2D _##PARTNAME##Texture; \
    SamplerState sampler_##PARTNAME##Texture; \
    float4 _##PARTNAME##Color1; \
    float4 _##PARTNAME##Color2; \
    int _##PARTNAME##GradientType; \
    float _##PARTNAME##GradientMidpoint; \
    float _##PARTNAME##GradientSharpness; \
    float _##PARTNAME##XPos; \
    float _##PARTNAME##YPos; \
    float _##PARTNAME##XScale; \
    float _##PARTNAME##YScale;

//create macro for adding eye parts
#define ADD_EYE_PART(PARTNAME, UV) AddEyePart(_Enable##PARTNAME,\
    _Use##PARTNAME##Texture, _##PARTNAME##ColorMode, _##PARTNAME##Texture,\
    _##PARTNAME##Color1, _##PARTNAME##Color2, _##PARTNAME##GradientType,\
    _##PARTNAME##GradientMidpoint, _##PARTNAME##GradientSharpness, _##PARTNAME##XPos,\
    _##PARTNAME##YPos, _##PARTNAME##XScale, _##PARTNAME##YScale, UV)

#define MAX_SDF_LAYERS 128

int SDFQueueIndex = 0;
float4 SDFQueue[MAX_SDF_LAYERS]; 

void addSDF(float sdf, float4 color1, float4 color2=float4(0.0, 0.0, 0.0, 0.0), float t=0.0, float alpha=1.0)
{
    float4 coloredSDF = colorSDF(sdf, color1, color2, t);
    coloredSDF.a *= alpha;
    
    SDFQueue[SDFQueueIndex] = coloredSDF;
    SDFQueueIndex++;
}

void addColor(float4 color) //to handle textures
{
    SDFQueue[SDFQueueIndex] = color;
    SDFQueueIndex++;
}

float4 findLastValidSDF(float4 bg_color)
{
    for(int i = SDFQueueIndex - 1; i >= 0; i--)
    {
        if(SDFQueue[i].a > 0.0)
        {
            return SDFQueue[i];
        }
    }

    return bg_color;
}

float4 renderSDF(float4 bg_color = (0).xxxx)
{
    float4 output = bg_color;
    for(int i = 0; i < SDFQueueIndex; i++)
    {
        output = float4(BlendOkLab(output, SDFQueue[i], SDFQueue[i].a), 1.0);
    }

    return saturate(output);
}

//perlin noise
Texture2D _PerlinTex;
SamplerState sampler_PerlinTex;

//iris lines
//from_angle and to_angle are in percentage of the circle
float iris_lines_overlay(float2 uv, float2 center, float2 radius, float line_width, float line_count,
    float from_angle, float to_angle, float invert, uint seed)
{
    //do the uv thingy
    uv = deformScaleUV(uv, radius);
    center = deformScaleUV(center, radius);
    
    from_angle *= PI*2;
    to_angle *= PI*2;
    
    line_count = floor(line_count);
    
    // first we get the angle from the left side of the iris based on the center
    float angle = atan2(uv.y - center.y, uv.x - center.x);

    //shift the angle by -90 degrees so that the angle starts from the top
    //and invert the angle so that it goes clockwise
    angle -= PI/2;
    angle = -angle;
    
    if(angle < 0.0)
    {
        angle += 2.0*PI;
    }

    //eventually invert angle selection
    if(!invert){
        if(angle < from_angle || angle > to_angle)
        {
            return 0.0;
        }
    } else {
        if(angle > from_angle && angle < to_angle)
        {
            return 0.0;
        }
    }
    
    float frac_grad = frac((3.141-angle) / 6.2831 * line_count);

    // we generate a random value based on the seed
    float rand_x = GenerateHashedRandomFloat(seed);

    uint new_seed = seed * 2;

    float rand_y = GenerateHashedRandomFloat(new_seed);
    
    // we quantize the angle based on the number of lines, so that we have
    // a consistent noise value for each line regardless of its size
    float angle_step = (PI*2) / line_count;
    bool is_odd = fmod(line_count, 2) > 0.0;
    angle += (is_odd ? angle_step / 2 : 0);
    float angle_quant = (floor(angle / angle_step) * angle_step);
    
    float line_length = SAMPLE_TEXTURE2D(_PerlinTex, sampler_PerlinTex, float2(angle_quant + rand_x, rand_y)) + .5;

    //clamp to avoid going over the border
    line_length = clamp(line_length, 0.0, 1);
    
    if(frac_grad > 0.5)
    {
        frac_grad = remap(0.5, 1.0, 0.5, 0, frac_grad);
    }
    
    float line_quant = 1.0-frac_grad;
    
    // Calculate the distance from the center
    float dist_from_center = distance(uv, center);

    // Create a smooth fade based on distance from the center and line length
    float fade = smoothstep(line_length - 0.1, line_length, dist_from_center);

    // Calculate the intensity of the fragment with fade
    float frag_intensity = (1.0 - fade) * line_quant;

    frac_grad -= ((1-line_width) / 2.0);
    return 5.0 * (frac_grad * frag_intensity);
}

EYE_PART_VARS(OuterIris)
EYE_PART_VARS(InnerIris)

float _EnableIrisLines;
float4 _IrisLinesColor;
float _IrisLinesCount;
float _IrisLinesWidth;
float _IrisLinesAngleStart;
float _IrisLinesAngleEnd;
float _IrisLinesInvertAngle;
int _IrisLinesSeed;

EYE_PART_VARS(Pupil)

float4 _BackgroundColor;

float _EnableHighlight;
float _UseHighlightTexture;
Texture2D _HighlightTexture;
float _HighlightColorMode;
float _HighlightScale;
float4 _HighlightColor;
float _AutoHighlightColor;
float _HighlightDeadzone;
float _HighlightDeadzoneAttenuation;
float _EnableSecondaryHighlight;
float _UseSecondaryHighlightTexture;
Texture2D _SecondaryHighlightTexture;
float _SecondaryHighlightColorMode;
float _SecondaryHighlightScale;
float _SecondaryHighlightMirrorX;
float _SecondaryHighlightMirrorY;

//lighting
float _EnableLighting;
float _MinimumLightLevel;

//high part shadow
float _EnableShadow;
float4 _ShadowColor;
float _ShadowHeight;
float _ShadowHorizontalOffset;
float _ShadowCurve;
float _ShadowSmoothness;

//teary
float _IsTeary;
float _TearySpeed;
float _TearyAmount;

//face direction for highlight
float3 _FaceCenter;
float3 _FaceFwdVec;

//movement
float _MovementX;
float _MovementY;

float AddEyePart(float enable, float useTexture, float colorMode, Texture2D _texture, float4 color1, float4 color2,
                int gradientType, float gradientMidpoint, float gradientSharpness, float xPos, float yPos,
                float xScale, float yScale, float2 uv)
{
    /*if(enable == 0.0)
    {
        return 1.0;
    }*/
    
    float4 tmp_color = color1;
    float4 tmp_color2 = color2;
    float t;
    
    float2 center = float2(xPos, yPos);
    float2 scale = float2(xScale, yScale);
    
    float sdf;
    float4 tex;

    if(useTexture)
    {
        tex = calcTexSDF(uv, scale, center, gradientType,
                                   gradientMidpoint, gradientSharpness, _texture);

        t = calcSDFGradientEllipse(uv, scale, center, gradientType,
           gradientMidpoint, gradientSharpness);
        
        if(colorMode == SDF_COLORBLEND_MULTIPLY)
        {
            tex = multiplyColorSDF(tex, tmp_color, tmp_color2, t);
            if (tex.a <= 0.0 || enable == 0.0)
            {
                tex.a = 0.0;
            }
            addColor(tex);
            return tex.a;
        } else if(colorMode == SDF_COLORBLEND_COLORIZE)
        {
            //we use the a channel of the texture as an sdf value
            if (tex.a <= 0.0 || enable == 0.0)
            {
                tmp_color.a = 0.0;
                tmp_color2.a = 0.0;
            }
            addSDF(tex.a, tmp_color, tmp_color2, t, tex.a);
            return tex.a;
        } else
        {
            addColor(tex);
            return tex.a;
        }
    } else
    {
        sdf = ellipseSDF(uv, scale, center);
        t = calcSDFGradientEllipse(uv, scale, center, gradientType,
                            gradientMidpoint, gradientSharpness);

        if(sdf > 0.0 || enable == 0.0)
        {
            tmp_color.a = 0.0;
            tmp_color2.a = 0.0;
        }

        addSDF(sdf, tmp_color, tmp_color2, t);
        return sdf;
    }
    
    return 1.0;
}

float4 frag(Varyings IN) : SV_TARGET
{
    SDFQueueIndex = 0;
    //clear sdf queue
    for(int i = 0; i < MAX_SDF_LAYERS; i++)
    {
        SDFQueue[i] = float4(0.0, 0.0, 0.0, 0.0);
    }
    
    float2 uv = IN.uv;

    //scale uv
    uv = deformScaleUV(uv, float2(1., 1.));
    
    //move uv
    uv = uv + float2(.042, -0.042);

    //movement
    uv = uv + float2(_MovementX, _MovementY);
    
    ADD_EYE_PART(OuterIris, uv);
    float inner_iris_sdf = ADD_EYE_PART(InnerIris, uv);

    float2 inner_iris_center = float2(_InnerIrisXPos, _InnerIrisYPos);
    float2 inner_iris_scale = float2(_InnerIrisXScale, _InnerIrisYScale);

    //iris lines
    //we wanna supersample the iris lines

    //we also want to change the supersampling width based on camera distance

    float camera_distance = length(GetCameraPositionWS() - _FaceCenter);

    float max_aa_width = 4096.0;
    float min_aa_width = 256.0;

    float aa_width = remap(.25, 0, min_aa_width, max_aa_width, camera_distance);
    //lemme debug the width
    //return camera_distance;
    //return remap(512.0, 4096.0, 0.0, 1.0, aa_width);
    aa_width = clamp(aa_width, min_aa_width, max_aa_width);

    
    float iris_lines = 0;
    for(int x = -1; x <= 1; x++){
        for(int y = -1; y <= 1; y++){
            float2 uv_offset = uv + float2(x, y) / aa_width;
            iris_lines += iris_lines_overlay(uv_offset, inner_iris_center, inner_iris_scale,
                _IrisLinesWidth, _IrisLinesCount, _IrisLinesAngleStart, _IrisLinesAngleEnd,
                _IrisLinesInvertAngle, _IrisLinesSeed);
        }
    }
    
    iris_lines /= 9.0;

    //no supersampling for comparison
/*
    float iris_lines = iris_lines_overlay(uv, inner_iris_center, inner_iris_scale,
                _IrisLinesWidth, _IrisLinesCount, _IrisLinesAngleStart, _IrisLinesAngleEnd,
                _IrisLinesInvertAngle, _IrisLinesSeed);
*/
    //float t = pow(1.0 - iris_lines, 3);

    //iris lines is not a real SDF, so we just add it as a color unless it's full black
    float4 iris_lines_color = saturate(_IrisLinesColor);
    iris_lines_color.a *= iris_lines * (inner_iris_sdf > 0.0 ? 0.0 : 1.0) * _EnableIrisLines;
    addColor(saturate(iris_lines_color));
    
    ADD_EYE_PART(Pupil, uv);

    float2 shadowUV = uv;
    float4 shadowColor = _ShadowColor;

    // counteract the movement
    shadowUV -= float2(_MovementX, _MovementY);
    
    if (_IsLeftEye > 0.0)
    {
        shadowUV.x = 1.0 - shadowUV.x;
    }
    
    shadowUV.x -= 1 - _ShadowHorizontalOffset;
    
    // x, a, b, c -> ax^2 + bx + c as an sdf
    float sdf = -quadraticSDF(shadowUV, _ShadowCurve, 0, _ShadowHeight);

    _ShadowSmoothness *= _ShadowSmoothness;
    float alphaMult = smoothstep(-_ShadowSmoothness/2.0, _ShadowSmoothness/2.0, sdf);
    shadowColor.a *= alphaMult;

    float shadowAlpha = shadowColor.a;
    shadowColor.a = 1.0;

    if(_EnableShadow < 1.0)
    {
        shadowColor.a = 0.0;
    }
    
    addSDF(sdf, shadowColor, shadowColor, 0.0, shadowAlpha);
    
    //highlight
    if(_EnableHighlight > 0.0)
    {
        //the highlight moves depending on the light direction
    
        float3 light_direction = GetMainLight().direction;
    
        // calculate the reflection vector
        float3 reflectionVector = reflect(-light_direction, _FaceFwdVec);

        // project the reflection vector onto 1/3 of the uv space
        float2 highlightPos = (reflectionVector.xy + 1.5) / 3.0;

        //invert y axis
        highlightPos.y = 1.0 - highlightPos.y;

        if(_IsTeary > 0.0)
        {
            //slightly wiggle the highlight to simulate teary eyes
            float2 offset = float2(0.0, 0.0);
            offset.x = sin(_Time.y * _TearySpeed*100) * _TearyAmount;
            highlightPos += offset;
        }

        //if auto highlight color is enabled, set the highlight color to the main light color
        if(_AutoHighlightColor > 0.0)
        {
            _HighlightColor = _MainLightColor;
        }
        
        //check if the highlight is in the deadzone in the center
        //start lerping to inner iris color
        float dist = length(highlightPos - float2(.5, .5));
        if(dist < _HighlightDeadzone)
        {
            //lerp up to 0.05 distance
            float lerp_amount = saturate(remap(_HighlightDeadzone, _HighlightDeadzone/2.0,
                                        0.0, 1.0,dist));
            _HighlightColor = lerp(_HighlightColor, findLastValidSDF(_BackgroundColor),
                                   lerp_amount * _HighlightDeadzoneAttenuation);
        }
        
        //add the highlight
        float2 highlightSize = _UseHighlightTexture ? float2(.15, .15) : float2(0.08, 0.15);
        AddEyePart(_EnableHighlight, _UseHighlightTexture, _HighlightColorMode, _HighlightTexture,
         _HighlightColor, _HighlightColor, 0, 0, 0,
          highlightPos.x, highlightPos.y, _HighlightScale * highlightSize.x, _HighlightScale * highlightSize.y, uv);

        //secondary highlight
        highlightPos.x = _SecondaryHighlightMirrorX > 0.0 ? 1.0 - highlightPos.x : highlightPos.x;
        highlightPos.y = _SecondaryHighlightMirrorY > 0.0 ? 1.0 - highlightPos.y : highlightPos.y;
        
        float2 secondaryHighlightSize = _UseSecondaryHighlightTexture ? float2(.06, .06) : float2(0.04, 0.06);
        
        AddEyePart(_EnableSecondaryHighlight, _UseSecondaryHighlightTexture, _SecondaryHighlightColorMode, _SecondaryHighlightTexture,
         _HighlightColor, _HighlightColor, 0, 0, 0,
          highlightPos.x, highlightPos.y, _SecondaryHighlightScale * secondaryHighlightSize.x,
           _SecondaryHighlightScale * secondaryHighlightSize.y, uv);
    }

    float4 color = 0;

    //apply main light
    float4 lightmapUV = 0;
    float4 shadowCoord = 0;
    OUTPUT_LIGHTMAP_UV(IN.lmuv, unity_LightmapST, lightmapUV);

    #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
        shadowCoord = ComputeScreenPos(TransformWorldToHClip(IN.positionWS));
    #else
        shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    #endif
    
    //shadow mask
    half4 shadowMask = SAMPLE_SHADOWMASK(shadowCoord);

    Light light = GetMainLight(shadowCoord, IN.positionWS, shadowMask);
    
    float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
      
    DiffuseData diff = (DiffuseData)0;
    diff = (DiffuseData)0;
    diff.smooth = 0;
    diff.lightTint = 1;
    diff.firstBndCol = saturate(_MinimumLightLevel * 2.0);
    diff.secBndCol = _MinimumLightLevel;
    diff.secBndOffset = 0.1;
    diff.NdotL = saturate(dot(normalWS, light.direction)) * light.shadowAttenuation * light.distanceAttenuation;
    diff.SSSPower = 1.0;
    diff.SSSOffset = float4(0.0, 0.0, 0.0, 1.0);

    color.rgb = DirectDiffuse(diff, light.color);
    
    //return shadowMask;

    //apply secondary lights
    #define _ADDITIONAL_LIGHTS 1
    #if _ADDITIONAL_LIGHTS
        uint numLights = GetAdditionalLightsCount();

        float3 diffuse = 0.0;
    
        [unroll]
        for(uint i = 0; i < numLights; ++i) {
            shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
            light = GetAdditionalLight(i, IN.positionWS, shadowMask);
            diff.NdotL = saturate(dot(normalWS, light.direction)) * light.shadowAttenuation * light.distanceAttenuation;
            diffuse += DirectDiffuse(diff, light.color);
        }
        color.rgb += diffuse;
    #endif

    if (_EnableLighting > 0.0)
        return renderSDF(_BackgroundColor) * color;
    else
        return renderSDF(_BackgroundColor);
}

