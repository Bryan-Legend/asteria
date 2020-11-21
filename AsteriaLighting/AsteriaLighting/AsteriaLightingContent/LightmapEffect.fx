texture2D Texture1;
sampler Texture1Sampler		: register(s0)  = sampler_state { Texture = (Texture1); };
texture2D Texture2;
sampler Texture2Sampler		: register(s1)  = sampler_state { Texture = (Texture2); };

float4 LightmapPixelShader(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 finalColor = 0;
	float4 tex1 = tex2D(Texture1Sampler, texCoord);
	float4 tex2 = tex2D(Texture2Sampler, texCoord);

	finalColor.rgb = tex1.rgb * tex2.rgb * 2;
	finalColor.a = tex1.a * tex2.a;

	return finalColor;
}

technique Lightmap
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 LightmapPixelShader();
    }
}
