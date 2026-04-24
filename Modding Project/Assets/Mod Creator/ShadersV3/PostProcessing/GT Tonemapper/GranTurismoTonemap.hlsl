
float w(float x, float e0, float e1)
{
    if(x<= e0){ return 0; }
    if(x>= e1){ return 1; }
    return (x - e0) / (e1 - e0) * (x - e0) / (e1 - e0) * (3 - 2 * (x - e0) / (e1 - e0));
}

float h(float x, float e0, float e1)
{
    if(x<= e0){ return 0; }
    if(x>= e1){ return 1; }
    return (x-e0)/(e1-e0);
}

float GtTonemap(float P, float a, float m, float l, float c, float b, float x)
{
    x = max(0, x);
    
    //Linear Region
    float l0 = ((P - m) * l) / a;
    // Unused
    //float L0 = m - (m / a);
    //float L1 = m + ((1.0 - m) / a);
    float Lx = m + a * (x - m);
    
    //Toe
    float Tx = m * pow(x / m, c) + b;
    
    //Shoulder
    float S0 = m + l0;
    float S1 = m + a*l0;

    float C2 = (a*P) / (P - S1);
    float Sexp = -(C2 * (x - S0) / P);
    float Sx = P - (P - S1) * exp(Sexp); 
    
    //toe weight
    float w0x = 1 - w(x, 0, m);
    
    //shoulder weight
    float w2x = h(x, m + l0, m + l0);
    
    //linear weight
    float w1x = 1 - w0x - w2x;

    return Tx*w0x+Lx*w1x+Sx*w2x;
}

//MaxBright = P
//Contrast = a
//LinearStart = m
//LinearLength = l
//BlackTightness = c
//BlackTightnessB = b
//InputColor = x
void GranTurismoTonemap_float(float3 InputColor, float MaxBrightness,  float Contrast, float LinearStart,
    float LinearLength, float BlackTightness, float BlackTightnessB,  out float3 Color)
{
    InputColor = max(0, InputColor);
    Color.r = GtTonemap(MaxBrightness, Contrast, LinearStart, LinearLength, BlackTightness, BlackTightnessB, InputColor.r);
    Color.g = GtTonemap(MaxBrightness, Contrast, LinearStart, LinearLength, BlackTightness, BlackTightnessB, InputColor.g);
    Color.b = GtTonemap(MaxBrightness, Contrast, LinearStart, LinearLength, BlackTightness, BlackTightnessB, InputColor.b);
}
