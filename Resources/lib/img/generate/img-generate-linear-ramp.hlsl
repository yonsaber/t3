cbuffer ParamConstants : register(b0)
{
    float2 Center;
    float Width;
    float Rotation;
    float PingPong;
    float Repeat;
    float Bias;
    float Offset;
    float SizeMode;
    float IsTextureValid;
}

cbuffer Resolution : register(b1)
{
    float TargetWidth;
    float TargetHeight;
}

struct vsOutput
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD;
};

Texture2D<float4> ImageA : register(t0);
Texture2D<float4> Gradient : register(t1);

sampler texSampler : register(s0);
sampler clampedSampler : register(s1);

float fmod(float x, float y)
{
    return (x - y * floor(x / y));
}

float4 psMain(vsOutput psInput) : SV_TARGET
{
    float2 uv = psInput.texCoord;

    float aspectRation = TargetWidth / TargetHeight;
    float2 p = uv;
    p -= 0.5;

    if (SizeMode < 0.5)
    {
        p.x *= aspectRation;
    }
    else
    {
        p.y /= aspectRation;
    }

    float radians = Rotation / 180 * 3.141578;
    float2 angle = float2(sin(radians), cos(radians));

    float c = dot(p - Center, angle);
    c += Offset;
    c = PingPong > 0.5
            ? (Repeat < 0.5 ? (abs(c) / Width)
                            : 1 - abs(fmod(c, Width * 2) - Width) / Width)
            : c / Width + 0.5;

    c = Repeat > 0.5
            ? fmod(c, 1)
            : saturate(c);

    float dBiased = Bias >= 0
                        ? pow(c, Bias + 1)
                        : 1 - pow(clamp(1 - c, 0, 10), -Bias + 1);

    dBiased = clamp(dBiased, 0.001, 0.999);

    float4 gradient = Gradient.Sample(clampedSampler, float2(dBiased, 0));

    if (IsTextureValid < 0.5)
        return gradient;

    float4 orgColor = ImageA.Sample(texSampler, psInput.texCoord);
    return float4((1.0 - gradient.a) * orgColor.rgb + gradient.a * gradient.rgb,
                  orgColor.a + gradient.a - orgColor.a * gradient.a);
}