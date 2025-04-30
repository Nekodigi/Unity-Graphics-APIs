//based on https://github.com/iweinbau/UnityGPUDynamicHashGrid/blob/main/Runtime/Compute/DynamicGridHashingUtils.hlsl
#ifndef DI_HASH_GRID_INCLUDED
#define DI_HASH_GRID_INCLUDED

#define UINT32_MAX 0xFFFFFFFF

RWStructuredBuffer<uint> _IndexBuffer;
RWStructuredBuffer<uint> _CellIndexBuffer;
RWStructuredBuffer<uint> _OffsetsBuffer;

float _SearchGridCellSize;

int3 CellIndex(float3 pos, float radius)
{
    return floor(pos / radius);
}

uint GetFlatCellIndex(int3 cellIndex, uint tableSize)
{
    const uint p1 = 73856093; // some large primes
    const uint p2 = 19349663;
    const uint p3 = 83492791;
    int n = p1 * cellIndex.x ^ p2 * cellIndex.y ^ p3 * cellIndex.z;
    return n % tableSize;
}

#endif // RANDOM_INCLUDED
