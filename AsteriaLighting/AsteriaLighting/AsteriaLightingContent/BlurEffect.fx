texture2D Texture;
sampler TextureSampler		: register(s0)  = sampler_state { Texture = (Texture); };

float hPixel;
float vPixel;

float4 HorizontalPixelShader(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 sum = 0;	

	sum += tex2D(TextureSampler, float2(texCoord.x - (4.0 * hPixel), texCoord.y)) * 0.05;
	sum += tex2D(TextureSampler, float2(texCoord.x - (3.0 * hPixel), texCoord.y)) * 0.09;
	sum += tex2D(TextureSampler, float2(texCoord.x - (2.0 * hPixel), texCoord.y)) * 0.12;
	sum += tex2D(TextureSampler, float2(texCoord.x - (1.0 * hPixel), texCoord.y)) * 0.15;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y)) * 0.16;
	sum += tex2D(TextureSampler, float2(texCoord.x + (1.0 * hPixel), texCoord.y)) * 0.15;
	sum += tex2D(TextureSampler, float2(texCoord.x + (2.0 * hPixel), texCoord.y)) * 0.12;
	sum += tex2D(TextureSampler, float2(texCoord.x + (3.0 * hPixel), texCoord.y)) * 0.09;
	sum += tex2D(TextureSampler, float2(texCoord.x + (4.0 * hPixel), texCoord.y)) * 0.05;

	return sum * color;	 
}

float4 VerticalPixelShader(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 sum = 0;

	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (4.0 * vPixel))) * 0.05;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (3.0 * hPixel))) * 0.09;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (2.0 * hPixel))) * 0.12;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (1.0 * hPixel))) * 0.15;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y)) * 0.16;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (1.0 * hPixel))) * 0.15;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (2.0 * hPixel))) * 0.12;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (3.0 * hPixel))) * 0.09;
	sum += tex2D(TextureSampler, float2(texCoord.x, texCoord.y - (4.0 * hPixel))) * 0.05;

	return sum * color;	 
}

technique Blur
{
    pass Horizontal
    {
        PixelShader = compile ps_2_0 HorizontalPixelShader();
    }

    pass Vertical
    {
        PixelShader = compile ps_2_0 VerticalPixelShader();
    }
}
