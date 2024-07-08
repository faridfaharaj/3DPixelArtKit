TEXTURE2D(_CameraColorTexture);
SAMPLER(sampler_CameraColorTexture);
float4 _CameraColorTexture_TexelSize;

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_CameraDepthNormalsTexture);
SAMPLER(sampler_CameraDepthNormalsTexture);
 
float3 DecodeNormal(float4 enc)
{
    float kScale = 1.7777;
    float3 nn = enc.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
    float g = 2.0 / dot(nn.xyz,nn.xyz);
    float3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}

float maxFour(float a, float b, float c, float d)
    {
        return max(a, max(b,max(c, d)));
    }

float minFour(float a, float b, float c, float d)
    {
        return min(a, min(b,min(c, d)));
    }

float3 midpoint(float3 a, float3 b)
    {
        return a-b;
    }

void Outline_float(float2 UV, float DepthSensitivity, float NormalsSensitivity, float NormalDepth, float ColorSensitivity, float4 DepthOutlineColor,  float4 NormalOutlineColor, float testIn, out float4 Out)
{
    float2 Texel = (1.0) / float2(_CameraColorTexture_TexelSize.z, _CameraColorTexture_TexelSize.w);

    float2 uvSamples[5];
    float depthSamples[5];
    float3 normalSamples[5], colorSamples[5];

    uvSamples[0] = UV;
    uvSamples[1] = UV + float2(Texel.x, 0);
    uvSamples[2] = UV + float2(0, Texel.y);
    uvSamples[3] = UV + float2(-Texel.x, 0);
    uvSamples[4] = UV + float2(0, -Texel.y);

    for(int i = 0; i < 5 ; i++)
    {
        depthSamples[i] = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvSamples[i]).r;
        normalSamples[i] = DecodeNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uvSamples[i]));
        colorSamples[i] = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uvSamples[i]);
    }

    // Depth
    float depthFiniteDifference0 = depthSamples[0] - depthSamples[1];
    float depthFiniteDifference1 = depthSamples[0] - depthSamples[2];
    float depthFiniteDifference2 = depthSamples[0] - depthSamples[3];
    float depthFiniteDifference3 = depthSamples[0] - depthSamples[4];
    float edgeDepth = maxFour(depthFiniteDifference0,depthFiniteDifference1,depthFiniteDifference2,depthFiniteDifference3) * 100;
    float depthThreshold = (1/DepthSensitivity) * depthSamples[0];
    edgeDepth = clamp(floor(edgeDepth/depthThreshold), 0, 1);

    // Normals
    float3 normalFiniteDifference0 = dot(normalSamples[0] - normalSamples[1], normalSamples[0] - normalSamples[1]);
    float3 normalFiniteDifference1 = dot(normalSamples[0] - normalSamples[2], normalSamples[0] - normalSamples[2]);
    float3 normalFiniteDifference2 = dot(normalSamples[0] - normalSamples[3], normalSamples[0] - normalSamples[3]);
    float3 normalFiniteDifference3 = dot(normalSamples[0] - normalSamples[4], normalSamples[0] - normalSamples[4]);
    float edgeNormal = minFour(normalSamples[1].y, normalSamples[2].y, normalSamples[3].y, normalSamples[4].y) < normalSamples[0].y ? sqrt(normalFiniteDifference0+normalFiniteDifference1+normalFiniteDifference2+normalFiniteDifference3) : 0;

                  //convex normals deletion
    edgeNormal = midpoint(normalSamples[0], normalSamples[2]).y + midpoint(normalSamples[0], normalSamples[1]).x + midpoint(normalSamples[3], normalSamples[0]).x + midpoint(normalSamples[4], normalSamples[0]).y < 0.02 ? edgeNormal : 0;

    edgeNormal = clamp(floor(edgeNormal*NormalsSensitivity), 0, 1);
    edgeNormal -= edgeNormal * clamp(floor(maxFour(abs(depthFiniteDifference2), abs(depthFiniteDifference3), abs(depthFiniteDifference0), abs(depthFiniteDifference1))/(NormalDepth/1000)), 0, 1);

    //edge merge
    float edge = max(edgeDepth, edgeNormal);
    float cleanNormal = clamp(edgeNormal - edgeDepth * 100, 0, 1);
    float4 original = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uvSamples[0]);	

    Out = ((1 - edge) * original) + (cleanNormal * lerp(original, NormalOutlineColor*4*(original),  NormalOutlineColor.a)) + (edgeDepth * lerp(original, DepthOutlineColor * 2 * original,  DepthOutlineColor.a));

}