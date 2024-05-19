Shader "Hidden/OutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline width", Range (0.0, 0.03)) = 0.01
        _Threshold ("Threshold", Range (0.0, 0.1)) = 0.05
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float3 _OutlineColor;
            float _OutlineWidth;
            float _Threshold;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraDepthNormalsTexture;

            float3x3 sobel_x = float3x3(
                -1.0, 0.0, 1.0,
                -2.0, 0.0, 2.0,
                -1.0, 0.0, 1.0
            );

            float3x3 sobel_y = float3x3(
                -1.0, -2.0, -1.0,
                0.0, 0.0, 0.0,
                1.0, 2.0, 1.0
            );

            float eps = 0.0001;

            //depth based
            /*
            fixed4 frag (v2f i) : SV_Target
            {
                //calculate pixel size
                float2 pixel = 1.0 / _ScreenParams.xy;

                float s00 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(-pixel.x, -pixel.y)));
                float s01 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(0.0, -pixel.y)));
                float s02 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(pixel.x, -pixel.y)));
                float s10 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(-pixel.x, 0.0)));
                float s11 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
                float s12 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(pixel.x, 0.0)));
                float s20 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(-pixel.x, pixel.y)));
                float s21 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(0.0, pixel.y)));
                float s22 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + float2(pixel.x, pixel.y)));
                

                float sx = s00 * sobel_x[0][0] + s01 * sobel_x[0][1] + s02 * sobel_x[0][2] +
                           s10 * sobel_x[1][0] + s11 * sobel_x[1][1] + s12 * sobel_x[1][2] +
                           s20 * sobel_x[2][0] + s21 * sobel_x[2][1] + s22 * sobel_x[2][2];

                float sy = s00 * sobel_y[0][0] + s01 * sobel_y[0][1] + s02 * sobel_y[0][2] +
                           s10 * sobel_y[1][0] + s11 * sobel_y[1][1] + s12 * sobel_y[1][2] +
                           s20 * sobel_y[2][0] + s21 * sobel_y[2][1] + s22 * sobel_y[2][2];

                sx /= 8.0;
                sy /= 8.0;

                float edge = sqrt(sx * sx + sy * sy);

                fixed4 originalColor = tex2D(_MainTex, i.uv);
                originalColor.a = 1.0;
                
                fixed4 outlineColor = fixed4(_OutlineColor, 1.0);

                //if edge is between threshold and threshold + outline width, draw outline
                if (edge > 0.03 && edge < _OutlineWidth + eps)
                {
                    return outlineColor;
                } else
                {
                    return fixed4(0.0, 0.0, 0.0, 0.0);
                }
            }*/

            
            //idmap based
            sampler2D _IdMap;
            
            fixed4 frag (v2f i) : SV_Target
            {
                //calculate pixel size
                float2 pixel = 1.0 / _ScreenParams.xy;
                float3 texColor1 = tex2D(_IdMap, i.uv).rgb;

                //this method is much easier, a pixel is an edge if it's different from any of its neighbors
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        //first check if the pixel is in bounds
                        if (i.uv.x + x * pixel.x >= 0.0 && i.uv.x + x * pixel.x <= 1.0 &&
                            i.uv.y + y * pixel.y >= 0.0 && i.uv.y + y * pixel.y <= 1.0)
                        {
                            //if the pixel is different from any of its neighbors, it's an edge
                            float3 texColor2 = tex2D(_IdMap, i.uv + float2(x * pixel.x, y * pixel.y)).rgb;
                            float3 diff = texColor1 - texColor2;
                            //if they're different and the pixel we're on is not black, it's an edge
                            if(diff.x > 0.0 || diff.y > 0.0 || diff.z > 0.0 && (texColor1.x > 0.0 || texColor1.y > 0.0 || texColor1.z > 0.0)){
                                return Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
                            }
                        }
                    }
                }

                return 1.0;
            }
            ENDCG
        }
    }
}
