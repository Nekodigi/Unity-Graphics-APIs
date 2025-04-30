#ifndef DI_PARTICLE_INCLUDED
#define DI_PARTICLE_INCLUDED

struct SParticle
{
    //basic simulation
    float3 position;
    float3 velocity;
    float age;
    float lifetime;
    int alive;
    float size;
    //advanced simulation
    float mass;
    float3 direction;
    float3 angle;
    float3 angularVelocity;
    float3 oldPosition;
    float3 targetPosition;
    //rendering
    float3 color;
    float alpha;
    float renderingSize;
    float3 scale;
    float3 pivot;
    float texIndex;
    float3 axisX;
    float3 axisY;
    float3 axisZ;
    //system
    uint particleId;
    uint seed;
    float spawnCount;
    float spawnTime;
    uint particleIndexInStrip;
};


#endif
