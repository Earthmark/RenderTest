float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexInputType
{
	float4 position : POSITION;
	float4 color : COLOR;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

PixelInputType ColorVertexShader(VertexInputType input)
{
	PixelInputType output;

	output.position = mul(mul(mul(input.position, World), View), Projection);

	output.color = input.color;

	return output;
}

float4 ColorPixelShader(PixelInputType input) : SV_Target
{
	return input.color;
}

technique10 ColorShader
{
	pass Pass1
	{
		SetVertexShader(CompileShader(vs_4_0, ColorVertexShader()));
		SetPixelShader(CompileShader(ps_4_0, ColorPixelShader()));
	}
}