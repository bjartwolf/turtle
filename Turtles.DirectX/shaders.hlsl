struct PSInput
{
	float4 position : SV_POSITION;
};

PSInput VSMain(float4 position : POSITION)
{
	PSInput result;

	result.position = float4(0.0f,0.0f,0.0f,0.0f); 
	return result;
}
