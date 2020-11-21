


Texture2D xTexture1 : register(t0);
Texture2D xTexture2 : register(t1);

sampler TextureSampler1 = sampler_state { texture = <xTexture1> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
sampler TextureSampler2 = sampler_state { texture = <xTexture2> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

float xBlendAmount;

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	// I've changed this a bit, we're going to keep everythign pre-multi'd
	color.rgba = tex2D(TextureSampler1, texCoord);

	// do the same with the second texture.
	float4 color2 = tex2D(TextureSampler2, texCoord);

	// mix just the diffuse portion of the textures
	color.rgb = lerp(color, color2, 1-xBlendAmount);
    
	return color;
}


technique BlendEffect
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}