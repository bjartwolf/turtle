cbuffer ConstantBuffer : register(b0)
{
	float4 offset;
};

struct PSInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

PSInput VSMain(float4 position : POSITION, float4 color : COLOR)
{
	PSInput result;

	result.position = position + offset;
	result.color.yzw = color.yzw;
	result.color.x = abs(offset * 1.01f); 

	if (result.position.x > 0.4f && result.position.y > 0.4f) {
		result.position.x  = 1.7f;
		result.position.y  = 1.7f;
	} else 
	{
		result.position.y = position.y + offset; 
	}

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}