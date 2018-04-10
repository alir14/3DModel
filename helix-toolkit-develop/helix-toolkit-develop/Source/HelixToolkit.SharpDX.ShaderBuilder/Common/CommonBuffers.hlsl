#ifndef CBUFFERS_HLSL
#define CBUFFERS_HLSL
#pragma pack_matrix( row_major )
#include"DataStructs.hlsl"


///------------------Constant Buffers-----------------------
//--------------------------------------------------------------------------------------
// Perframe Buffers
//--------------------------------------------------------------------------------------
cbuffer cbTransforms : register(b0)
{
    float4x4 mView;
    float4x4 mProjection;
    float4x4 mViewProjection;
	// camera frustum: 
	// [fov,asepct-ratio,near,far]
    float4 vFrustum;
	// viewport:
	// [w,h,0,0]
    float4 vViewport;
	// camera position
    float3 vEyePos;
    float padding0;
};

#if defined(MESH)
//Per model
cbuffer cbMeshModel : register(b1)
{
    float4x4 mWorld;
    bool bInvertNormal = false;
    bool bHasInstances = false;
    bool bHasInstanceParams = false;
    bool bHasBones = false;
    float4 vParams = float4(0, 0, 0, 0); //Shared with models
    float4 vColor = float4(1, 1, 1, 1); //Shared with models
    bool4 bParams = bool4(false, false, false, false); // Shared with models for enable/disable features
	float minTessDistance = 1;
	float maxTessDistance = 100;
	float minTessFactor = 4;
	float maxTessFactor = 1;
    float4 vMaterialAmbient = 0.25f; //Ka := surface material's ambient coefficient
    float4 vMaterialDiffuse = 0.5f; //Kd := surface material's diffuse coefficient
    float4 vMaterialEmissive = 0.0f; //Ke := surface material's emissive coefficient
    float4 vMaterialSpecular = 0.0f; //Ks := surface material's specular coefficient
    float4 vMaterialReflect = 0.0f; //Kr := surface material's reflectivity coefficient
    float sMaterialShininess = 1.0f; //Ps := surface material's shininess
	
    bool bHasDiffuseMap = false;
    bool bHasAlphaMap = false;
    bool bHasNormalMap = false;
    bool bHasDisplacementMap = false;
    bool bHasCubeMap = false;
    bool bRenderShadowMap = false;
    float paddingMaterial0;
    float4 displacementMapScaleMask = float4(0, 0, 0, 1);
    float4 wireframeColor = float4(0,0,1,1);
};
#endif

#if defined(SCREENDUPLICATION)
    cbuffer cbScreenClone : register(b9)
    {
        float4 VertCoord[4];
        float4 TextureCoord[4];
        float4 CursorVertCoord[4];
    };
#endif

#define MaxBones 128

static const int4 minBoneV = { 0, 0, 0, 0 };
static const int4 maxBoneV = { MaxBones - 1, MaxBones - 1, MaxBones - 1, MaxBones - 1 };

cbuffer cbBoneSkinning : register(b2)
{
    matrix skinMatrices[MaxBones];
};

cbuffer cbLights : register(b3)
{
    LightStruct Lights[LIGHTS];
    float4 vLightAmbient = float4(0.2f, 0.2f, 0.2f, 1.0f);
    int NumLights;
    float3 padding;
};

#if defined(POINTLINE) // model for line, point and billboard
//Per model
cbuffer cbPointLineModel : register(b4)
{
    float4x4 pWorld;
    bool pHasInstances = false;
    bool pHasInstanceParams = false;
	float2 padding1;
    float4 pfParams = float4(0, 0, 0, 0); //Shared with line, points and billboard
    float4 pColor = float4(1, 1, 1, 1); //Shared with line, points and billboard
	bool4 pbParams = bool4(false, false, false, false);
};
#endif

cbuffer cbShadow : register(b5)
{
    float2 vShadowMapSize = float2(1024, 1024);
    bool bHasShadowMap = false;
    float paddingShadow0;
    float4 vShadowMapInfo = float4(0.005, 1.0, 0.5, 0.0);
    float4x4 vLightViewProjection;
};
#if defined(CLIPPLANE)
cbuffer cbClipping : register(b6)
{
    bool4 EnableCrossPlane;
    float4 CrossSectionColors;
	// Format:
	// M00M01M02 PlaneNormal1 M03 Plane1 Distance to origin
	// M10M11M12 PlaneNormal2 M13 Plane2 Distance to origin
	// M20M21M22 PlaneNormal3 M23 Plane3 Distance to origin
	// M30M31M32 PlaneNormal4 M33 Plane4 Distance to origin
    float4x4 CrossPlaneParams;
}
#endif

#if defined(BORDEREFFECTS)

cbuffer cbBorderEffect : register(b6)
{
    float4 Color;
    float4x4 Param;
};
#endif

#if defined(PARTICLE)
cbuffer cbParticleFrame : register(b7)
{
    uint NumParticles;
    float3 ExtraAccelation;

    float TimeFactors;
    float3 DomainBoundsMax;

    float3 DomainBoundsMin;
    uint CumulateAtBound;

    float3 ConsumerLocation;
    float ConsumerGravity;

    float ConsumerRadius;
    float3 RandomVector;

    uint RandomSeed;
    uint NumTexCol;
    uint NumTexRow;
    bool AnimateByEnergyLevel;
    float2 ParticleSize;
    float Turbulance;
    float pad0;
};

cbuffer cbParticleCreateParameters : register(b8)
{
    float3 EmitterLocation;
    float InitialEnergy;

    float EmitterRadius;
    float2 pad2;
    float InitialVelocity;

    float4 ParticleBlendColor;

    float EnergyDissipationRate; //Energy dissipation rate per second
    float3 InitialAcceleration;
};
#endif
///------------------Textures---------------------
Texture2D texDiffuseMap : register(t0);
Texture2D texAlphaMap : register(t1);
Texture2D texNormalMap : register(t2);
Texture2D texDisplacementMap : register(t3);
TextureCube texCubeMap : register(t4);
Texture2D texShadowMap : register(t5);

Texture2D texParticle : register(t0);
StructuredBuffer<Particle> SimulationState : register(t0);
Texture2D billboardTexture : register(t0);; // billboard text image

///------------------Samplers-------------------
SamplerState samplerDiffuse : register(s0);

SamplerState samplerAlpha : register(s1);

SamplerState samplerNormal : register(s2);

SamplerState samplerDisplace : register(s3);

SamplerState samplerCube : register(s4);

SamplerComparisonState samplerShadow : register(s5);

SamplerState samplerParticle : register(s6);

SamplerState samplerBillboard : register(s7);

///---------------------UAV-----------------------------

ConsumeStructuredBuffer<Particle> CurrentSimulationState : register(u0);
AppendStructuredBuffer<Particle> NewSimulationState : register(u1);


#endif