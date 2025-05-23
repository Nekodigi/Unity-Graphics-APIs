#include "MarchTables.hlsl"


struct Triangle
{
    float3 vertexC;
    float3 vertexB;
    float3 vertexA;
};

int numPointsPerAxis;
float isoLevel;

float3 interpolateVerts(float4 v1, float4 v2)
{
    float t = (isoLevel - v1.w) / (v2.w - v1.w);
    return v1.xyz + t * (v2.xyz - v1.xyz);
}

int indexFromCoord(int x, int y, int z)
{
    return z * numPointsPerAxis * numPointsPerAxis + y * numPointsPerAxis + x;
}

int GetCubeIndex(float4 cubeCorners[8])
{
    int cubeIndex = 0;
    if (cubeCorners[0].w < isoLevel) cubeIndex |= 1;
    if (cubeCorners[1].w < isoLevel) cubeIndex |= 2;
    if (cubeCorners[2].w < isoLevel) cubeIndex |= 4;
    if (cubeCorners[3].w < isoLevel) cubeIndex |= 8;
    if (cubeCorners[4].w < isoLevel) cubeIndex |= 16;
    if (cubeCorners[5].w < isoLevel) cubeIndex |= 32;
    if (cubeCorners[6].w < isoLevel) cubeIndex |= 64;
    if (cubeCorners[7].w < isoLevel) cubeIndex |= 128;
    return cubeIndex;
}
