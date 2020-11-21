Texture2D lightSources;
sampler LightSourceSampler: register(s0);

Texture2D previousLightMap;
sampler PreviousLightMapSampler: register(s1);

Texture2D occulsionMap;
sampler OcculsionMapSampler: register(s2);

float horizontalPixel;
float verticalPixel;
float2 offsetFromPrevious;

float4 main(float2 texCoord : TEXCOORD0) : COLOR0
{
	//return float4(texCoord.x, texCoord.y, 0, 1);
	//return float4(1, 1, 1, 1) * tex2D(OcculsionMapSampler, texCoord).r;
	//return tex2D(LightSourceSampler, texCoord);
	
	float3 result = tex2D(LightSourceSampler, texCoord).rgb;

	float2 texCoordForPreviousLightMap = texCoord + offsetFromPrevious;
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(horizontalPixel, 0)).rgb, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(-horizontalPixel, 0)).rgb, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(0, verticalPixel)).rgb, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(0, -verticalPixel)).rgb, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(horizontalPixel, verticalPixel)).rgb * 0.99, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(-horizontalPixel, verticalPixel)).rgb * 0.99, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(horizontalPixel, -verticalPixel)).rgb * 0.99, result);
	result = max(tex2D(PreviousLightMapSampler, texCoordForPreviousLightMap + float2(-horizontalPixel, -verticalPixel)).rgb * 0.99, result);

	return float4((result * tex2D(OcculsionMapSampler, texCoord).r) - 0.01, 1);
}

technique LightSpreadEffect
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}